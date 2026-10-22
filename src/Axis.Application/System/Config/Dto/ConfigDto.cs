using Axis.Domain.Enums;

namespace Axis.Application.System.Config.Dto;

/// <summary>
/// 参数配置列表项及编辑回显数据。
/// </summary>
public sealed class ConfigDto
{
    /// <summary>
    /// 参数主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分组编码；可为空。
    /// </summary>
    public string? GroupCode { get; set; }

    /// <summary>
    /// 参数显示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 参数编码；租户内唯一，业务按此读取。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 参数值；可为空。
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 配置类型枚举值。
    /// </summary>
    public ConfigTypeEnum Type { get; set; }

    /// <summary>
    /// 是否内置参数；内置参数不允许删除。
    /// </summary>
    public bool IsBuiltin { get; set; }

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
