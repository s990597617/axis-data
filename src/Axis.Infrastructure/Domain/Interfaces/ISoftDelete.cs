namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 软删除标记。
/// </summary>
/// <remarks>
/// 实现该接口的实体由 SqlSugar 全局过滤器自动附加 <c>Deleted == false</c>；
/// 需包含已删数据时在查询上使用 <c>ClearFilter&lt;ISoftDelete&gt;()</c>。
/// </remarks>
public interface ISoftDelete
{
    /// <summary>
    /// 是否已软删除；<c>true</c> 表示已删除，全局过滤器默认不可见。
    /// </summary>
    bool Deleted { get; set; }
}
