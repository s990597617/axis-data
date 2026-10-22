using System.Reflection;
using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 定时任务执行器目录：从 DI 收集全部 <see cref="IAxisJob"/>。
/// </summary>
[Injection(ServiceLifetime.Scoped, InjectAs.Self)]
public sealed class AxisJobCatalog(IEnumerable<IAxisJob> jobs) : IAutoInject
{
    private readonly IReadOnlyDictionary<string, IAxisJob> _map = jobs
        .GroupBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 已注册任务编码列表。
    /// </summary>
    public IReadOnlyCollection<string> Codes => _map.Keys.ToArray();

    /// <summary>
    /// 全部执行器描述（含未标注 Attribute 的实现，供 UI 下拉选择）。
    /// </summary>
    public IReadOnlyList<AxisJobExecutorDescriptor> ListExecutors()
        => _map.Values
            .Select(ToDescriptor)
            .OrderBy(item => item.OrderNo)
            .ThenBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// 带 <see cref="AxisJobAttribute"/> 的执行器（启动反向注册目标）。
    /// </summary>
    public IReadOnlyList<AxisJobExecutorDescriptor> ListSyncTargets()
        => ListExecutors().Where(item => item.HasMetadata).ToArray();

    /// <summary>
    /// 按编码解析执行器。
    /// </summary>
    public IAxisJob? Resolve(string jobCode)
        => string.IsNullOrWhiteSpace(jobCode)
            ? null
            : _map.GetValueOrDefault(jobCode.Trim());

    /// <summary>
    /// 是否已注册该编码。
    /// </summary>
    public bool Contains(string jobCode) => Resolve(jobCode) is not null;

    private static AxisJobExecutorDescriptor ToDescriptor(IAxisJob job)
    {
        var code = job.Code.Trim();
        var attr = job.GetType().GetCustomAttribute<AxisJobAttribute>();
        if (attr is null)
        {
            return new AxisJobExecutorDescriptor(
                code,
                code,
                null,
                null,
                Concurrent: false,
                DefaultParams: null,
                Builtin: false,
                OrderNo: 100,
                Enabled: false,
                Remark: null,
                HasMetadata: false);
        }

        return new AxisJobExecutorDescriptor(
            code,
            string.IsNullOrWhiteSpace(attr.Name) ? code : attr.Name.Trim(),
            string.IsNullOrWhiteSpace(attr.GroupName) ? null : attr.GroupName.Trim(),
            string.IsNullOrWhiteSpace(attr.Cron) ? null : attr.Cron.Trim(),
            attr.Concurrent,
            string.IsNullOrWhiteSpace(attr.DefaultParams) ? null : attr.DefaultParams.Trim(),
            attr.Builtin,
            attr.OrderNo,
            attr.Enabled,
            string.IsNullOrWhiteSpace(attr.Remark) ? null : attr.Remark.Trim(),
            HasMetadata: true);
    }
}
