namespace Axis.Domain.System;

/// <summary>
/// 编码规则模板占位符名称。
/// </summary>
/// <remarks>
/// 引擎按 <c>{Name}</c> 解析；下列为第一期内置名。其它名称必须由调用方 <c>Variables</c> 提供。
/// </remarks>
public static class CodePlaceholderNames
{
    /// <summary>
    /// 规则前缀。
    /// </summary>
    public const string Prefix = "Prefix";

    /// <summary>
    /// 按规则日期格式渲染的时间片段。
    /// </summary>
    public const string Date = "Date";

    /// <summary>
    /// 左侧补零后的流水号。
    /// </summary>
    public const string Serial = "Serial";

    /// <summary>
    /// 物料编码（调用方上下文）。
    /// </summary>
    public const string Material = "Material";

    /// <summary>
    /// 产线编码（调用方上下文）。
    /// </summary>
    public const string Line = "Line";

    /// <summary>
    /// 班次（调用方上下文）。
    /// </summary>
    public const string Shift = "Shift";

    /// <summary>
    /// 校验位。
    /// </summary>
    public const string Check = "Check";

    /// <summary>
    /// 第一期管理端可插入的占位符列表（含花括号）。
    /// </summary>
    public static readonly string[] Tokens =
    [
        "{" + Prefix + "}",
        "{" + Date + "}",
        "{" + Serial + "}",
        "{" + Material + "}",
        "{" + Line + "}",
        "{" + Shift + "}",
        "{" + Check + "}"
    ];
}
