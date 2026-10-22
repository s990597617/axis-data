using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源管理应用服务契约。
/// </summary>
public interface ISourceAppService
{
    Task<SqlSugarPagedList<SourceDto>> PageAsync(SourcePageQuery query, CancellationToken ct = default);

    /// <summary>启用中的数据源下拉选项。</summary>
    Task<IReadOnlyList<DataHubLookupItemDto>> ListLookupAsync(CancellationToken ct = default);

    Task CreateAsync(SaveSourceInput input, CancellationToken ct = default);

    Task UpdateAsync(long id, SaveSourceInput input, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);
}
