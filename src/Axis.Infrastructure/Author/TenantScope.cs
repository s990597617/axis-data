using Axis.Infrastructure.Components.Database.Permission;
using SqlSugar;

namespace Axis.Infrastructure.Author;

/// <summary>
/// 租户作用域默认实现：压入 AsyncLocal 后重挂 SqlSugar 租户过滤。
/// </summary>
public sealed class TenantScope(ISqlSugarClient db) : ITenantScope, IAutoInject
{
    /// <summary>
    /// 在指定租户下执行委托。
    /// </summary>
    public Task RunAsync(long? tenantId, Func<CancellationToken, Task> action, CancellationToken ct = default)
        => RunAsync(tenantId, async token =>
        {
            await action(token);
            return true;
        }, ct);

    /// <summary>
    /// 在指定租户下执行委托并返回结果。
    /// </summary>
    public async Task<T> RunAsync<T>(
        long? tenantId,
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        var state = tenantId is null
            ? new TenantScopeState { Unrestricted = true }
            : new TenantScopeState { TenantId = tenantId };

        using (TenantScopeContext.Push(state))
        {
            SqlSugarFilter.RebindTenantFilters(db);
            try
            {
                return await action(ct);
            }
            finally
            {
                SqlSugarFilter.RebindTenantFilters(db);
            }
        }
    }
}
