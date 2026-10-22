using Axis.Domain.System.Seeds;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Jobs;

namespace Axis.Host.Api.Jobs;

/// <summary>
/// Demo 心跳任务：验证调度链路。
/// </summary>
[AxisJob("Demo 心跳", "0 */5 * * * ?", GroupName = "demo", Builtin = true, Enabled = false, OrderNo = 1,
    Remark = "验证调度链路的示例任务，默认停用")]
public sealed class DemoHeartbeatJob(ILogger<DemoHeartbeatJob> logger) : IAxisJob, IAutoInject
{
    /// <inheritdoc />
    public string Code => SysJobSeed.HeartbeatJobCode;

    /// <inheritdoc />
    public Task ExecuteAsync(AxisJobContext context, CancellationToken ct)
    {
        logger.LogInformation(
            "Demo 心跳执行：JobId={JobId}, Manual={Manual}, Params={Params}",
            context.JobId,
            context.IsManual,
            context.Params ?? "-");
        return Task.CompletedTask;
    }
}