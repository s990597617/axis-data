using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集任务管理应用服务契约。
/// </summary>
public interface IIngestJobAppService
{
    /// <summary>
    /// 按条件分页查询采集任务。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>采集任务 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<IngestJobDto>> PageAsync(IngestJobPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建采集任务；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveIngestJobInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定采集任务。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveIngestJobInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除采集任务。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
