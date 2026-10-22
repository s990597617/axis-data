using Axis.Domain.Enums;

namespace Axis.Application.System.Org.Dto;

/// <summary>
/// 机构列表项及编辑回显数据。
/// </summary>
/// <remarks>
/// 仅暴露管理端展示与编辑所需字段，不直接返回领域实体。
/// </remarks>
public sealed class OrgDto
{
    /// <summary>
    /// 机构主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 上级机构 Id；根节点为 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 机构名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 机构编码；可为空，非空时租户内唯一。
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 机构类型枚举值。
    /// </summary>
    public OrgTypeEnum Type { get; set; }

    /// <summary>
    /// 排序号；数值越小越靠前。
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
