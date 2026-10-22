using Axis.Application.Common;
using Axis.Application.Demo.PosClone.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.Demo.PosClone;

/// <summary>
/// 岗位副本管理应用服务。
/// </summary>
public sealed class PosCloneAppService(
    IRepository<SysPosClone> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysPosClone>(repository, db, currentAccount), IPosCloneAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询岗位副本，投影为 <see cref="PosCloneDto"/>。
    /// </summary>
    public async Task<SqlSugarPagedList<PosCloneDto>> PageAsync(PosClonePageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new PosCloneDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 创建岗位副本。
    /// </summary>
    public async Task CreateAsync(SavePosCloneInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new SysPosClone
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            Name = input.Name,
            Code = input.Code,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定岗位副本。
    /// </summary>
    public async Task UpdateAsync(long id, SavePosCloneInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("岗位副本不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除岗位副本。
    /// </summary>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<SysPosClone>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("岗位副本不存在");
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
        if (exists) throw new BusinessException($"岗位副本编码 '{code}' 已存在");
    }}
