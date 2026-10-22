using Axis.Application.Common;
using Axis.Application.System.Job.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Jobs;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Job;

/// <summary>
/// 定时任务应用服务：库表为配置源，同步到 Quartz。
/// </summary>
public sealed class JobAppService(
    IRepository<SysJob> repository,
    IRepository<SysJobLog> logRepository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IAxisJobScheduler scheduler,
    AxisJobCatalog catalog)
    : BaseAppService<SysJob>(repository, db, currentAccount), IJobAppService, IAutoInject
{
    /// <inheritdoc />
    public async Task<SqlSugarPagedList<JobDto>> PageAsync(JobPageQuery query, CancellationToken ct = default)
    {
        var page = await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.JobCode.Contains(query.Keyword!))
            .WhereIF(!string.IsNullOrWhiteSpace(query.GroupName), item => item.GroupName == query.GroupName)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new JobDto
            {
                Id = item.Id,
                JobCode = item.JobCode,
                Name = item.Name,
                GroupName = item.GroupName,
                Cron = item.Cron,
                Concurrent = item.Concurrent,
                Params = item.Params,
                IsBuiltin = item.IsBuiltin,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

        foreach (var item in page.Items)
        {
            item.ExecutorRegistered = catalog.Contains(item.JobCode);
            item.NextFireTime = await scheduler.GetNextFireTimeAsync(item.JobCode, ct);
        }

        return page;
    }

    /// <inheritdoc />
    public async Task CreateAsync(SaveJobInput input, CancellationToken ct = default)
    {
        EnsureExecutor(input.JobCode);
        EnsureCron(input.Cron);
        await EnsureCodeUniqueAsync(input.JobCode, null, ct);

        var entity = new SysJob
        {
            Id = YitIdHelper.NextId(),
            JobCode = input.JobCode.Trim(),
            Name = input.Name.Trim(),
            GroupName = input.GroupName?.Trim(),
            Cron = input.Cron.Trim(),
            Concurrent = input.Concurrent,
            Params = input.Params,
            IsBuiltin = false,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        };

        await Db.Insertable(entity).ExecuteCommandAsync(ct);
        await scheduler.ScheduleOrUpdateAsync(ToDescriptor(entity), ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(long id, SaveJobInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("定时任务不存在");

        EnsureCron(input.Cron);
        if (!string.Equals(entity.JobCode, input.JobCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            if (entity.IsBuiltin)
                throw new BusinessException("内置任务不允许修改任务编码");
            EnsureExecutor(input.JobCode);
            await EnsureCodeUniqueAsync(input.JobCode, id, ct);
            await scheduler.UnscheduleAsync(entity.JobCode, ct);
            entity.JobCode = input.JobCode.Trim();
        }
        else
        {
            EnsureExecutor(entity.JobCode);
        }

        entity.Name = input.Name.Trim();
        entity.GroupName = input.GroupName?.Trim();
        entity.Cron = input.Cron.Trim();
        entity.Concurrent = input.Concurrent;
        entity.Params = input.Params;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;

        await Db.Updateable(entity).ExecuteWithOptLockAsync(ct);
        await scheduler.ScheduleOrUpdateAsync(ToDescriptor(entity), ct);
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("定时任务不存在");
        if (entity.IsBuiltin)
            throw new BusinessException("内置任务不允许删除，请改为停用");

        await Db.Deleteable<SysJob>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        await scheduler.UnscheduleAsync(entity.JobCode, ct);
    }

    /// <inheritdoc />
    public async Task ChangeStatusAsync(long id, bool enabled, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("定时任务不存在");

        EnsureExecutor(entity.JobCode);
        EnsureCron(entity.Cron);

        entity.Status = enabled ? StatusEnum.Enable : StatusEnum.Disable;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        // 列更新豁免乐观锁：启停仅改状态相关列。
        await Db.Updateable(entity)
            .UpdateColumns(item => new { item.Status, item.ModifiedBy, item.Modifier, item.ModifiedTime })
            .ExecuteCommandAsync(ct);

        await scheduler.ScheduleOrUpdateAsync(ToDescriptor(entity), ct);
        if (enabled)
            await scheduler.ResumeAsync(entity.JobCode, ct);
        else
            await scheduler.PauseAsync(entity.JobCode, ct);
    }

    /// <inheritdoc />
    public async Task RunAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("定时任务不存在");
        EnsureExecutor(entity.JobCode);

        // 确保 JobDetail 存在后再手动触发
        await scheduler.ScheduleOrUpdateAsync(ToDescriptor(entity) with { Enabled = true }, ct);
        await scheduler.TriggerAsync(entity.JobCode, manual: true, ct);
        if (entity.Status != StatusEnum.Enable)
            await scheduler.PauseAsync(entity.JobCode, ct);
    }

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<JobLogDto>> PageLogsAsync(JobLogPageQuery query, CancellationToken ct = default)
        => await logRepository.AsQueryable()
            .WhereIF(query.JobId.HasValue, item => item.JobId == query.JobId!.Value)
            .WhereIF(!string.IsNullOrWhiteSpace(query.JobCode), item => item.JobCode == query.JobCode)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.StartedTime, OrderByType.Desc)
            .Select(item => new JobLogDto
            {
                Id = item.Id,
                JobId = item.JobId,
                JobCode = item.JobCode,
                JobName = item.JobName,
                Status = item.Status,
                TriggerType = item.TriggerType,
                StartedTime = item.StartedTime,
                EndedTime = item.EndedTime,
                DurationMs = item.DurationMs,
                Message = item.Message,
                Exception = item.Exception
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <inheritdoc />
    public Task<IReadOnlyList<JobExecutorDto>> ListExecutorsAsync(CancellationToken ct = default)
    {
        var items = catalog.ListExecutors()
            .Select(item => new JobExecutorDto
            {
                Code = item.Code,
                Name = item.Name,
                GroupName = item.GroupName,
                Cron = item.Cron,
                Concurrent = item.Concurrent,
                DefaultParams = item.DefaultParams,
                Builtin = item.Builtin,
                OrderNo = item.OrderNo,
                Enabled = item.Enabled,
                Remark = item.Remark,
                HasMetadata = item.HasMetadata
            })
            .ToArray();
        return Task.FromResult<IReadOnlyList<JobExecutorDto>>(items);
    }

    /// <inheritdoc />
    public async Task EnsureCatalogSyncedAsync(CancellationToken ct = default)
    {
        var targets = catalog.ListSyncTargets();
        if (targets.Count == 0) return;

        foreach (var target in targets)
        {
            if (string.IsNullOrWhiteSpace(target.Cron))
                throw new BusinessException($"执行器「{target.Code}」缺少默认 Cron，无法自动注册");

            // 含软删，避免唯一键冲突或重复插入
            var existing = await Db.Queryable<SysJob>()
                .ClearFilter()
                .Where(item => item.JobCode == target.Code)
                .FirstAsync(ct);

            if (existing is null)
            {
                var createdBy = CurrentAccount.Id > 0 ? CurrentAccount.Id : 0L;
                var creator = string.IsNullOrWhiteSpace(CurrentAccount.RealName)
                    ? "system"
                    : CurrentAccount.RealName;

                await Db.Insertable(new SysJob
                {
                    Id = YitIdHelper.NextId(),
                    JobCode = target.Code,
                    Name = target.Name,
                    GroupName = target.GroupName,
                    Cron = target.Cron!,
                    Concurrent = target.Concurrent,
                    Params = target.DefaultParams,
                    IsBuiltin = target.Builtin,
                    OrderNo = target.OrderNo,
                    Status = target.Enabled ? StatusEnum.Enable : StatusEnum.Disable,
                    Remark = target.Remark,
                    CreatedBy = createdBy,
                    Creator = creator,
                    CreatedTime = DateTime.Now,
                    Deleted = false
                }).ExecuteCommandAsync(ct);
                continue;
            }

            var dirty = false;
            if (existing.Deleted)
            {
                existing.Deleted = false;
                // 复活后默认停用，避免误跑；运维可再启用
                existing.Status = StatusEnum.Disable;
                dirty = true;
            }

            // 仅刷新目录元数据，不覆盖 Cron / Status / Params / Concurrent / OrderNo
            if (!string.Equals(existing.Name, target.Name, StringComparison.Ordinal))
            {
                existing.Name = target.Name;
                dirty = true;
            }

            if (existing.IsBuiltin != target.Builtin)
            {
                existing.IsBuiltin = target.Builtin;
                dirty = true;
            }

            if (!string.Equals(existing.GroupName ?? string.Empty, target.GroupName ?? string.Empty, StringComparison.Ordinal))
            {
                existing.GroupName = target.GroupName;
                dirty = true;
            }

            if (!string.Equals(existing.Remark ?? string.Empty, target.Remark ?? string.Empty, StringComparison.Ordinal))
            {
                existing.Remark = target.Remark;
                dirty = true;
            }

            if (!dirty) continue;

            existing.ModifiedTime = DateTime.Now;
            existing.Modifier = string.IsNullOrWhiteSpace(CurrentAccount.RealName)
                ? "system"
                : CurrentAccount.RealName;
            if (CurrentAccount.Id > 0)
                existing.ModifiedBy = CurrentAccount.Id;

            await Db.Updateable(existing).ExecuteWithOptLockAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task SyncSchedulerAsync(CancellationToken ct = default)
    {
        await EnsureCatalogSyncedAsync(ct);
        var jobs = await Repo.AsQueryable().ToListAsync(ct);
        var descriptors = jobs.Select(ToDescriptor);
        await scheduler.SyncAllAsync(descriptors, ct);
    }

    private async Task EnsureCodeUniqueAsync(string jobCode, long? excludeId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.JobCode == jobCode.Trim())
            .WhereIF(excludeId.HasValue, item => item.Id != excludeId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"任务编码「{jobCode}」已存在");
    }

    private void EnsureExecutor(string jobCode)
    {
        if (!catalog.Contains(jobCode))
            throw new BusinessException($"未注册任务执行器「{jobCode}」，请先在代码中实现 IAxisJob");
    }

    private void EnsureCron(string cron)
    {
        if (!scheduler.IsValidCron(cron))
            throw new BusinessException($"非法 Cron 表达式：{cron}");
    }

    private static AxisJobScheduleDescriptor ToDescriptor(SysJob entity)
        => new(
            entity.Id,
            entity.JobCode,
            entity.Name,
            entity.Cron,
            entity.Concurrent,
            entity.Params,
            entity.Status == StatusEnum.Enable);
}
