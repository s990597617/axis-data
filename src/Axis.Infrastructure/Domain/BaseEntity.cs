using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Infrastructure.Domain;

/// <summary>
/// 自定义主键类型基类
/// </summary>
[SugarIndex("index_{table}_CreateBy", nameof(CreatedTime), OrderByType.Asc)]
public abstract class BaseEntity : BaseEntityId, ISoftDelete, IVersion
{
    /// <summary>
    /// 创建人编号
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// 创建人名称
    /// </summary>   
    public string Creator { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 修改人编号
    /// </summary>
    [SugarColumn(ColumnDescription = "修改人编号", IsNullable = true)]
    public long? ModifiedBy { get; set; }

    /// <summary>
    /// 修改人名称
    /// </summary>
    [SugarColumn(ColumnDescription = "修改人名称", IsNullable = true)]
    public string Modifier { get; set; }

    /// <summary>
    /// 修改时间
    /// </summary>
    [SugarColumn(ColumnDescription = "修改时间", IsNullable = true)]
    public virtual DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 已删除的
    /// </summary>
    public bool Deleted { get; set; } = false;
    
    /// <summary>
    /// 版本号（乐观锁令牌）；更新时由 SqlSugar OptLock 校验并自动推进。
    /// </summary>
    [SugarColumn(ColumnDescription = "版本号(乐观锁)", IsEnableUpdateVersionValidation = true)]
    public long Version { get; set; }
}