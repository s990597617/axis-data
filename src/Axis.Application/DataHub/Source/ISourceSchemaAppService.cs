using Axis.Application.DataHub.Common;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源元数据浏览（表/视图/列）。
/// </summary>
public interface ISourceSchemaAppService
{
    /// <summary>列出数据源中的表或视图。</summary>
    Task<IReadOnlyList<DataHubLookupItemDto>> ListObjectsAsync(long sourceId, DhObjectTypeEnum objectType, CancellationToken ct = default);

    /// <summary>列出指定对象的列。</summary>
    Task<IReadOnlyList<DataHubLookupItemDto>> ListColumnsAsync(long sourceId, string objectName, CancellationToken ct = default);
}