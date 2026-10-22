using Axis.Application.Demo.Brand.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Demo.Brand;

/// <summary>
/// 品牌管理应用服务契约。
/// </summary>
public interface IBrandAppService
{
    /// <summary>
    /// 按条件分页查询品牌。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>品牌 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<BrandDto>> PageAsync(BrandPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建品牌；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveBrandInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定品牌。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveBrandInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除品牌。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
