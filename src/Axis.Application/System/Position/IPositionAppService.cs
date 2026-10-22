using Axis.Application.System.Position.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Position;

/// <summary>
/// 岗位管理应用服务契约。
/// </summary>
public interface IPositionAppService
{
    /// <summary>
    /// 按条件分页查询岗位。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>岗位 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<PositionDto>> PageAsync(PositionPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建岗位；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SavePositionInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定岗位。
    /// </summary>
    /// <param name="id">岗位主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">岗位不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SavePositionInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除岗位；已被用户使用时不允许删除。
    /// </summary>
    /// <param name="id">岗位主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">岗位不存在或仍被用户使用时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
