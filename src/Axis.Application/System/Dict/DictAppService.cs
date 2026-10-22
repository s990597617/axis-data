using Axis.Application.Common;
using Axis.Application.System.Dict.Dto;
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

namespace Axis.Application.System.Dict;

/// <summary>
/// 字典管理应用服务。
/// </summary>
/// <remarks>
/// 负责字典类型与数据项维护；写库成功后同步刷新字典缓存；HTTP 由 Endpoint 处理。
/// </remarks>
public sealed class DictAppService(
    IRepository<SysDictType> typeRepository,
    IRepository<SysDictData> dataRepository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ISysDictCache dictCache)
    : BaseAppService<SysDictType>(typeRepository, db, currentAccount), IDictAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询字典类型列表，投影为 <see cref="DictTypeDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>字典类型 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<DictTypeDto>> PageTypesAsync(DictTypePageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new DictTypeDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                IsBuiltin = item.IsBuiltin,
                IsTenant = item.IsTenant,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 校验编码唯一后创建字典类型。
    /// </summary>
    /// <param name="input">新增字段；编码须在目标归属范围内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在或归属非法时抛出。</exception>
    public async Task CreateTypeAsync(SaveDictTypeInput input, CancellationToken ct = default)
    {
        var isTenant = ResolveIsTenantOnCreate(input.IsTenant);
        var tenantId = ResolveTenantIdOnCreate(isTenant);
        await EnsureTypeCodeUniqueAsync(input.Code, tenantId, null, ct);
        await Db.Insertable(new SysDictType
        {
            Id = YitIdHelper.NextId(),
            TenantId = tenantId,
            Name = input.Name,
            Code = input.Code,
            IsBuiltin = false,
            IsTenant = isTenant,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);

        await dictCache.RefreshAsync(input.Code, CacheScope(isTenant, tenantId), ct);
    }

    /// <summary>
    /// 更新指定字典类型的主表字段。
    /// </summary>
    /// <param name="id">字典类型主键。</param>
    /// <param name="input">更新字段；编码须在当前归属范围内唯一（排除自身）。IsTenant/TenantId 创建后不可改。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在或编码冲突时抛出。</exception>
    public async Task UpdateTypeAsync(long id, SaveDictTypeInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("字典类型不存在");
        EnsureBuiltinMutable(entity);

        var code = entity.IsBuiltin ? entity.Code : input.Code;
        await EnsureTypeCodeUniqueAsync(code, entity.TenantId, id, ct);

        var oldCode = entity.Code;
        entity.Name = input.Name;
        entity.Code = code;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity)
            .IgnoreColumns(item => new { item.IsTenant, item.TenantId, item.IsBuiltin })
            .ExecuteWithOptLockAsync(ct);

        // 编码变化时同时失效旧键，避免旧 Code 缓存残留
        var scope = CacheScope(entity);
        if (!string.Equals(oldCode, code, StringComparison.Ordinal))
            await dictCache.InvalidateAsync(oldCode, scope, ct);
        await dictCache.RefreshAsync(code, scope, ct);
    }

    /// <summary>
    /// 删除字典类型；内置类型或仍有数据项时不允许删除。
    /// </summary>
    /// <param name="id">字典类型主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在、内置或仍有数据项时抛出。</exception>
    public async Task DeleteTypeAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("字典类型不存在");
        if (entity.IsBuiltin) throw new BusinessException("内置字典类型不允许删除");

        var hasData = await dataRepository.AsQueryable().AnyAsync(item => item.DictTypeId == id, ct);
        if (hasData) throw new BusinessException("字典类型下仍有数据项，请先清空数据项");
        await Db.Deleteable<SysDictType>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        await dictCache.InvalidateAsync(entity.Code, CacheScope(entity), ct);
    }

    /// <summary>
    /// 查询指定字典类型下的数据项列表。
    /// </summary>
    /// <param name="query">类型 Id 与可选筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>数据项 DTO 只读列表。</returns>
    public async Task<IReadOnlyList<DictDataDto>> ListDataAsync(DictDataListQuery query, CancellationToken ct = default)
        => await dataRepository.AsQueryable()
            .Where(item => item.DictTypeId == query.DictTypeId)
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Label.Contains(query.Keyword!) || item.Value.Contains(query.Keyword!))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .Select(item => new DictDataDto
            {
                Id = item.Id,
                DictTypeId = item.DictTypeId,
                Label = item.Label,
                Value = item.Value,
                TagStyle = item.TagStyle,
                ListClass = item.ListClass,
                IsDefault = item.IsDefault,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark
            })
            .ToListAsync(ct);

    /// <summary>
    /// 在指定类型下创建字典数据项；必要时清除其它默认项。
    /// </summary>
    /// <param name="input">新增字段；同类型下 Value 须唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在或 Value 冲突时抛出。</exception>
    public async Task CreateDataAsync(SaveDictDataInput input, CancellationToken ct = default)
    {
        await EnsureTypeWritableAsync(input.DictTypeId, ct);
        await EnsureDataValueUniqueAsync(input.DictTypeId, input.Value, null, ct);
        if (input.IsDefault) await ClearDefaultAsync(input.DictTypeId, null, ct);

        await Db.Insertable(new SysDictData
        {
            Id = YitIdHelper.NextId(),
            DictTypeId = input.DictTypeId,
            Label = input.Label,
            Value = input.Value,
            TagStyle = input.TagStyle,
            ListClass = input.ListClass,
            IsDefault = input.IsDefault,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);

        await RefreshDataCacheAsync(input.DictTypeId, ct);
    }

    /// <summary>
    /// 更新指定字典数据项。
    /// </summary>
    /// <param name="id">数据项主键。</param>
    /// <param name="input">更新字段；同类型下 Value 须唯一（排除自身）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">数据项不存在、类型无效或 Value 冲突时抛出。</exception>
    public async Task UpdateDataAsync(long id, SaveDictDataInput input, CancellationToken ct = default)
    {
        var entity = await dataRepository.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("字典数据项不存在");
        await EnsureTypeWritableAsync(input.DictTypeId, ct);
        await EnsureDataValueUniqueAsync(input.DictTypeId, input.Value, id, ct);
        if (input.IsDefault) await ClearDefaultAsync(input.DictTypeId, id, ct);

        entity.DictTypeId = input.DictTypeId;
        entity.Label = input.Label;
        entity.Value = input.Value;
        entity.TagStyle = input.TagStyle;
        entity.ListClass = input.ListClass;
        entity.IsDefault = input.IsDefault;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await dataRepository.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        await RefreshDataCacheAsync(input.DictTypeId, ct);
    }

    /// <summary>
    /// 删除指定字典数据项。
    /// </summary>
    /// <param name="id">数据项主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">数据项不存在时抛出。</exception>
    public async Task DeleteDataAsync(long id, CancellationToken ct = default)
    {
        var typeId = await dataRepository.AsQueryable()
            .Where(item => item.Id == id)
            .Select(item => item.DictTypeId)
            .FirstAsync(ct);
        if (typeId <= 0) throw new BusinessException("字典数据项不存在");

        await EnsureTypeWritableAsync(typeId, ct);

        var affected = await Db.Deleteable<SysDictData>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("字典数据项不存在");

        await RefreshDataCacheAsync(typeId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DictCacheItemDto>> ListItemsByCodeAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return [];

        var items = await dictCache.GetItemsAsync(code.Trim(), ct);
        return items.Select(item => new DictCacheItemDto
        {
            Label = item.Label,
            Value = item.Value,
            TagStyle = item.TagStyle,
            ListClass = item.ListClass,
            IsDefault = item.IsDefault,
            OrderNo = item.OrderNo
        }).ToList();
    }

    /// <summary>
    /// 解析创建时的租户字典标记：非超管强制为租户字典。
    /// </summary>
    private bool ResolveIsTenantOnCreate(bool requested)
        => CurrentAccount.IsSuperAdmin ? requested : true;

    /// <summary>
    /// 按是否租户字典解析写入的 TenantId。
    /// </summary>
    private long? ResolveTenantIdOnCreate(bool isTenant)
    {
        if (!isTenant) return null;
        return CurrentAccount.TenantId
               ?? throw new BusinessException("当前账号未绑定租户，无法创建租户字典");
    }

    /// <summary>
    /// 校验字典类型编码在目标归属范围内唯一（全局与租户各自独立）。
    /// </summary>
    /// <param name="code">待校验编码。</param>
    /// <param name="tenantId">目标 TenantId；null 表示全平台。</param>
    /// <param name="excludedId">更新时排除的类型 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    private async Task EnsureTypeCodeUniqueAsync(string code, long? tenantId, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .Where(item => item.TenantId == tenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"字典编码 '{code}' 已存在");
    }

    /// <summary>
    /// 内置字典仅超级管理员可改；非内置任意有权限用户可改。
    /// </summary>
    private void EnsureBuiltinMutable(SysDictType type)
    {
        if (type.IsBuiltin && !CurrentAccount.IsSuperAdmin)
            throw new BusinessException("内置字典仅超级管理员可修改");
    }

    /// <summary>
    /// 校验字典类型存在，且当前用户有权对其写数据项。
    /// </summary>
    private async Task EnsureTypeWritableAsync(long typeId, CancellationToken ct)
    {
        var type = await Repo.AsQueryable().Where(item => item.Id == typeId).FirstAsync(ct)
                   ?? throw new BusinessException("字典类型不存在");
        EnsureBuiltinMutable(type);
    }

    /// <summary>
    /// 校验字典类型存在。
    /// </summary>
    /// <param name="typeId">字典类型主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在时抛出。</exception>
    private async Task EnsureTypeExistsAsync(long typeId, CancellationToken ct)
    {
        if (!await Repo.AsQueryable().AnyAsync(item => item.Id == typeId, ct))
            throw new BusinessException("字典类型不存在");
    }

    /// <summary>
    /// 校验同一字典类型下的数据值唯一。
    /// </summary>
    /// <param name="typeId">字典类型主键。</param>
    /// <param name="value">待校验存储值。</param>
    /// <param name="excludedId">更新时排除的数据项 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">Value 已存在时抛出。</exception>
    private async Task EnsureDataValueUniqueAsync(long typeId, string value, long? excludedId, CancellationToken ct)
    {
        var exists = await dataRepository.AsQueryable()
            .Where(item => item.DictTypeId == typeId && item.Value == value)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"字典值 '{value}' 已存在");
    }

    /// <summary>
    /// 清除同一字典类型下的其它默认项，保证最多只有一个默认值。
    /// </summary>
    /// <param name="typeId">字典类型主键。</param>
    /// <param name="excludedId">保留默认的数据项 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task ClearDefaultAsync(long typeId, long? excludedId, CancellationToken ct)
    {
        // 列更新豁免乐观锁：批量清除默认标记，非详情编辑。
        await dataRepository.AsUpdateable()
            .SetColumns(item => new SysDictData { IsDefault = false })
            .Where(item => item.DictTypeId == typeId && item.IsDefault)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 数据项写库后按所属类型编码刷新字典缓存。
    /// </summary>
    /// <param name="typeId">字典类型主键。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task RefreshDataCacheAsync(long typeId, CancellationToken ct)
    {
        var type = await Repo.AsQueryable()
            .Where(item => item.Id == typeId)
            .Select(item => new { item.Code, item.TenantId, item.IsTenant })
            .FirstAsync(ct);
        if (!string.IsNullOrWhiteSpace(type.Code))
            await dictCache.RefreshAsync(type.Code, CacheScope(type.IsTenant, type.TenantId), ct);
    }

    /// <summary>
    /// 缓存 scope：租户字典用 TenantId，平台字典用 null（对应键 g）。
    /// </summary>
    private static long? CacheScope(SysDictType entity)
        => CacheScope(entity.IsTenant, entity.TenantId);

    private static long? CacheScope(bool isTenant, long? tenantId)
        => isTenant ? tenantId : null;
}
