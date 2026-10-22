namespace Axis.Application.System.Tenant.Dto;

/// <summary>
/// 仅携带租户主键的路径参数。
/// </summary>
public sealed class TenantIdRequest
{
    /// <summary>
    /// 租户主键 Id。
    /// </summary>
    public long Id { get; set; }
}
