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
    /// Name。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// SourceType。
    /// </summary>
    public DhSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// SecretRef。
    /// </summary>
    [MaxLength(128)]
    public string? SecretRef { get; set; }

    /// <summary>
    /// IsReadOnly。
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// OrderNo。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// Status。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// Remark。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
