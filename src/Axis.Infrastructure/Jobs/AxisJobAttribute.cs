namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 标记 <see cref="IAxisJob"/> 实现的调度元数据，供启动时反向注册到 <c>sys_job</c>。
/// </summary>
/// <remarks>
/// 任务编码仍以 <see cref="IAxisJob.Code"/> 为唯一关联键；本特性只声明默认调度配置与展示信息。
/// 未标注本特性的执行器可被手动建任务引用，但不会自动入库。
/// </remarks>
/// <param name="name">任务显示名称。</param>
/// <param name="cron">默认 Cron 表达式（Quartz）。</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class AxisJobAttribute(string name, string cron) : Attribute
{
    /// <summary>
    /// 任务显示名称。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 默认 Cron 表达式。
    /// </summary>
    public string Cron { get; } = cron;

    /// <summary>
    /// 任务分组。
    /// </summary>
    public string? GroupName { get; init; }

    /// <summary>
    /// 默认是否允许并发。
    /// </summary>
    public bool Concurrent { get; init; }

    /// <summary>
    /// 默认参数 JSON。
    /// </summary>
    public string? DefaultParams { get; init; }

    /// <summary>
    /// 是否内置（内置禁止删除）。
    /// </summary>
    public bool Builtin { get; init; } = true;

    /// <summary>
    /// 默认排序。
    /// </summary>
    public int OrderNo { get; init; } = 100;

    /// <summary>
    /// 默认是否启用；未指定时为 <c>false</c>（停用）。
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; init; }
}
