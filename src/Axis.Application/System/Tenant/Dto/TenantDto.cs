using Axis.Domain.Enums;

namespace Axis.Application.System.Tenant.Dto;

/// <summary>
/// 租户列表项及编辑回显数据。
/// </summary>
public sealed class TenantDto
{
    /// <summary>
    /// 租户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户显示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 租户编码；全局唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 租户隔离类型。
    /// </summary>
    public TenantTypeEnum Type { get; set; }

    /// <summary>
    /// 独立库连接配置编号；非独立库模式可为空。
    /// </summary>
    public string? DbConfigId { get; set; }

    /// <summary>
    /// 租户管理员用户 Id；可为空。
    /// </summary>
    public long? AdminUserId { get; set; }

    /// <summary>
    /// 开通状态。
    /// </summary>
    public TenantProvisionStatusEnum ProvisionStatus { get; set; }

    /// <summary>
    /// 开通失败原因；可为空。
    /// </summary>
    public string? ProvisionMessage { get; set; }

    /// <summary>
    /// 租户到期时间；为空表示不过期。
    /// </summary>
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
