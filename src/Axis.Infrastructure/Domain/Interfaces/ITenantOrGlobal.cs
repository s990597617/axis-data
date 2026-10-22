namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 租户或全局共享数据标记（字典、系统配置等）。
/// </summary>
/// <remarks>
/// 与 <see cref="ITenantId"/> 不同：本接口允许 <c>TenantId == null</c> 的平台行对当前租户可见。
/// 用户、角色、机构、岗位等强隔离实体应实现 <see cref="ITenantId"/>，勿混用本接口。
/// </remarks>
public interface ITenantOrGlobal
{
    /// <summary>
    /// 租户 Id；为空表示平台全局行。
    /// </summary>
    long? TenantId { get; set; }
}
