using Axis.Application.Common;
using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Dataset.Dto;
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

namespace Axis.Application.DataHub.Dataset;

/// <summary>
/// 数据集管理应用服务。
/// </summary>
public sealed class DatasetAppService(
    IRepository<DhDataset> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<DhDataset>(repository, db, currentAccount), IDatasetAppService, IAutoInject
{
    public async Task<SqlSugarPagedList<DatasetDto>> PageAsync(DatasetPageQuery query, CancellationToken ct = default)
    {
        var page = await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new DatasetDto
            {
                Id = item.Id,
                SourceId = item.SourceId,
                Name = item.Name,
                Code = item.Code,
                ObjectType = item.ObjectType,
                ObjectName = item.ObjectName,
                CustomSql = item.CustomSql,
                PrimaryKeyFields = item.PrimaryKeyFields,
                IncrementalField = item.IncrementalField,
                UpdateStrategy = item.UpdateStrategy,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

        await FillSourceNamesAsync(page.Items, ct);
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

    public async Task CreateAsync(SaveDatasetInput input, CancellationToken ct = default)
    {
        await EnsureSourceExistsAsync(input.SourceId, ct);
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new DhDataset
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            SourceId = input.SourceId,
            Name = input.Name,
            Code = input.Code,
            ObjectType = input.ObjectType,
            ObjectName = input.ObjectName,
            CustomSql = input.CustomSql,
            PrimaryKeyFields = input.PrimaryKeyFields,
            IncrementalField = input.IncrementalField,
            UpdateStrategy = input.UpdateStrategy,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    public async Task UpdateAsync(long id, SaveDatasetInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("数据集不存在");
        await EnsureSourceExistsAsync(input.SourceId, ct);
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.SourceId = input.SourceId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.ObjectType = input.ObjectType;
        entity.ObjectName = input.ObjectName;
        entity.CustomSql = input.CustomSql;
        entity.PrimaryKeyFields = input.PrimaryKeyFields;
        entity.IncrementalField = input.IncrementalField;
        entity.UpdateStrategy = input.UpdateStrategy;
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
        var inUse = await Db.Queryable<DhIngestJob>().Where(item => item.DatasetId == id).AnyAsync(ct);
        if (inUse) throw new BusinessException("数据集已被采集任务引用，无法删除");

        var affected = await Db.Deleteable<DhDataset>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("数据集不存在");
    }

    private async Task FillSourceNamesAsync(IEnumerable<DatasetDto> items, CancellationToken ct)
    {
        if (!items.Any()) return;
        var ids = items.Select(item => item.SourceId).Distinct().ToList();
        var rows = await Db.Queryable<DhSource>()
            .Where(item => ids.Contains(item.Id))
            .Select(item => new { item.Id, item.Name, item.Code })
            .ToListAsync(ct);
        var map = rows.ToDictionary(item => item.Id, item => item);
        foreach (var item in items)
        {
            if (!map.TryGetValue(item.SourceId, out var source)) continue;
            item.SourceName = source.Name;
            item.SourceCode = source.Code;
        }
    }

    private async Task EnsureSourceExistsAsync(long sourceId, CancellationToken ct)
    {
        if (sourceId <= 0) throw new BusinessException("请选择数据源");
        var exists = await Db.Queryable<DhSource>().Where(item => item.Id == sourceId).AnyAsync(ct);
        if (!exists) throw new BusinessException("所选数据源不存在");
    }

    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"数据集编码 '{code}' 已存在");
    }
}
