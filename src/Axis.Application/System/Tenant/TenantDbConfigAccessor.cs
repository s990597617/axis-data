using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Dependency.Annotations;
using Microsoft.AspNetCore.Http;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 按当前登录租户解析独立库 ConfigId，并在请求内缓存。
/// </summary>
public sealed class TenantDbConfigAccessor(
    ICurrentAccount currentAccount,
    ITenantConnectionResolver connectionResolver,
    IHttpContextAccessor httpContextAccessor)
    : ITenantDbConfigAccessor, IAutoInject
{
    private const string HttpItemsKey = "Axis.TenantDbConfigId";

    /// <inheritdoc />
    public string GetConfigId()
    {
        var http = httpContextAccessor.HttpContext;
        if (http?.Items[HttpItemsKey] is string cached && !string.IsNullOrWhiteSpace(cached))
            return cached;

        // 构造期/后台任务可能无 HttpContext；未登录则主库
        if (currentAccount.Id <= 0 && currentAccount.TenantId is null)
            return SqlSugarConst.MainConfigId;

        var configId = connectionResolver
            .ResolveConfigIdAsync(currentAccount.TenantId)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        if (string.IsNullOrWhiteSpace(configId))
            configId = SqlSugarConst.MainConfigId;

        if (http is not null)
            http.Items[HttpItemsKey] = configId;

        return configId;
    }
}
