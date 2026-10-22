using Axis.Domain.Enums;

namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 字典类型列表项及编辑回显数据。
/// </summary>
/// <remarks>
/// 仅暴露管理端展示与编辑所需字段，不直接返回领域实体。
/// </remarks>
public sealed class DictTypeDto
{
    /// <summary>
    /// 字典类型主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 字典类型显示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 字典类型编码；租户内唯一，业务代码按此读取字典。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 是否内置类型；内置类型不允许删除。
    /// </summary>
    public bool IsBuiltin { get; set; }

    /// <summary>
    /// 是否租户字典；否表示全平台共用。
    /// </summary>
    public bool IsTenant { get; set; }

    /// <summary>
    /// 排序号；数值越小越靠前。
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
