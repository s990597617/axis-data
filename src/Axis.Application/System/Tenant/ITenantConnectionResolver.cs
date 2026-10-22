using Axis.Domain.System;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 按当前登录租户解析独立库 ConfigId（主库账号路由：TenantId → SysTenant.DbConfigId）。
/// </summary>
public interface ITenantConnectionResolver
{
    /// <summary>
    /// 解析租户对应的数据库 ConfigId；Id 隔离或未配置时返回主库。
    /// </summary>
    Task<string> ResolveConfigIdAsync(long? tenantId, CancellationToken ct = default);
}
