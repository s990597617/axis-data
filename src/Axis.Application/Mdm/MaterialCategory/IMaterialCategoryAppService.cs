using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Mdm.MaterialCategory;

/// <summary>
/// 物料分类应用服务契约。
/// </summary>
public interface IMaterialCategoryAppService
{
    /// <summary>
    /// 按条件查询分类平铺列表。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>分类 DTO 只读列表，由前端组装树。</returns>
    Task<IReadOnlyList<MaterialCategoryDto>> ListAsync(MaterialCategoryListQuery query, CancellationToken ct = default);

    /// <summary>
    /// 查询全部启用分类并组装为树。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>根级分类树节点列表。</returns>
    Task<IReadOnlyList<MaterialCategoryTreeNode>> GetTreeAsync(CancellationToken ct = default);

    /// <summary>
    /// 校验上级与编码后创建分类。
    /// </summary>
    /// <param name="input">新增字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">未绑定租户、上级不存在或编码冲突时抛出。</exception>
    Task CreateAsync(SaveMaterialCategoryInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定分类；禁止把节点移动到自身下级形成环。
    /// </summary>
    /// <param name="id">分类主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">分类不存在、形成环或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveMaterialCategoryInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除分类；存在下级或仍被物料引用时不允许删除。
    /// </summary>
    /// <param name="id">分类主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">存在下级、仍被物料引用或分类不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
