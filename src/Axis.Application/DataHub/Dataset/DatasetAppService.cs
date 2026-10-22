using Axis.Application.Common;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Domain.DataHub;
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
    /// <summary>
    /// 按条件分页查询数据集，投影为 <see cref="DatasetDto"/>。
    /// </summary>
    public async Task<SqlSugarPagedList<DatasetDto>> PageAsync(DatasetPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
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

    /// <summary>
    /// 创建数据集。
    /// </summary>
    public async Task CreateAsync(SaveDatasetInput input, CancellationToken ct = default)
    {
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

    /// <summary>
    /// 更新指定数据集。
    /// </summary>
    public async Task UpdateAsync(long id, SaveDatasetInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("数据集不存在");
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

    /// <summary>
    /// 删除数据集。
    /// </summary>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<DhDataset>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("数据集不存在");
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
        if (exists) throw new BusinessException($"数据集编码 '{code}' 已存在");
    }
}
