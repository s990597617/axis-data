using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Process.Dto;

/// <summary>
/// 工序列表项及编辑回显数据。
/// </summary>
public sealed class ProcessDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 工序名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 工序编码；租户内唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注。
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
