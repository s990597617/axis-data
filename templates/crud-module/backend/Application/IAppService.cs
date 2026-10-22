using Axis.Application.__Area__.__Module__.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.__Area__.__Module__;

/// <summary>
/// __CnName__管理应用服务契约。
/// </summary>
public interface I__Module__AppService
{
    /// <summary>
    /// 按条件分页查询__CnName__。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>__CnName__ DTO 分页结果。</returns>
    Task<SqlSugarPagedList<__Module__Dto>> PageAsync(__Module__PageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建__CnName__；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(Save__Module__Input input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定__CnName__。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, Save__Module__Input input, CancellationToken ct = default);

    /// <summary>
    /// 删除__CnName__。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
