using Axis.Domain.Enums;

namespace Axis.Application.System.OpenApiIdentity.Dto;

/// <summary>
/// 开放接口身份列表项。
/// </summary>
public sealed class OpenApiIdentityDto
{
    /// <summary>主键。</summary>
    public long Id { get; set; }

    /// <summary>身份标识 AccessKey。</summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>绑定租户 Id。</summary>
    public long? TenantId { get; set; }

    /// <summary>绑定租户名称。</summary>
    public string? TenantName { get; set; }

    /// <summary>绑定用户 Id。</summary>
    public long UserId { get; set; }

    /// <summary>绑定用户账号。</summary>
    public string? UserAccount { get; set; }

    /// <summary>绑定用户姓名。</summary>
    public string? UserRealName { get; set; }

    /// <summary>状态。</summary>
    public StatusEnum Status { get; set; }

    /// <summary>备注。</summary>
    public string? Remark { get; set; }

    /// <summary>创建时间。</summary>
    public DateTime CreatedTime { get; set; }
}
