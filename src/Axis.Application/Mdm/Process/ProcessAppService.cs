using Axis.Application.Common;
using Axis.Application.Mdm.Process.Dto;
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

namespace Axis.Application.Mdm.Process;

/// <summary>
/// 工序管理应用服务。
/// </summary>
/// <remarks>
/// 编码留空走 <see cref="ICodeGenerator"/>（规则 <c>PRC</c>）；编码租户内唯一。
/// </remarks>
public sealed class ProcessAppService(
    IRepository<MdmProcess> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ICodeGenerator codeGenerator,
    IMapper mapper)
    : BaseAppService<MdmProcess>(repository, db, currentAccount), IProcessAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询工序，投影为 <see cref="ProcessDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>工序 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<ProcessDto>> PageAsync(ProcessPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new ProcessDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            }, true)
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 预览工序下一编码；只读流水，不占用。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    public async Task<ProcessNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default)
    {
        RequireTenant();
        var preview = await codeGenerator.PreviewAsync(MdmCodeRuleCodes.Process, ct: ct);
        return new ProcessNextCodeDto { Code = preview.Sample };
    }

    /// <summary>
    /// 创建工序；编码留空则占用 PRC 流水。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task CreateAsync(SaveProcessInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var code = await ResolveNewCodeAsync(input.Code, ct);
        await EnsureCodeUniqueAsync(code, tenantId, null, ct);

        var entity = mapper.Map<MdmProcess>(input);
        entity.Id = 0;
        entity.TenantId = tenantId;
        entity.Code = code;
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定工序；编码留空则保留原值。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task UpdateAsync(long id, SaveProcessInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("工序不存在");
        var code = string.IsNullOrWhiteSpace(input.Code) ? entity.Code : input.Code.Trim();
        await EnsureCodeUniqueAsync(code, tenantId, id, ct);

        mapper.Map(input, entity);
        entity.Code = code;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除工序。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var referenced = await Db.Queryable<MdmRoutingStep>().AnyAsync(item => item.ProcessId == id, ct);
        if (referenced) throw new BusinessException("工序已被工艺路线引用，不能删除");

        var affected = await Db.Deleteable<MdmProcess>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("工序不存在");
    }

    /// <summary>
    /// 查询启用中的工序，供工艺路线等下拉使用。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用工序的只读列表。</returns>
    public async Task<IReadOnlyList<ProcessOptionDto>> ListEnabledAsync(CancellationToken ct = default)
        => await Repo.AsQueryable()
            .Where(item => item.Status == StatusEnum.Enable)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.Code)
            .Select(item => new ProcessOptionDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name
            })
            .ToListAsync(ct);

    /// <summary>
    /// 工序为租户强隔离数据，必须绑定当前租户。
    /// </summary>
    /// <returns>当前租户 Id。</returns>
    private long RequireTenant()
        => CurrentAccount.TenantId
           ?? throw new BusinessException("当前账号未绑定租户，不能维护工序");

    /// <summary>
    /// 手填编码则去空白；否则占用 PRC 流水。
    /// </summary>
    /// <param name="code">用户输入编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>最终落库编码。</returns>
    private async Task<string> ResolveNewCodeAsync(string? code, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(code))
            return code.Trim();

        var generated = await codeGenerator.NextAsync(MdmCodeRuleCodes.Process, ct: ct);
        return generated.Value;
    }

    /// <summary>
    /// 校验编码在指定租户内唯一。
    /// </summary>
    /// <param name="code">工序编码。</param>
    /// <param name="tenantId">租户 Id。</param>
    /// <param name="excludedId">更新时排除的主键。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureCodeUniqueAsync(string code, long tenantId, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code && item.TenantId == tenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"工序编码 '{code}' 已存在");
    }
}
