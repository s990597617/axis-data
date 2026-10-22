namespace Axis.Domain.Enums;

/// <summary>
/// 编码规则流水重置周期。
/// </summary>
/// <remarks>
/// 决定 <c>sys_code_serial.PeriodKey</c> 的分桶方式；发布后不得改已有成员数值。
/// 与字典 <c>code_reset_cycle</c> 双轨对齐（Value 为整数字符串）。
/// </remarks>
public enum CodeResetCycleEnum
{
    /// <summary>
    /// 未知或未指定；不得用于已启用规则。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 不重置；流水在规则生命周期内单调递增。
    /// </summary>
    Never = 1,

    /// <summary>
    /// 按自然日重置。
    /// </summary>
    Daily = 2,

    /// <summary>
    /// 按自然月重置。
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// 按自然年重置。
    /// </summary>
    Yearly = 4
}
