using Axis.Application.Common;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
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
    ICurrentAccount currentAccount)
    : BaseAppService<DhIngestJob>(repository, db, currentAccount), IIngestJobAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询采集任务，投影为 <see cref="IngestJobDto"/>。
    /// </summary>
    public async Task<SqlSugarPagedList<IngestJobDto>> PageAsync(IngestJobPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
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

    /// <summary>
    /// 创建采集任务。
    /// </summary>
    public async Task CreateAsync(SaveIngestJobInput input, CancellationToken ct = default)
    {
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
            BatchSize = input.BatchSize,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定采集任务。
    /// </summary>
    public async Task UpdateAsync(long id, SaveIngestJobInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("采集任务不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.DatasetId = input.DatasetId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Strategy = input.Strategy;
        entity.CronExpression = input.CronExpression;
        entity.BatchSize = input.BatchSize;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除采集任务。
    /// </summary>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<DhIngestJob>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("采集任务不存在");
    }

    /// <summary>
    /// 校验非空编码在当前租户内唯一。
    /// </summary>
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
