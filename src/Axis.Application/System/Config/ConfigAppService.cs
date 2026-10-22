using Axis.Application.Common;
using Axis.Application.System.Config.Dto;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Config;

/// <summary>
/// 参数配置应用服务。
/// </summary>
/// <remarks>
/// 写库成功后同步刷新配置缓存；业务侧按 code 取值走 <see cref="ISysConfigCache"/>。
/// </remarks>
public sealed class ConfigAppService(
    IRepository<SysConfig> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ISysConfigCache configCache)
    : BaseAppService<SysConfig>(repository, db, currentAccount), IConfigAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询参数配置，投影为 <see cref="ConfigDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>参数配置 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<ConfigDto>> PageAsync(ConfigPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(!string.IsNullOrWhiteSpace(query.GroupCode), item => item.GroupCode == query.GroupCode)
            .WhereIF(query.Type.HasValue, item => item.Type == query.Type!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new ConfigDto
            {
                Id = item.Id,
                GroupCode = item.GroupCode,
                Name = item.Name,
                Code = item.Code,
                Value = item.Value,
                Type = item.Type,
                IsBuiltin = item.IsBuiltin,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 校验编码唯一后创建参数配置。
    /// </summary>
    /// <param name="input">新增字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    public async Task CreateAsync(SaveConfigInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new SysConfig
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            GroupCode = input.GroupCode,
            Name = input.Name,
            Code = input.Code,
            Value = input.Value,
            Type = input.Type,
            IsBuiltin = false,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);

        await configCache.RefreshAsync(input.Code, ct);
    }

    /// <summary>
    /// 更新指定参数配置。
    /// </summary>
    /// <param name="id">参数主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">参数不存在或编码冲突时抛出。</exception>
    public async Task UpdateAsync(long id, SaveConfigInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("参数配置不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        var oldCode = entity.Code;
        entity.GroupCode = input.GroupCode;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Value = input.Value;
        entity.Type = input.Type;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        // 编码变化时失效旧键，避免旧 Code 缓存残留
        if (!string.Equals(oldCode, input.Code, StringComparison.Ordinal))
            await configCache.InvalidateAsync(oldCode, ct);
        await configCache.RefreshAsync(input.Code, ct);
    }

    /// <summary>
    /// 删除参数配置；内置参数不允许删除。
    /// </summary>
    /// <param name="id">参数主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">参数不存在或为内置参数时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("参数配置不存在");
        if (entity.IsBuiltin) throw new BusinessException("内置参数不允许删除");
        await Db.Deleteable<SysConfig>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        await configCache.InvalidateAsync(entity.Code, ct);
    }

    /// <summary>
    /// 校验同一租户下的参数编码唯一性。
    /// </summary>
    /// <param name="code">待校验编码。</param>
    /// <param name="excludedId">更新时排除的参数 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    private async Task EnsureCodeUniqueAsync(string code, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"参数编码 '{code}' 已存在");
    }
}
