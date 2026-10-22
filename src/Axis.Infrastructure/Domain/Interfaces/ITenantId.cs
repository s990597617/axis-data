namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 租户数据强隔离标记（按 <see cref="TenantId"/> 严格过滤）。
/// </summary>
/// <remarks>
/// 实现该接口的实体由 SqlSugar QueryFilter 附加 <c>TenantId == 当前租户</c>；
/// 超级管理员不挂租户过滤器。需要「本租户 + 平台全局」可见性时请实现 <see cref="ITenantOrGlobal"/>。
/// </remarks>
public interface ITenantId
{
    /// <summary>
    /// 租户 Id；业务强隔离表不应为 null（未绑定租户的账号禁止登录）。
    /// </summary>
    long? TenantId { get; set; }
}
