namespace Axis.Domain.Enums;

/// <summary>
/// DataHub 数据源连通性测试状态。
/// </summary>
public enum DhConnectionTestStatusEnum
{
    /// <summary>
    /// 未测试。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 成功。
    /// </summary>
    Success = 1,

    /// <summary>
    /// 失败。
    /// </summary>
    Failed = 2
}