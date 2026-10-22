namespace Axis.Domain.Enums;

/// <summary>
/// 定时任务触发方式。
/// </summary>
public enum JobTriggerTypeEnum
{
    /// <summary>
    /// Cron 调度触发。
    /// </summary>
    Cron = 1,

    /// <summary>
    /// 管理端手动触发。
    /// </summary>
    Manual = 2
}
