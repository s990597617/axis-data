using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source.Dto;
using Axis.Infrastructure.Components.Database.Internal;

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

    /// <summary>按入参测试连通性（不要求已保存）。</summary>
    Task<TestSourceConnectionResultDto> TestConnectionAsync(TestSourceConnectionInput input, CancellationToken ct = default);

    /// <summary>按已保存数据源测试连通性，并回写最近测试结果。</summary>
    Task<TestSourceConnectionResultDto> TestConnectionByIdAsync(long id, CancellationToken ct = default);
}