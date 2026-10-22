using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 开放接口身份：AccessKey / AccessSecret，绑定租户与用户，供 HMAC 签名认证。
/// </summary>
[SugarTable("sys_open_api_identity", "开放接口身份")]
[SysTable]
[SugarIndex("uk_{table}_access_key", nameof(AccessKey), OrderByType.Asc, true)]
[SugarIndex("index_{table}_user", nameof(UserId), OrderByType.Asc)]
public class SysOpenApiIdentity : BaseEntity, ITenantId
{
    /// <summary>
    /// 绑定租户 Id；可空表示平台级身份。
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定租户Id", IsNullable = true)]
    public long? TenantId { get; set; }

    /// <summary>
    /// 绑定用户 Id；签名认证成功后以此用户身份进入系统。
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定用户Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 身份标识（AccessKey），全局唯一。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "身份标识", Length = 64)]
    public string AccessKey { get; set; } = null!;

    /// <summary>
    /// 签名密钥（AccessSecret）；仅服务端持久化，列表接口不回传明文。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "签名密钥", Length = 128)]
    public string AccessSecret { get; set; } = null!;

    /// <summary>
    /// 状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
