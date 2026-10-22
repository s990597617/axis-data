namespace Axis.Infrastructure.Jobs;

/// <summary>
/// Quartz JobDataMap 键名约定。
/// </summary>
public static class AxisJobDataKeys
{
    public const string JobId = "jobId";
    public const string JobCode = "jobCode";
    public const string JobName = "jobName";
    public const string Params = "params";
    public const string IsManual = "isManual";
    public const string Group = "AXIS";
}
