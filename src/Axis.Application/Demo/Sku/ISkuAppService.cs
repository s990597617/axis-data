using Axis.Application.Demo.Sku.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Demo.Sku;

/// <summary>
/// SKU管理应用服务契约。
/// </summary>
public interface ISkuAppService
{
    /// <summary>
    /// 按条件分页查询SKU。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>SKU DTO 分页结果。</returns>
    Task<SqlSugarPagedList<SkuDto>> PageAsync(SkuPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建SKU；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveSkuInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定SKU。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveSkuInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除SKU。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
