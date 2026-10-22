namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 实体基类
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public long Id { get; set; }
}