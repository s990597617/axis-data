using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Tenant.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 租户管理应用服务契约。
/// </summary>
public interface ITenantAppService
{
    /// <summary>
    /// 按条件分页查询租户。
    /// </summary>
    Task<SqlSugarPagedList<TenantDto>> PageAsync(TenantPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 校验编码与数据库配置后创建租户；Id 隔离自动写入菜单并开通管理员，独立库触发建库开通。
    /// </summary>
    /// <exception cref="BusinessException">编码冲突或独立库配置无效时抛出。</exception>
    Task CreateAsync(SaveTenantInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定租户。
    /// </summary>
    /// <exception cref="BusinessException">租户不存在、编码冲突或配置不合规时抛出。</exception>
    Task UpdateAsync(long id, SaveTenantInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除租户；仍有用户时不允许删除。
    /// </summary>
    /// <exception cref="BusinessException">租户不存在或仍有用户时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 查询租户已启用的菜单与按钮 Id。
    /// </summary>
    Task<IReadOnlyList<long>> GetMenuIdsAsync(long tenantId, CancellationToken ct = default);

    /// <summary>
    /// 覆盖保存租户菜单启用集（不可含平台专属菜单）。
    /// </summary>
    Task GrantMenusAsync(long tenantId, IReadOnlyCollection<long> menuIds, CancellationToken ct = default);

    /// <summary>
    /// 可分配给租户的菜单平铺列表（排除平台专属）。
    /// </summary>
    Task<IReadOnlyList<MenuDto>> ListAssignableMenusAsync(CancellationToken ct = default);
}
