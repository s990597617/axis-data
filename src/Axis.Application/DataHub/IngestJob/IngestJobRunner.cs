using System.Data;
using System.Diagnostics;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集任务执行器：创建运行记录、连接源端受控读取并回写指标。
/// </summary>
/// <remarks>
/// Phase 1 不落 ODS，仅验证连接与读取批数据；<see cref="DhIngestRun.RowCountChanged"/> 恒为 0。
/// </remarks>
public sealed class IngestJobRunner(
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    Source.SourceConnectionFactory connectionFactory,
    IConfiguration configuration)
    : IIngestJobRunner, IAutoInject
{
    /// <inheritdoc />
    public async Task<TriggerIngestJobResultDto> ExecuteAsync(long jobId, CancellationToken ct = default)
    {
        var job = await db.Queryable<DhIngestJob>().Where(item => item.Id == jobId).FirstAsync(ct)
                  ?? throw new BusinessException("采集任务不存在");

        if (job.Status != StatusEnum.Enable)
            throw new BusinessException("采集任务已停用，无法执行");

        var hasRunning = await db.Queryable<DhIngestRun>()
            .Where(item => item.JobId == jobId && item.RunStatus == DhRunStatusEnum.Running)
            .AnyAsync(ct);
        if (hasRunning)
            throw new BusinessException("该任务已有运行中的采集，请稍后再试");

        var dataset = await db.Queryable<DhDataset>().Where(item => item.Id == job.DatasetId).FirstAsync(ct)
                      ?? throw new BusinessException("采集任务关联的数据集不存在");

        if (dataset.Status != StatusEnum.Enable)
            throw new BusinessException("关联数据集已停用，无法执行采集");

        var source = await db.Queryable<DhSource>().Where(item => item.Id == dataset.SourceId).FirstAsync(ct)
                     ?? throw new BusinessException("数据集关联的数据源不存在");

        if (source.Status != StatusEnum.Enable)
            throw new BusinessException("关联数据源已停用，无法执行采集");

        if (source.SourceType == DhSourceTypeEnum.Http)
            throw new BusinessException("HTTP 数据源暂不支持 SQL 采集，请使用关系型数据源");

        var batchCap = configuration.GetValue("DataHub:DefaultBatchSize", 1000);
        if (batchCap <= 0) batchCap = 1000;
        var batchSize = Math.Min(job.BatchSize <= 0 ? batchCap : job.BatchSize, batchCap);

        var strategy = job.Strategy;
        var watermark = await ResolveWatermarkAsync(jobId, strategy, dataset.IncrementalField, ct);
        var run = await CreateRunRecordAsync(job, ct);

        var sw = Stopwatch.StartNew();
        try
        {
            await MarkRunningAsync(run.Id, ct);

            var baseSelect = QualifyBaseSelect(dataset, source.SourceType);
            var filtered = IngestSqlBuilder.ApplyIncrementalFilter(
                baseSelect,
                strategy,
                dataset.IncrementalField,
                watermark,
                source.SourceType);

            var connectionString = connectionFactory.ResolveConnectionString(source);
            using var client = connectionFactory.OpenClient(source.SourceType, connectionString);

            var (rowsRead, newWatermark) = await ReadBatchAsync(
                client,
                filtered,
                batchSize,
                source.SourceType,
                strategy,
                dataset.IncrementalField,
                ct);

            sw.Stop();
            await CompleteRunAsync(
                run.Id,
                DhRunStatusEnum.Succeeded,
                rowsRead,
                0,
                newWatermark ?? watermark,
                null,
                ct);

            return new TriggerIngestJobResultDto
            {
                RunId = run.Id,
                RunCode = run.Code,
                RunStatus = DhRunStatusEnum.Succeeded,
                RowCountRead = rowsRead,
                RowCountChanged = 0,
                Watermark = newWatermark ?? watermark
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            var message = TrimError(ex);
            await CompleteRunAsync(run.Id, DhRunStatusEnum.Failed, 0, 0, watermark, message, ct);

            return new TriggerIngestJobResultDto
            {
                RunId = run.Id,
                RunCode = run.Code,
                RunStatus = DhRunStatusEnum.Failed,
                RowCountRead = 0,
                RowCountChanged = 0,
                Watermark = watermark,
                ErrorMessage = message
            };
        }
    }

    private static string QualifyBaseSelect(DhDataset dataset, DhSourceTypeEnum sourceType)
    {
        var raw = IngestSqlBuilder.BuildBaseSelect(dataset);
        if (dataset.ObjectType is DhObjectTypeEnum.Table or DhObjectTypeEnum.View)
            return IngestSqlBuilder.QualifyObjectNames(raw, sourceType);
        return raw;
    }

    private async Task<string?> ResolveWatermarkAsync(
        long jobId,
        DhUpdateStrategyEnum strategy,
        string? incrementalField,
        CancellationToken ct)
    {
        if (strategy != DhUpdateStrategyEnum.Incremental || string.IsNullOrWhiteSpace(incrementalField))
            return null;

        var last = await db.Queryable<DhIngestRun>()
            .Where(item => item.JobId == jobId && item.RunStatus == DhRunStatusEnum.Succeeded)
            .Where(item => item.Watermark != null && item.Watermark != "")
            .OrderBy(item => item.FinishedTime, OrderByType.Desc)
            .Select(item => item.Watermark)
            .FirstAsync(ct);

        return string.IsNullOrWhiteSpace(last) ? null : last;
    }

    private async Task<DhIngestRun> CreateRunRecordAsync(DhIngestJob job, CancellationToken ct)
    {
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var jobCode = string.IsNullOrWhiteSpace(job.Code) ? job.Id.ToString() : job.Code;
        var code = $"run-{jobCode}-{stamp}";
        var name = $"{job.Name}-{stamp}";

        var entity = new DhIngestRun
        {
            Id = YitIdHelper.NextId(),
            TenantId = currentAccount.TenantId,
            JobId = job.Id,
            Name = name,
            Code = code,
            RunStatus = DhRunStatusEnum.Pending,
            OrderNo = 100,
            Status = StatusEnum.Enable,
            CreatedBy = currentAccount.Id,
            Creator = currentAccount.RealName,
            CreatedTime = DateTime.Now
        };

        await db.Insertable(entity).ExecuteCommandAsync(ct);
        return entity;
    }

    private async Task MarkRunningAsync(long runId, CancellationToken ct)
    {
        var now = DateTime.Now;
        await db.Updateable<DhIngestRun>()
            .SetColumns(item => new DhIngestRun
            {
                RunStatus = DhRunStatusEnum.Running,
                StartedTime = now,
                ModifiedBy = currentAccount.Id,
                Modifier = currentAccount.RealName,
                ModifiedTime = now
            })
            .Where(item => item.Id == runId)
            .ExecuteCommandAsync(ct);
    }

    private async Task CompleteRunAsync(
        long runId,
        DhRunStatusEnum status,
        long rowsRead,
        long rowsChanged,
        string? watermark,
        string? error,
        CancellationToken ct)
    {
        var now = DateTime.Now;
        await db.Updateable<DhIngestRun>()
            .SetColumns(item => new DhIngestRun
            {
                RunStatus = status,
                FinishedTime = now,
                RowCountRead = rowsRead,
                RowCountChanged = rowsChanged,
                Watermark = watermark,
                ErrorMessage = error,
                ModifiedBy = currentAccount.Id,
                Modifier = currentAccount.RealName,
                ModifiedTime = now
            })
            .Where(item => item.Id == runId)
            .ExecuteCommandAsync(ct);
    }

    private static async Task<(long RowsRead, string? NewWatermark)> ReadBatchAsync(
        SqlSugarClient client,
        string filteredSelect,
        int batchSize,
        DhSourceTypeEnum sourceType,
        DhUpdateStrategyEnum strategy,
        string? incrementalField,
        CancellationToken ct)
    {
        var countSql = IngestSqlBuilder.BuildCountSql(filteredSelect);
        var readSql = IngestSqlBuilder.BuildReadSql(filteredSelect, batchSize, sourceType);

        return await Task.Run(() =>
        {
            try
            {
                _ = client.Ado.GetScalar(countSql);
                var table = client.Ado.GetDataTable(readSql);
                var rowsRead = table.Rows.Count;
                var watermark = ExtractWatermark(table, strategy, incrementalField);
                return (RowsRead: (long)rowsRead, NewWatermark: watermark);
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new BusinessException($"源端读取失败：{TrimError(ex)}");
            }
        }, ct);
    }

    private static string? ExtractWatermark(
        DataTable table,
        DhUpdateStrategyEnum strategy,
        string? incrementalField)
    {
        if (strategy != DhUpdateStrategyEnum.Incremental || string.IsNullOrWhiteSpace(incrementalField))
            return null;
        var column = table.Columns.Cast<DataColumn>()
            .FirstOrDefault(col => col.ColumnName.Equals(incrementalField, StringComparison.OrdinalIgnoreCase));
        if (column is null || table.Rows.Count == 0)
            return null;

        object? max = null;
        foreach (DataRow row in table.Rows)
        {
            var value = row[column];
            if (value is DBNull) continue;
            if (max is null || CompareValues(value, max) > 0)
                max = value;
        }

        return max?.ToString();
    }

    private static int CompareValues(object left, object right)
    {
        if (left is IComparable comparable && right is IComparable)
        {
            try
            {
                return comparable.CompareTo(right);
            }
            catch
            {
                return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
            }
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.Ordinal);
    }

    private static string TrimError(Exception ex)
    {
        var message = ex switch
        {
            BusinessException be => be.Message,
            _ => ex.Message
        };
        if (string.IsNullOrWhiteSpace(message)) return "未知错误";
        message = message.ReplaceLineEndings(" ").Trim();
        return message.Length > 1000 ? message[..1000] : message;
    }
}
