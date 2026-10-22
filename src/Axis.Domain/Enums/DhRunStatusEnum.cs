namespace Axis.Domain.Enums;

/// <summary>
/// DataHub 采集运行状态。
/// </summary>
public enum DhRunStatusEnum
{
    Pending = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5
}
