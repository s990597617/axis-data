using Axis.Infrastructure.Author;
using Yitter.IdGenerator;

namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 为事件补齐 EventId / 租户 / 操作人。
/// </summary>
public static class AxisEventEnricher
{
    /// <summary>
    /// 缺省字段由当前账户或租户作用域填充。
    /// </summary>
    public static void Enrich(IAxisEvent @event, ICurrentAccount? account)
    {
        if (@event is not AxisEvent axis)
            return;

        if (axis.EventId == 0)
            axis.EventId = YitIdHelper.NextId();

        axis.TenantId ??= account?.TenantId ?? TenantScopeContext.Current?.TenantId;
        if (axis.OperatorId is null && account is { Id: > 0 })
            axis.OperatorId = account.Id;

        if (string.IsNullOrWhiteSpace(axis.CorrelationId))
            axis.CorrelationId = axis.EventId.ToString();
    }
}
