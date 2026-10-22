using Axis.Application.DataHub.Dataset.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.DataHub.Dataset;

/// <summary>
/// 数据集管理应用服务契约。
/// </summary>
public interface IDatasetAppService
{
    /// <summary>
    /// 按条件分页查询数据集。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>数据集 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<DatasetDto>> PageAsync(DatasetPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 创建数据集；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    Task CreateAsync(SaveDatasetInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定数据集。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveDatasetInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除数据集。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
