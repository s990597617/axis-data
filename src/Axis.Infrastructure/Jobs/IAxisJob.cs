namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 可被调度执行的业务任务契约（代码注册，禁止反射任意类型）。
/// </summary>
public interface IAxisJob
{
    /// <summary>
    /// 稳定编码，与 <c>sys_job.job_code</c> 对齐。
    /// </summary>
    string Code { get; }

    /// <summary>
    /// 执行任务。
    /// </summary>
    /// <param name="context">执行上下文。</param>
    /// <param name="ct">取消令牌。</param>
    Task ExecuteAsync(AxisJobContext context, CancellationToken ct);
}
