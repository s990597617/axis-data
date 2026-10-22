namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 仅携带编码规则主键的路径参数。
/// </summary>
public sealed class CodeRuleIdRequest
{
    /// <summary>
    /// 规则主键。
    /// </summary>
    public long Id { get; set; }
}
