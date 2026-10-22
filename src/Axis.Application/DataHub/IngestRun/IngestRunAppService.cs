using Axis.Application.Common;
using Axis.Application.DataHub.IngestRun.Dto;
using Axis.Domain.DataHub;
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
    /// <summary>
    /// 按条件分页查询采集运行，投影为 <see cref="IngestRunDto"/>。
    /// </summary>
    public async Task<SqlSugarPagedList<IngestRunDto>> PageAsync(IngestRunPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
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

    /// <summary>
    /// 创建采集运行。
    /// </summary>
    public async Task CreateAsync(SaveIngestRunInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new DhIngestRun
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            JobId = input.JobId,
            Name = input.Name,
            Code = input.Code,
            RunStatus = input.RunStatus,
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

    /// <summary>
    /// 更新指定采集运行。
    /// </summary>
    public async Task UpdateAsync(long id, SaveIngestRunInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("采集运行不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.JobId = input.JobId;
        entity.Name = input.Name;
        entity.Code = input.Code;
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

    /// <summary>
    /// 删除采集运行。
    /// </summary>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<DhIngestRun>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("采集运行不存在");
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
        if (exists) throw new BusinessException($"采集运行编码 '{code}' 已存在");
    }
}
