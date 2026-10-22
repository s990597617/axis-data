using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Config.Dto;

/// <summary>
/// 新增或更新参数配置的输入数据。
/// </summary>
/// <remarks>
/// 格式校验以 Validator 为准；编码唯一性由应用服务校验。
/// </remarks>
public sealed class SaveConfigInput
{
    /// <summary>
    /// 参数主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分组编码；可为空，最长 64。
    /// </summary>
    [MaxLength(64)]
    public string? GroupCode { get; set; }

    /// <summary>
    /// 参数显示名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 参数编码；必填，最长 64，租户内唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 参数值；可为空，最长 1024。
    /// </summary>
    [MaxLength(1024)]
    public string? Value { get; set; }

    /// <summary>
    /// 配置类型；默认系统级。
    /// </summary>
    public ConfigTypeEnum Type { get; set; } = ConfigTypeEnum.System;

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
