using Axis.Application.Common;
using Axis.Application.System.CodeRule.Dto;
using Axis.Application.System.CodeRule.Internal;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;

namespace Axis.Application.System.CodeRule;

/// <summary>
/// 编码规则管理应用服务。
/// </summary>
/// <remarks>
/// 写库成功后刷新规则缓存；业务取号走 <see cref="ICodeGenerator"/>。
/// </remarks>
public sealed class CodeRuleAppService(
    IRepository<SysCodeRule> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ISysCodeRuleCache ruleCache,
    ICodeGenerator codeGenerator,
    IMapper mapper)
    : BaseAppService<SysCodeRule>(repository, db, currentAccount), ICodeRuleAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询编码规则，投影为 <see cref="CodeRuleDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>规则 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<CodeRuleDto>> PageAsync(CodeRulePageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.SourceType.HasValue, item => item.SourceType == query.SourceType!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.Priority)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new CodeRuleDto
            {
                Id = item.Id,
                Code = item.Code,
                Name = item.Name,
                Prefix = item.Prefix,
                DateFormat = item.DateFormat,
                SerialLength = item.SerialLength,
                ResetCycle = item.ResetCycle,
                Template = item.Template,
                CheckAlgo = item.CheckAlgo,
                SourceType = item.SourceType,
                ReverseRegex = item.ReverseRegex,
                Priority = item.Priority,
                IsBuiltin = item.IsBuiltin,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            }, true)
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 校验编码唯一后创建规则；正则为空时按模板生成。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task CreateAsync(SaveCodeRuleInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        FillGeneratedRegex(input);
        var entity = mapper.Map<SysCodeRule>(input);
        entity.Id = 0;
        entity.TenantId = CurrentAccount.TenantId;
        entity.IsBuiltin = false;
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
        await ruleCache.RefreshAsync(input.Code, ct);
    }

    /// <summary>
    /// 更新指定规则；内置规则不允许改编码。
    /// </summary>
    /// <param name="id">规则主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task UpdateAsync(long id, SaveCodeRuleInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("编码规则不存在");
        if (entity.IsBuiltin && !string.Equals(entity.Code, input.Code, StringComparison.Ordinal))
            throw new BusinessException("内置规则不允许修改编码");

        await EnsureCodeUniqueAsync(input.Code, id, ct);
        FillGeneratedRegex(input);
        var oldCode = entity.Code;
        var builtin = entity.IsBuiltin;
        mapper.Map(input, entity);
        entity.IsBuiltin = builtin;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        if (!string.Equals(oldCode, input.Code, StringComparison.Ordinal))
            await ruleCache.InvalidateAsync(oldCode, ct);
        await ruleCache.RefreshAsync(input.Code, ct);
    }

    /// <summary>
    /// 删除规则；内置规则不允许删除。
    /// </summary>
    /// <param name="id">规则主键。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("编码规则不存在");
        if (entity.IsBuiltin) throw new BusinessException("内置规则不允许删除");
        await Db.Deleteable<SysCodeRule>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        await ruleCache.InvalidateAsync(entity.Code, ct);
    }

    /// <summary>
    /// 按未保存表单预览编码并生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <returns>预览结果。</returns>
    public CodePreviewDto Preview(SaveCodeRuleInput input)
        => codeGenerator.PreviewTemplate(input);

    /// <summary>
    /// 按当前模板生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <returns>正则文本。</returns>
    public string GenerateRegex(SaveCodeRuleInput input)
        => CodeTemplateEngine.BuildRegex(CodeTemplateEngine.ToSnapshot(input));

    /// <summary>
    /// 正则未填时按模板自动生成。
    /// </summary>
    private static void FillGeneratedRegex(SaveCodeRuleInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.ReverseRegex)) return;
        input.ReverseRegex = CodeTemplateEngine.BuildRegex(CodeTemplateEngine.ToSnapshot(input));
    }

    /// <summary>
    /// 校验同一租户下规则编码唯一。
    /// </summary>
    private async Task EnsureCodeUniqueAsync(string code, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"规则编码 '{code}' 已存在");
    }
}
