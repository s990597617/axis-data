using Axis.Application.System.Config.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Config;

/// <summary>
/// 参数配置应用服务契约。
/// </summary>
public interface IConfigAppService
{
    /// <summary>
    /// 按条件分页查询参数配置。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>参数配置 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<ConfigDto>> PageAsync(ConfigPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 校验编码唯一后创建参数配置。
    /// </summary>
    /// <param name="input">新增字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    Task CreateAsync(SaveConfigInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定参数配置。
    /// </summary>
    /// <param name="id">参数主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">参数不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveConfigInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除参数配置；内置参数不允许删除。
    /// </summary>
    /// <param name="id">参数主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">参数不存在或为内置参数时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
