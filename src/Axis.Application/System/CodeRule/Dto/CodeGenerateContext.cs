namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 取号 / 预览时的运行上下文。
/// </summary>
public sealed class CodeGenerateContext
{
    /// <summary>
    /// 租户 Id；空则使用当前登录租户。Job 必须显式传入。
    /// </summary>
    public long? TenantId { get; init; }

    /// <summary>
    /// 取号时钟；空则使用服务器本地时间。
    /// </summary>
    public DateTime? Now { get; init; }

    /// <summary>
    /// 模板变量，键为占位符名（不含花括号），如 <c>Material</c>。
    /// </summary>
    public IReadOnlyDictionary<string, string> Variables { get; init; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 仅预览可用的流水覆盖；<c>NextAsync</c> 禁止使用。
    /// </summary>
    public long? SerialOverride { get; init; }
}
