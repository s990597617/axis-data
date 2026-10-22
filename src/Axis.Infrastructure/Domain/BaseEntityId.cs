using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Infrastructure.Domain;

/// <summary>
/// 基类实体（只定义 id）
/// </summary>
public class BaseEntityId : IEntity
{
    /// <summary>
    /// 雪花Id
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public virtual long Id { get; set; }
}