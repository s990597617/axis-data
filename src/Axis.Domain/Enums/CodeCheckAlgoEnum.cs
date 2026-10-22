namespace Axis.Domain.Enums;

/// <summary>
/// 编码规则校验位算法。
/// </summary>
/// <remarks>
/// 与字典 <c>code_check_algo</c> 双轨对齐。已发布数值不得重排；新算法只能追加。
/// </remarks>
public enum CodeCheckAlgoEnum
{
    /// <summary>
    /// 未知或未指定；不得用于已启用规则。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 无校验位；模板不得包含 <c>{Check}</c>。
    /// </summary>
    None = 1,

    /// <summary>
    /// 对已渲染正文做十进制模 10，附加一位数字。
    /// </summary>
    Mod10 = 2
}
