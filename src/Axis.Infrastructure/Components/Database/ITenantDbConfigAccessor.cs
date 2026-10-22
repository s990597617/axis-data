namespace Axis.Infrastructure.Components.Database;

/// <summary>
/// 解析当前请求应使用的 SqlSugar 连接 ConfigId（独立库租户切换）。
/// </summary>
/// <remarks>
/// 以登录上下文 <c>TenantId</c> 为准，不信任请求头传入的租户标识。
/// Id 隔离或未开通独立库时返回 <c>master</c>。
/// </remarks>
public interface ITenantDbConfigAccessor
{
    /// <summary>
    /// 当前请求数据库 ConfigId；主库为 <c>master</c>。
    /// </summary>
    string GetConfigId();
}
