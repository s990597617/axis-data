using Axis.Application.Common;
using Axis.Application.DataHub.IngestRun.Dto;
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

namespace Axis.Application.DataHub.IngestRun;

/// <summary>
/// 采集运行管理应用服务。
/// </summary>
public sealed class IngestRunAppService(
    IRepository<DhIngestRun> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<DhIngestRun>(repository, db, currentAccount), IIngestRunAppService, IAutoInject
{
    public async Task<SqlSugarPagedList<IngestRunDto>> PageAsync(IngestRunPageQuery query, CancellationToken ct = default)
    {
        var page = await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .WhereIF(query.JobId.HasValue, item => item.JobId == query.JobId!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new IngestRunDto
            {
                Id = item.Id,
                JobId = item.JobId,
                Name = item.Name,
                Code = item.Code,
                RunStatus = item.RunStatus,
                StartedTime = item.StartedTime,
                FinishedTime = item.FinishedTime,
                RowCountRead = item.RowCountRead,
                RowCountChanged = item.RowCountChanged,
                ErrorMessage = item.ErrorMessage,
                Watermark = item.Watermark,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

        await FillJobNamesAsync(page.Items, ct);
        return page;
    }

    public async Task CreateAsync(SaveIngestRunInput input, CancellationToken ct = default)
    {
        var job = await EnsureJobExistsAsync(input.JobId, ct);
        var stamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var name = string.IsNullOrWhiteSpace(input.Name) ? $"{job.Name}-{stamp}" : input.Name!;
        var jobCode = string.IsNullOrWhiteSpace(job.Code) ? job.Id.ToString() : job.Code;
        var code = string.IsNullOrWhiteSpace(input.Code) ? $"run-{jobCode}-{stamp}" : input.Code!;
        await EnsureCodeUniqueAsync(code, null, ct);

        await Db.Insertable(new DhIngestRun
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            JobId = input.JobId,
            Name = name,
            Code = code,
            RunStatus = input.RunStatus == 0 ? DhRunStatusEnum.Pending : input.RunStatus,
            StartedTime = input.StartedTime,
            FinishedTime = input.FinishedTime,
            RowCountRead = input.RowCountRead,
            RowCountChanged = input.RowCountChanged,
            ErrorMessage = input.ErrorMessage,
            Watermark = input.Watermark,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    public async Task UpdateAsync(long id, SaveIngestRunInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("采集运行不存在");
        await EnsureJobExistsAsync(input.JobId, ct);
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Code))
            throw new BusinessException("名称和编码不能为空");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.JobId = input.JobId;
        entity.Name = input.Name!;
        entity.Code = input.Code!;
        entity.RunStatus = input.RunStatus;
        entity.StartedTime = input.StartedTime;
        entity.FinishedTime = input.FinishedTime;
        entity.RowCountRead = input.RowCountRead;
        entity.RowCountChanged = input.RowCountChanged;
        entity.ErrorMessage = input.ErrorMessage;
        entity.Watermark = input.Watermark;
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
        var affected = await Db.Deleteable<DhIngestRun>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("采集运行不存在");
    }

    private async Task FillJobNamesAsync(IEnumerable<IngestRunDto> items, CancellationToken ct)
    {
        if (!items.Any()) return;
        var ids = items.Select(item => item.JobId).Distinct().ToList();
        var rows = await Db.Queryable<DhIngestJob>()
            .Where(item => ids.Contains(item.Id))
            .Select(item => new { item.Id, item.Name, item.Code })
            .ToListAsync(ct);
        var map = rows.ToDictionary(item => item.Id, item => item);
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.JobId, out var job)) continue;
            item.JobName = job.Name;
            item.JobCode = job.Code;
        }
    }

    private async Task<DhIngestJob> EnsureJobExistsAsync(long jobId, CancellationToken ct)
    {
        if (jobId <= 0) throw new BusinessException("请选择采集任务");
        return await Db.Queryable<DhIngestJob>().Where(item => item.Id == jobId).FirstAsync(ct)
               ?? throw new BusinessException("所选采集任务不存在");
    }

    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"采集运行编码 '{code}' 已存在");
    }
}
