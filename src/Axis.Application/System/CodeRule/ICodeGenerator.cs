using Axis.Application.System.CodeRule.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.CodeRule;

/// <summary>
/// 业务取号 / 预览 / 解析端口；无页面权限语义。
/// </summary>
/// <remarks>
/// 各业务模块只注入本接口，禁止自行查询 <c>sys_code_rule</c> 拼字符串。
/// </remarks>
public interface ICodeGenerator
{
    /// <summary>
    /// 按规则编码占用流水并返回完整编码。
    /// </summary>
    /// <param name="ruleCode">规则编码，须引用 <c>CodeRuleCodes</c> 常量。</param>
    /// <param name="context">取号上下文；Job 须显式带租户。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>完整编码与拆段。</returns>
    /// <exception cref="BusinessException">规则不存在、已禁用、外部来源或变量缺失时抛出。</exception>
    Task<CodeGenerateResult> NextAsync(
        string ruleCode,
        CodeGenerateContext? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// 按已保存规则预览下一号，不占用流水。
    /// </summary>
    /// <param name="ruleCode">规则编码。</param>
    /// <param name="context">预览上下文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览结果。</returns>
    Task<CodePreviewDto> PreviewAsync(
        string ruleCode,
        CodeGenerateContext? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// 按未保存表单预览编码并生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <param name="context">预览上下文。</param>
    /// <returns>预览结果。</returns>
    CodePreviewDto PreviewTemplate(SaveCodeRuleInput input, CodeGenerateContext? context = null);

    /// <summary>
    /// 按优先级匹配反向正则，解析扫码结果。
    /// </summary>
    /// <param name="code">待解析编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>命中的规则；无一匹配时为 null。</returns>
    Task<CodeParseResult?> ParseAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 校验外来码是否符合指定规则。
    /// </summary>
    /// <param name="ruleCode">规则编码。</param>
    /// <param name="code">待校验编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">规则不存在或编码不匹配时抛出。</exception>
    Task ValidateAsync(string ruleCode, string code, CancellationToken ct = default);
}
