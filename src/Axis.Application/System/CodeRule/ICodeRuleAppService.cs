using Axis.Application.System.CodeRule.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.CodeRule;

/// <summary>
/// 编码规则管理端用例。
/// </summary>
public interface ICodeRuleAppService
{
    /// <summary>
    /// 按条件分页查询编码规则。
    /// </summary>
    /// <param name="query">分页与筛选。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>规则 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<CodeRuleDto>> PageAsync(CodeRulePageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 校验编码唯一后创建规则。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveCodeRuleInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定规则；内置规则不允许改编码。
    /// </summary>
    /// <param name="id">规则主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在、内置改编码或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveCodeRuleInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除规则；内置规则不允许删除。
    /// </summary>
    /// <param name="id">规则主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或为内置规则时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 按未保存表单预览编码并生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <returns>预览结果。</returns>
    CodePreviewDto Preview(SaveCodeRuleInput input);

    /// <summary>
    /// 按当前模板生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <returns>正则文本。</returns>
    string GenerateRegex(SaveCodeRuleInput input);
}
