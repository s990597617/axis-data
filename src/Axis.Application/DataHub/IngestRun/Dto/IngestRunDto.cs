using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestRun.Dto;

/// <summary>
/// 采集运行列表项及编辑回显数据。
/// </summary>
public sealed class IngestRunDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// JobId。
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    /// Name。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// RunStatus。
    /// </summary>
    public DhRunStatusEnum RunStatus { get; set; }

    /// <summary>
    /// StartedTime。
    /// </summary>
    public DateTime? StartedTime { get; set; }

    /// <summary>
    /// FinishedTime。
    /// </summary>
    public DateTime? FinishedTime { get; set; }

    /// <summary>
    /// RowCountRead。
    /// </summary>
    public long RowCountRead { get; set; }

    /// <summary>
    /// RowCountChanged。
    /// </summary>
    public long RowCountChanged { get; set; }

    /// <summary>
    /// ErrorMessage。
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Watermark。
    /// </summary>
    public string? Watermark { get; set; }

    /// <summary>
    /// OrderNo。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// Status。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// Remark。
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
