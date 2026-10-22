using Axis.Application.Common;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Application.System.CodeRule;
using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;

namespace Axis.Application.Mdm.Routing;

/// <summary>
/// 工艺路线应用服务。
/// </summary>
/// <remarks>
/// 头行同事务保存；更新时先删后插工序行。HTTP 事务由 Endpoint <c>Transactional</c> 覆盖。
/// </remarks>
public sealed class RoutingAppService(
    IRepository<MdmRouting> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ICodeGenerator codeGenerator,
    IMapper mapper)
    : BaseAppService<MdmRouting>(repository, db, currentAccount), IRoutingAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询工艺路线头，投影为 <see cref="RoutingDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>路线 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<RoutingDto>> PageAsync(RoutingPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new RoutingDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                StepCount = SqlFunc.Subqueryable<MdmRoutingStep>()
                    .Where(step => step.RoutingId == item.Id)
                    .Count(),
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 获取工艺路线详情（含工序行）。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>含工序行的路线。</returns>
    public async Task<RoutingDto> GetDetailAsync(long id, CancellationToken ct = default)
    {
        var item = await Repo.AsQueryable().Where(row => row.Id == id).FirstAsync(ct)
                   ?? throw new BusinessException("工艺路线不存在");
        var steps = await LoadStepsAsync(id, ct);
        return new RoutingDto
        {
            Id = item.Id,
            Name = item.Name,
            Code = item.Code,
            StepCount = steps.Count,
            OrderNo = item.OrderNo,
            Status = item.Status,
            Remark = item.Remark,
            Steps = steps,
            CreatedTime = item.CreatedTime,
            ModifiedTime = item.ModifiedTime
        };
    }

    /// <summary>
    /// 预览路线下一编码；只读流水，不占用。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    public async Task<RoutingNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default)
    {
        RequireTenant();
        var preview = await codeGenerator.PreviewAsync(MdmCodeRuleCodes.Routing, ct: ct);
        return new RoutingNextCodeDto { Code = preview.Sample };
    }

    /// <summary>
    /// 创建工艺路线及工序行；编码留空则占用 RTG 流水。
    /// </summary>
    /// <param name="input">头与行。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task CreateAsync(SaveRoutingInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        await EnsureProcessesAsync(input.Steps, ct);
        var code = await ResolveNewCodeAsync(input.Code, ct);
        await EnsureCodeUniqueAsync(code, tenantId, null, ct);

        var entity = mapper.Map<MdmRouting>(input);
        entity.Id = 0;
        entity.TenantId = tenantId;
        entity.Code = code;
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
        await ReplaceStepsAsync(entity.Id, tenantId, input.Steps, ct);
    }

    /// <summary>
    /// 更新工艺路线；先删后插工序行。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">头与行。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task UpdateAsync(long id, SaveRoutingInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("工艺路线不存在");
        await EnsureProcessesAsync(input.Steps, ct);
        var code = string.IsNullOrWhiteSpace(input.Code) ? entity.Code : input.Code.Trim();
        await EnsureCodeUniqueAsync(code, tenantId, id, ct);

        mapper.Map(input, entity);
        entity.Code = code;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
        await ReplaceStepsAsync(id, tenantId, input.Steps, ct);
    }

    /// <summary>
    /// 删除工艺路线及其工序行。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        RequireTenant();
        await Db.Deleteable<MdmRoutingStep>().Where(item => item.RoutingId == id).ExecuteCommandAsync(ct);
        var affected = await Db.Deleteable<MdmRouting>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("工艺路线不存在");
    }

    /// <summary>
    /// 路线为租户强隔离数据，必须绑定当前租户。
    /// </summary>
    /// <returns>当前租户 Id。</returns>
    private long RequireTenant()
        => CurrentAccount.TenantId
           ?? throw new BusinessException("当前账号未绑定租户，不能维护工艺路线");

    /// <summary>
    /// 手填编码则去空白；否则占用 RTG 流水。
    /// </summary>
    /// <param name="code">用户输入编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>最终落库编码。</returns>
    private async Task<string> ResolveNewCodeAsync(string? code, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(code))
            return code.Trim();

        var generated = await codeGenerator.NextAsync(MdmCodeRuleCodes.Routing, ct: ct);
        return generated.Value;
    }

    /// <summary>
    /// 校验编码在指定租户内唯一。
    /// </summary>
    /// <param name="code">路线编码。</param>
    /// <param name="tenantId">租户 Id。</param>
    /// <param name="excludedId">更新时排除的主键。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureCodeUniqueAsync(string code, long tenantId, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code && item.TenantId == tenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"路线编码 '{code}' 已存在");
    }

    /// <summary>
    /// 校验工序均存在且启用。
    /// </summary>
    /// <param name="steps">待保存工序行。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureProcessesAsync(IReadOnlyList<SaveRoutingStepInput> steps, CancellationToken ct)
    {
        var processIds = steps.Select(item => item.ProcessId).Distinct().ToList();
        var enabledIds = await Db.Queryable<MdmProcess>()
            .Where(item => processIds.Contains(item.Id) && item.Status == StatusEnum.Enable)
            .Select(item => item.Id)
            .ToListAsync(ct);
        if (enabledIds.Count != processIds.Count)
            throw new BusinessException("存在无效或已停用的工序");
    }

    /// <summary>
    /// 清空并重写指定路线的工序行。
    /// </summary>
    /// <param name="routingId">路线主键。</param>
    /// <param name="tenantId">租户 Id。</param>
    /// <param name="steps">新工序行。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task ReplaceStepsAsync(
        long routingId,
        long tenantId,
        IReadOnlyList<SaveRoutingStepInput> steps,
        CancellationToken ct)
    {
        await Db.Deleteable<MdmRoutingStep>().Where(item => item.RoutingId == routingId).ExecuteCommandAsync(ct);
        var rows = steps
            .OrderBy(item => item.Seq)
            .Select(item => new MdmRoutingStep
            {
                Id = 0,
                TenantId = tenantId,
                RoutingId = routingId,
                ProcessId = item.ProcessId,
                Seq = item.Seq,
                StandardMinutes = item.StandardMinutes,
                IsReportPoint = item.IsReportPoint,
                Remark = item.Remark
            })
            .ToList();
        if (rows.Count > 0)
            await Db.Insertable(rows).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 加载路线工序行并带出工序编码、名称。
    /// </summary>
    /// <param name="routingId">路线主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>按序号排序的工序行。</returns>
    private async Task<List<RoutingStepDto>> LoadStepsAsync(long routingId, CancellationToken ct)
        => await Db.Queryable<MdmRoutingStep>()
            .LeftJoin<MdmProcess>((step, process) => step.ProcessId == process.Id)
            .Where((step, process) => step.RoutingId == routingId)
            .OrderBy((step, process) => step.Seq)
            .Select((step, process) => new RoutingStepDto
            {
                Id = step.Id,
                Seq = step.Seq,
                ProcessId = step.ProcessId,
                ProcessCode = process.Code,
                ProcessName = process.Name,
                StandardMinutes = step.StandardMinutes,
                IsReportPoint = step.IsReportPoint,
                Remark = step.Remark
            })
            .ToListAsync(ct);
}
