namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 已注册任务执行器描述（供管理端选择与启动同步）。
/// </summary>
/// <param name="Code">稳定编码，对应 <c>sys_job.job_code</c>。</param>
/// <param name="Name">显示名称。</param>
/// <param name="GroupName">默认分组。</param>
/// <param name="Cron">默认 Cron；无 Attribute 时可能为空。</param>
/// <param name="Concurrent">默认是否允许并发。</param>
/// <param name="DefaultParams">默认参数 JSON。</param>
/// <param name="Builtin">是否内置。</param>
/// <param name="OrderNo">默认排序。</param>
/// <param name="Enabled">默认是否启用。</param>
/// <param name="Remark">备注。</param>
/// <param name="HasMetadata">是否标注了 <see cref="AxisJobAttribute"/>（仅有元数据的才会自动入库）。</param>
public sealed record AxisJobExecutorDescriptor(
    string Code,
    string Name,
    string? GroupName,
    string? Cron,
    bool Concurrent,
    string? DefaultParams,
    bool Builtin,
    int OrderNo,
    bool Enabled,
    string? Remark,
    bool HasMetadata);
