using Axis.Application.System.Menu.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Menu;

/// <summary>
/// 菜单管理应用服务契约。
/// </summary>
/// <remarks>
/// 负责菜单、目录与按钮节点维护；不直接处理 HTTP。
/// </remarks>
public interface IMenuAppService
{
    /// <summary>
    /// 按条件查询菜单节点平铺列表。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配标题与权限标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>菜单 DTO 只读列表，由前端组装树。</returns>
    Task<IReadOnlyList<MenuDto>> ListAsync(MenuListQuery query, CancellationToken ct = default);

    /// <summary>
    /// 获取系统权限码目录（按模块分组），供菜单按钮表单选择。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>模块分组列表；数据来自领域 <c>PermissionCatalog</c>。</returns>
    Task<IReadOnlyList<PermissionCatalogModuleDto>> GetPermissionCatalogAsync(
        CancellationToken ct = default);

    /// <summary>
    /// 校验父级与唯一性后创建菜单节点。
    /// </summary>
    /// <param name="input">新增字段；页面菜单须填路由，按钮须填权限标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">字段不合规、父级不存在或唯一性冲突时抛出。</exception>
    Task CreateAsync(SaveMenuInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定菜单节点；禁止把节点移动到自身后代下。
    /// </summary>
    /// <param name="id">菜单节点主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">节点不存在、形成环或唯一性冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveMenuInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除菜单节点；存在子节点或已分配角色时不允许删除。
    /// </summary>
    /// <param name="id">菜单节点主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">存在子节点、已授权或节点不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
