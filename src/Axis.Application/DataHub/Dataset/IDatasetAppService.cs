using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.DataHub.Dataset;

/// <summary>
/// 数据集管理应用服务契约。
/// </summary>
public interface IDatasetAppService
{
    Task<SqlSugarPagedList<DatasetDto>> PageAsync(DatasetPageQuery query, CancellationToken ct = default);

    /// <summary>启用中的数据集下拉选项。</summary>
    Task<IReadOnlyList<DataHubLookupItemDto>> ListLookupAsync(CancellationToken ct = default);

    Task CreateAsync(SaveDatasetInput input, CancellationToken ct = default);

    Task UpdateAsync(long id, SaveDatasetInput input, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
