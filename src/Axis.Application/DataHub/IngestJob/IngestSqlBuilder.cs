using System.Text.RegularExpressions;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集读取 SQL 拼装（表/视图/自定义查询 + 增量过滤 + 批大小）。
/// </summary>
internal static partial class IngestSqlBuilder
{
    private static readonly Regex SafeIdentifier = IdentifierRegex();

    /// <summary>
    /// 构建用于统计与读取的基础查询（不含 TOP/LIMIT）。
    /// </summary>
    public static string BuildBaseSelect(DhDataset dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);

        return dataset.ObjectType switch
        {
            DhObjectTypeEnum.Table or DhObjectTypeEnum.View => BuildTableSelect(dataset.ObjectName),
            DhObjectTypeEnum.Query => BuildQuerySelect(dataset.CustomSql),
            _ => throw new BusinessException("不支持的数据集对象类型")
        };
    }

    /// <summary>
    /// 在基础查询上追加增量过滤（仅当策略为增量且配置了增量字段时）。
    /// </summary>
    public static string ApplyIncrementalFilter(
        string baseSelect,
        DhUpdateStrategyEnum strategy,
        string? incrementalField,
        string? watermark,
        DhSourceTypeEnum sourceType)
    {
        if (strategy != DhUpdateStrategyEnum.Incremental
            || string.IsNullOrWhiteSpace(incrementalField)
            || string.IsNullOrWhiteSpace(watermark))
        {
            return baseSelect;
        }

        var field = QuoteIdentifier(incrementalField.Trim(), sourceType);
        var safeWatermark = watermark.Replace("'", "''", StringComparison.Ordinal);
        return $"{baseSelect} WHERE {field} > '{safeWatermark}'";
    }

    /// <summary>
    /// 统计行数 SQL。
    /// </summary>
    public static string BuildCountSql(string filteredSelect)
        => $"SELECT COUNT(1) FROM ({filteredSelect}) axis_ingest_cnt";

    /// <summary>
    /// 受批大小限制的读取 SQL。
    /// </summary>
    public static string BuildReadSql(string filteredSelect, int batchSize, DhSourceTypeEnum sourceType)
    {
        if (batchSize <= 0) throw new BusinessException("批大小必须大于 0");

        return sourceType switch
        {
            DhSourceTypeEnum.SqlServer =>
                $"SELECT TOP ({batchSize}) * FROM ({filteredSelect}) axis_ingest_read",
            DhSourceTypeEnum.MySql or DhSourceTypeEnum.PostgreSql =>
                $"SELECT * FROM ({filteredSelect}) axis_ingest_read LIMIT {batchSize}",
            _ => throw new BusinessException("当前数据源类型暂不支持采集读取")
        };
    }

    private static string BuildTableSelect(string? objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            throw new BusinessException("数据集未配置对象名");

        var trimmed = objectName.Trim();
        var parts = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
            throw new BusinessException("数据集对象名无效");

        foreach (var part in parts)
        {
            if (!SafeIdentifier.IsMatch(part))
                throw new BusinessException($"对象名包含非法字符：{part}");
        }

        // 由后续 QuoteIdentifier 按库类型加引号；此处用中性拼接
        return $"SELECT * FROM {string.Join('.', parts)}";
    }

    private static string BuildQuerySelect(string? customSql)
    {
        if (string.IsNullOrWhiteSpace(customSql))
            throw new BusinessException("查询类型数据集未配置自定义 SQL");

        var sql = customSql.Trim();
        if (sql.Contains(';', StringComparison.Ordinal))
            throw new BusinessException("自定义 SQL 不允许包含分号");

        return $"SELECT * FROM ({sql}) axis_ingest_src";
    }

    private static string QuoteIdentifier(string name, DhSourceTypeEnum sourceType)
    {
        if (!SafeIdentifier.IsMatch(name))
            throw new BusinessException($"增量字段名无效：{name}");

        return sourceType switch
        {
            DhSourceTypeEnum.SqlServer => $"[{name}]",
            DhSourceTypeEnum.MySql => $"`{name}`",
            DhSourceTypeEnum.PostgreSql => $"\"{name}\"",
            _ => name
        };
    }

    /// <summary>
    /// 将表对象名按数据源类型加引号（仅处理 schema.table 形式）。
    /// </summary>
    public static string QualifyObjectNames(string baseSelect, DhSourceTypeEnum sourceType)
    {
        // baseSelect 形如 SELECT * FROM schema.table 或 SELECT * FROM table
        const string prefix = "SELECT * FROM ";
        if (!baseSelect.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return baseSelect;

        var rest = baseSelect[prefix.Length..].Trim();
        var parts = rest.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var quoted = parts.Select(p => QuoteIdentifier(p, sourceType));
        return prefix + string.Join('.', quoted);
    }

    [GeneratedRegex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant)]
    private static partial Regex IdentifierRegex();
}
