using Axis.Domain.Enums;

namespace Axis.Application.Demo.PosClone.Dto;

/// <summary>
/// 岗位副本列表项及编辑回显数据。
/// </summary>
public sealed class PosCloneDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 岗位名称.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 岗位编码.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 状态.
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注.
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
