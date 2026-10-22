using Axis.Application.Demo.Product.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Demo.Product;

/// <summary>
/// 产品管理应用服务契约。
/// </summary>
public interface IProductAppService
{
    /// <summary>
    /// 按条件分页查询产品。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>产品 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<ProductDto>> PageAsync(ProductPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建产品；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveProductInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定产品。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveProductInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除产品。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
