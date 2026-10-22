using Axis.Application.Common;
using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集任务管理应用服务。
/// </summary>
public sealed class IngestJobAppService(
    IRepository<DhIngestJob> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IIngestJobRunner ingestJobRunner)
    : BaseAppService<DhIngestJob>(repository, db, currentAccount), IIngestJobAppService, IAutoInject
{
    public async Task<SqlSugarPagedList<IngestJobDto>> PageAsync(IngestJobPageQuery query, CancellationToken ct = default)
    {
        var page = await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new IngestJobDto
            {
                Id = item.Id,
                DatasetId = item.DatasetId,
                Name = item.Name,
                Code = item.Code,
                Strategy = item.Strategy,
                CronExpression = item.CronExpression,
                BatchSize = item.BatchSize,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

        await FillDatasetNamesAsync(page.Items, ct);
        return page;
    }

    public async Task<IReadOnlyList<DataHubLookupItemDto>> ListLookupAsync(CancellationToken ct = default)
    {
        var rows = await Repo.AsQueryable()
            .Where(item => item.Status == StatusEnum.Enable)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Name, item.Code })
            .ToListAsync(ct);

        return rows.Select(item => new DataHubLookupItemDto
        {
            Value = item.Id.ToString(),
            Label = string.IsNullOrWhiteSpace(item.Code) ? item.Name : $"{item.Name}（{item.Code}）"
        }).ToList();
    }

    public async Task CreateAsync(SaveIngestJobInput input, CancellationToken ct = default)
    {
        await EnsureDatasetExistsAsync(input.DatasetId, ct);
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new DhIngestJob
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            DatasetId = input.DatasetId,
            Name = input.Name,
            Code = input.Code,
            Strategy = input.Strategy,
            CronExpression = input.CronExpression,
            BatchSize = input.BatchSize <= 0 ? 1000 : input.BatchSize,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    public async Task UpdateAsync(long id, SaveIngestJobInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("采集任务不存在");
        await EnsureDatasetExistsAsync(input.DatasetId, ct);
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.DatasetId = input.DatasetId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Strategy = input.Strategy;
        entity.CronExpression = input.CronExpression;
        entity.BatchSize = input.BatchSize <= 0 ? 1000 : input.BatchSize;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var inUse = await Db.Queryable<DhIngestRun>().Where(item => item.JobId == id).AnyAsync(ct);
        if (inUse) throw new BusinessException("采集任务已有运行记录，无法删除");

        var affected = await Db.Deleteable<DhIngestJob>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("采集任务不存在");
    }

    public Task<TriggerIngestJobResultDto> TriggerRunAsync(long id, CancellationToken ct = default)
        => ingestJobRunner.ExecuteAsync(id, ct);

    private async Task FillDatasetNamesAsync(IEnumerable<IngestJobDto> items, CancellationToken ct)
    {
        if (!items.Any()) return;
        var ids = items.Select(item => item.DatasetId).Distinct().ToList();
        var rows = await Db.Queryable<DhDataset>()
            .Where(item => ids.Contains(item.Id))
            .Select(item => new { item.Id, item.Name, item.Code })
            .ToListAsync(ct);
        var map = rows.ToDictionary(item => item.Id, item => item);
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.DatasetId, out var dataset)) continue;
            item.DatasetName = dataset.Name;
            item.DatasetCode = dataset.Code;
        }
    }

    private async Task EnsureDatasetExistsAsync(long datasetId, CancellationToken ct)
    {
        if (datasetId <= 0) throw new BusinessException("请选择数据集");
        var exists = await Db.Queryable<DhDataset>().Where(item => item.Id == datasetId).AnyAsync(ct);
        if (!exists) throw new BusinessException("所选数据集不存在");
    }

    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"采集任务编码 '{code}' 已存在");
    }
}
