using Axis.Application.Common;
using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source.Dto;
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

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源管理应用服务。
/// </summary>
public sealed class SourceAppService(
    IRepository<DhSource> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<DhSource>(repository, db, currentAccount), ISourceAppService, IAutoInject
{
    public async Task<SqlSugarPagedList<SourceDto>> PageAsync(SourcePageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new SourceDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                SourceType = item.SourceType,
                SecretRef = item.SecretRef,
                IsReadOnly = item.IsReadOnly,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

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

    public async Task CreateAsync(SaveSourceInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new DhSource
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            Name = input.Name,
            Code = input.Code,
            SourceType = input.SourceType,
            SecretRef = input.SecretRef,
            IsReadOnly = input.IsReadOnly,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    public async Task UpdateAsync(long id, SaveSourceInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("数据源不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.SourceType = input.SourceType;
        entity.SecretRef = input.SecretRef;
        entity.IsReadOnly = input.IsReadOnly;
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
        var inUse = await Db.Queryable<DhDataset>().Where(item => item.SourceId == id).AnyAsync(ct);
        if (inUse) throw new BusinessException("数据源已被数据集引用，无法删除");

        var affected = await Db.Deleteable<DhSource>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("数据源不存在");
    }

    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"数据源编码 '{code}' 已存在");
    }
}
