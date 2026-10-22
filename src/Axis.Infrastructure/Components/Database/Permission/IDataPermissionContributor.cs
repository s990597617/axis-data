using SqlSugar;

namespace Axis.Infrastructure.Components.Database.Permission;

/// <summary>
/// 数据权限过滤器扩展点：业务模块可注册自定义实体过滤。
/// </summary>
/// <remarks>
/// <para>实现类同时实现 <c>IAutoInject</c> 即可自动注入；由 <see cref="SqlSugarFilter.SetCustomEntityFilter"/> 挂载。</para>
/// <para>仅在当前用户非无限制数据范围时调用 <see cref="Apply"/>。</para>
/// <para>
/// 选型建议：
/// <list type="bullet">
/// <item>仅机构 → <c>IOrgId</c></item>
/// <item>机构或创建人 → <c>IOrgCreatedBy</c></item>
/// <item>机构或发布人/负责人 → <c>IOrgOwner</c>（字段映射为 OwnerUserId）</item>
/// <item>发布人 + 经理可见下级等 → 实现本接口，用表达式/子查询</item>
/// <item>强依赖用例上下文、难以全局化 → AppService 内 <c>ClearFilter</c> 后手写 Where</item>
/// </list>
/// </para>
/// <para>
/// 示例（伪代码）：
/// <code>
/// public sealed class ArticleDataPermissionContributor : IDataPermissionContributor, IAutoInject
/// {
///     public void Apply(ISqlSugarClient db, DataScopeState scope)
///     {
///         var orgIds = scope.OrgIds.ToList();
///         var uid = scope.UserId;
///         // 机构范围内 或 自己发布 或 自己是作者的经理（子查询示意）
///         db.QueryFilter.AddTableFilter&lt;Article&gt;(a =&gt;
///             orgIds.Contains(a.OrgId)
///             || a.PublisherId == uid
///             || SqlFunc.Subqueryable&lt;SysUser&gt;().Where(u =&gt; u.Id == a.PublisherId &amp;&amp; u.ManagerUserId == uid).Any());
///     }
///
///     public void Clear(ISqlSugarClient db) =&gt; db.QueryFilter.Clear&lt;Article&gt;();
/// }
/// </code>
/// 注意：该实体不要再实现 <c>IOrgId</c>/<c>IOrgOwner</c>，避免条件被 AND 叠加。
/// </para>
/// </remarks>
public interface IDataPermissionContributor
{
    /// <summary>
    /// 向当前请求的 QueryFilter 追加自定义条件。
    /// </summary>
    void Apply(ISqlSugarClient db, DataScopeState scope);

    /// <summary>
    /// 清除本扩展写入的过滤器。
    /// </summary>
    void Clear(ISqlSugarClient db);
}
