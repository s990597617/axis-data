using Axis.Domain.Enums;

namespace Axis.Application.System.Org.Dto;

/// <summary>
/// 启用机构组成的树节点，供下拉选择与用户分配机构使用。
/// </summary>
/// <remarks>
/// 由 <see cref="Org.IOrgAppService.GetTreeAsync"/> 组装；含递归 <see cref="Children"/>。
/// </remarks>
public sealed class OrgTreeNode
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
    /// 机构编码；可为空。
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 机构类型枚举值。
    /// </summary>
    public OrgTypeEnum Type { get; set; }

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态；树接口仅返回启用节点。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 下级机构节点；无子节点时为空列表。
    /// </summary>
    public List<OrgTreeNode> Children { get; set; } = [];
}
