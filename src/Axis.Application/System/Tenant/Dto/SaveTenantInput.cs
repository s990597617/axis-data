using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Tenant.Dto;

/// <summary>
/// 新增或更新租户的输入数据。
/// </summary>
/// <remarks>
/// 独立数据库模式须填写 <see cref="DbConfigId"/>；编码全局唯一由应用服务校验。
/// </remarks>
public sealed class SaveTenantInput
{
    /// <summary>
    /// 租户主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户显示名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 租户编码；必填，最长 64，全局唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 租户隔离类型；默认字段隔离。
    /// </summary>
    public TenantTypeEnum Type { get; set; } = TenantTypeEnum.Id;

    /// <summary>
    /// 独立库连接配置编号；独立库模式必填，最长 64。
    /// </summary>
    [MaxLength(64)]
    public string? DbConfigId { get; set; }

    /// <summary>
    /// 租户管理员用户 Id；可为空。
    /// </summary>
    public long? AdminUserId { get; set; }

    /// <summary>
    /// 租户到期时间；为空表示不过期。
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
