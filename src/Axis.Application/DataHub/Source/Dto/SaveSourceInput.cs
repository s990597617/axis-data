using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 新增或更新数据源的输入数据。
/// </summary>
public sealed class SaveSourceInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 数据源类型。
    /// </summary>
    public DhSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 密钥引用（config: / env:）。
    /// </summary>
    [MaxLength(128)]
    public string? SecretRef { get; set; }

    /// <summary>
    /// 内联连接串（仅写入，不回显）。
    /// 更新时：null/省略保留原值；空字符串清空；非空则替换。
    /// </summary>
    [MaxLength(1024)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 是否只读。
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// 排序。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}