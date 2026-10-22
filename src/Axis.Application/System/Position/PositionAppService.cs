using Axis.Application.Common;
using Axis.Application.System.Position.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Position;

/// <summary>
/// 岗位管理应用服务。
/// </summary>
public sealed class PositionAppService(
    IRepository<SysPos> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysPos>(repository, db, currentAccount), IPositionAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询岗位，投影为 <see cref="PositionDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>岗位 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<PositionDto>> PageAsync(PositionPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new PositionDto
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
    /// 创建岗位；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    public async Task CreateAsync(SavePositionInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new SysPos
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
    /// 更新指定岗位。
    /// </summary>
    /// <param name="id">岗位主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">岗位不存在或编码冲突时抛出。</exception>
    public async Task UpdateAsync(long id, SavePositionInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("岗位不存在");
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
    /// 删除岗位；已被用户使用时不允许删除。
    /// </summary>
    /// <param name="id">岗位主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">岗位不存在或仍被用户使用时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var isUsed = await Db.Queryable<SysUser>().AnyAsync(user => user.PosId == id, ct);
        if (isUsed) throw new BusinessException("岗位已被用户使用，不能删除");

        var affected = await Db.Deleteable<SysPos>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("岗位不存在");
    }

    /// <summary>
    /// 校验非空岗位编码在当前租户内唯一。
    /// </summary>
    /// <param name="code">待校验编码；空则跳过。</param>
    /// <param name="excludedId">更新时排除的岗位 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"岗位编码 '{code}' 已存在");
    }
}
