using Axis.Domain.Enums;

namespace Axis.Application.Demo.Product.Dto;

/// <summary>
/// 产品列表项及编辑回显数据。
/// </summary>
public sealed class ProductDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 产品名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 产品编码；可为空，非空时租户内唯一。
    /// </summary>
    public string? Code { get; set; }

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
