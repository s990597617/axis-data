using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Global;
using SqlSugar;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 租户连接解析：主库查询 <see cref="SysTenant"/>，独立库且已就绪时返回 DbConfigId。
/// </summary>
public sealed class TenantConnectionResolver(ISqlSugarClient db)
    : ITenantConnectionResolver, IAutoInject
{
    /// <inheritdoc />
    public async Task<string> ResolveConfigIdAsync(long? tenantId, CancellationToken ct = default)
    {
        if (!tenantId.HasValue)
            return SqlSugarConst.MainConfigId;

        var tenant = await db.Queryable<SysTenant>()
            .Where(item => item.Id == tenantId.Value)
            .Select(item => new { item.Type, item.DbConfigId, item.ProvisionStatus })
            .FirstAsync(ct);

        if (tenant is null)
            return SqlSugarConst.MainConfigId;

        if (tenant.Type != TenantTypeEnum.Db
            || string.IsNullOrWhiteSpace(tenant.DbConfigId)
            || tenant.ProvisionStatus != TenantProvisionStatusEnum.Ready)
            return SqlSugarConst.MainConfigId;

        var known = GlobalContext.BootstrapDb?.ConnectionConfigs?
            .Any(config => string.Equals(
                config.ConfigId?.ToString(),
                tenant.DbConfigId,
                StringComparison.OrdinalIgnoreCase)) == true;

        return known ? tenant.DbConfigId! : SqlSugarConst.MainConfigId;
    }
}
