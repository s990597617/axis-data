using Axis.Domain.System;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 独立库租户开通：按配置建库并 InitTables，可选写入菜单种子。
/// </summary>
public interface ITenantDatabaseProvisioner
{
    /// <summary>
    /// 开通指定租户的独立数据库（受连接配置 EnableInitDb / EnableInitTable 开关约束）。
    /// </summary>
    Task<TenantProvisionResult> ProvisionAsync(SysTenant tenant, CancellationToken ct = default);
}

/// <summary>
/// 独立库开通结果。
/// </summary>
public sealed class TenantProvisionResult
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// 失败原因或说明。
    /// </summary>
    public string? Message { get; init; }

    public static TenantProvisionResult Ok(string? message = null)
        => new() { Succeeded = true, Message = message };

    public static TenantProvisionResult Fail(string message)
        => new() { Succeeded = false, Message = message };
}
