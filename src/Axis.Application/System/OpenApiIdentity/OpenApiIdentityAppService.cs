using System.Security.Cryptography;
using Axis.Application.Common;
using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.OpenApiIdentity;

/// <summary>
/// 开放接口身份管理应用服务。
/// </summary>
public sealed class OpenApiIdentityAppService(
    ISysOpenApiIdentityRepository repository,
    IRepository<SysUser> userRepo,
    IRepository<SysTenant> tenantRepo,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysOpenApiIdentity>(repository, db, currentAccount),
      IOpenApiIdentityAppService,
      IAutoInject
{
    /// <inheritdoc />
    public async Task<SqlSugarPagedList<OpenApiIdentityDto>> PageAsync(
        OpenApiIdentityPageQuery query,
        CancellationToken ct = default)
    {
        return await Repo.AsQueryable()
            .LeftJoin<SysUser>((item, user) => item.UserId == user.Id)
            .LeftJoin<SysTenant>((item, user, tenant) => item.TenantId == tenant.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), (item, user, tenant) =>
                item.AccessKey.Contains(query.Keyword!)
                || (item.Remark != null && item.Remark.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, (item, user, tenant) => item.Status == query.Status!.Value)
            .OrderByDescending((item, user, tenant) => item.CreatedTime)
            .Select((item, user, tenant) => new OpenApiIdentityDto
            {
                Id = item.Id,
                AccessKey = item.AccessKey,
                TenantId = item.TenantId,
                TenantName = tenant.Name,
                UserId = item.UserId,
                UserAccount = user.Account,
                UserRealName = user.RealName,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <inheritdoc />
    public async Task<CreateOpenApiIdentityResult> CreateAsync(
        SaveOpenApiIdentityInput input,
        CancellationToken ct = default)
    {
        await EnsureAccessKeyUniqueAsync(input.AccessKey, null, ct);
        await EnsureUserExistsAsync(input.UserId, ct);
        await EnsureTenantExistsAsync(input.TenantId, ct);

        var secret = string.IsNullOrWhiteSpace(input.AccessSecret)
            ? GenerateSecret()
            : input.AccessSecret.Trim();
        var id = YitIdHelper.NextId();

        await Db.Insertable(new SysOpenApiIdentity
        {
            Id = id,
            AccessKey = input.AccessKey.Trim(),
            AccessSecret = secret,
            TenantId = input.TenantId,
            UserId = input.UserId,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);

        return new CreateOpenApiIdentityResult
        {
            Id = id,
            AccessKey = input.AccessKey.Trim(),
            AccessSecret = secret
        };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(long id, SaveOpenApiIdentityInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("开放接口身份不存在");

        await EnsureAccessKeyUniqueAsync(input.AccessKey, id, ct);
        await EnsureUserExistsAsync(input.UserId, ct);
        await EnsureTenantExistsAsync(input.TenantId, ct);

        entity.AccessKey = input.AccessKey.Trim();
        entity.TenantId = input.TenantId;
        entity.UserId = input.UserId;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<SysOpenApiIdentity>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("开放接口身份不存在");
    }

    /// <inheritdoc />
    public async Task<ResetOpenApiSecretResult> ResetSecretAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("开放接口身份不存在");

        var secret = GenerateSecret();
        entity.AccessSecret = secret;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        return new ResetOpenApiSecretResult
        {
            AccessKey = entity.AccessKey,
            AccessSecret = secret
        };
    }

    private async Task EnsureAccessKeyUniqueAsync(string accessKey, long? excludedId, CancellationToken ct)
    {
        var key = accessKey.Trim();
        var exists = await Repo.AsQueryable()
            .ClearFilter()
            .Where(item => item.AccessKey == key)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"身份标识 '{key}' 已存在");
    }

    private async Task EnsureUserExistsAsync(long userId, CancellationToken ct)
    {
        var exists = await userRepo.AsQueryable()
            .ClearFilter()
            .AnyAsync(item => item.Id == userId && !item.Deleted, ct);
        if (!exists) throw new BusinessException("绑定用户不存在");
    }

    private async Task EnsureTenantExistsAsync(long? tenantId, CancellationToken ct)
    {
        if (!tenantId.HasValue || tenantId.Value <= 0) return;
        var exists = await tenantRepo.AsQueryable()
            .ClearFilter()
            .AnyAsync(item => item.Id == tenantId.Value && !item.Deleted, ct);
        if (!exists) throw new BusinessException("绑定租户不存在");
    }

    private static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
