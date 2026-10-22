namespace Axis.Domain.Enums;

/// <summary>
/// 租户开通状态（独立库场景用于建库/InitTables 结果；Id 隔离默认 Ready）。
/// </summary>
public enum TenantProvisionStatusEnum
{
    /// <summary>
    /// 待开通（独立库尚未完成建库）。
    /// </summary>
    Pending = 0,

    /// <summary>
    /// 已就绪，可登录使用。
    /// </summary>
    Ready = 1,

    /// <summary>
    /// 开通失败。
    /// </summary>
    Failed = 2,
}
