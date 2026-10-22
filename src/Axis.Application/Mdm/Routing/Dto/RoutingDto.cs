using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 工艺路线列表项及详情数据。
/// </summary>
public sealed class RoutingDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 路线名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 路线编码；租户内唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 工序列数；列表查询填充，详情可不使用。
    /// </summary>
    public int StepCount { get; set; }

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
    /// 工序行；详情接口填充，列表为空。
    /// </summary>
    public List<RoutingStepDto> Steps { get; set; } = [];

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
