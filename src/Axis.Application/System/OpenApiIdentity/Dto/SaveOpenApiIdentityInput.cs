using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.OpenApiIdentity.Dto;

/// <summary>
/// 新增或更新开放接口身份。
/// </summary>
/// <remarks>
/// 新增时可空 <see cref="AccessSecret"/>，服务端自动生成；更新不改密钥（重置走独立接口）。
/// </remarks>
public sealed class SaveOpenApiIdentityInput
{
    /// <summary>主键；更新时使用。</summary>
    public long Id { get; set; }

    /// <summary>身份标识 AccessKey。</summary>
    [Required, MaxLength(64)]
    public string AccessKey { get; set; } = null!;

    /// <summary>签名密钥；新增时可空表示自动生成。</summary>
    [MaxLength(128)]
    public string? AccessSecret { get; set; }

    /// <summary>绑定租户 Id。</summary>
    public long? TenantId { get; set; }

    /// <summary>绑定用户 Id。</summary>
    [Range(1, long.MaxValue)]
    public long UserId { get; set; }

    /// <summary>状态。</summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>备注。</summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
