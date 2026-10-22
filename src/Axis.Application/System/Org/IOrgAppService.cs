using Axis.Application.System.Org.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Org;

/// <summary>
/// 机构管理应用服务契约。
/// </summary>
/// <remarks>
/// 负责机构树维护与启用机构树查询；不直接处理 HTTP。
/// </remarks>
public interface IOrgAppService
{
    /// <summary>
    /// 按条件查询机构平铺列表。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>机构 DTO 只读列表，由前端组装树。</returns>
    Task<IReadOnlyList<OrgDto>> ListAsync(OrgListQuery query, CancellationToken ct = default);

    /// <summary>
    /// 查询全部启用机构并组装为树。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>根级机构树节点列表。</returns>
    Task<IReadOnlyList<OrgTreeNode>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>
    /// 校验上级与编码后创建机构。
    /// </summary>
    /// <param name="input">新增字段；编码非空时须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">上级不存在或编码冲突时抛出。</exception>
    Task CreateAsync(SaveOrgInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定机构；禁止把节点移动到自身下级形成环。
    /// </summary>
    /// <param name="id">机构主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">机构不存在、形成环或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveOrgInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除机构；存在下级或下属用户时不允许删除。
    /// </summary>
    /// <param name="id">机构主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">存在下级、下属用户或机构不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
