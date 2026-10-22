using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Domain;
using Axis.Infrastructure.Domain.Interfaces;
using Axis.Infrastructure.Exceptions;
using SqlSugar;

namespace Axis.Application.Common;

/// <summary>
/// 通用应用服务基类，提供常见的增删改查与批量删除等基础操作。
/// 派生类可根据需要覆盖方法或提供 DTO 映射逻辑。
/// </summary>
/// <typeparam name="TEntity">实体类型，建议继承自 BaseEntityId 或 BaseEntity</typeparam>
public class BaseAppService<TEntity> where TEntity : BaseEntityId, new()
{
    protected readonly IRepository<TEntity> Repo;
    protected readonly ISqlSugarClient Db;
    protected readonly ICurrentAccount CurrentAccount;

    public BaseAppService(IRepository<TEntity> repo, ISqlSugarClient db, ICurrentAccount currentAccount)
    {
        Repo = repo ?? throw new ArgumentNullException(nameof(repo));
        Db = db ?? throw new ArgumentNullException(nameof(db));
        CurrentAccount = currentAccount ?? throw new ArgumentNullException(nameof(currentAccount));
    }

    /// <summary>
    /// 根据 Id 获取实体
    /// </summary>
    public virtual async Task<TEntity?> GetAsync(long id, CancellationToken ct = default)
    {
        return await Repo.AsQueryable().Where(u => u.Id == id).FirstAsync(ct);
    }

    /// <summary>
    /// 创建实体
    /// </summary>
    public virtual async Task CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新实体（整对象更新）；实现 <see cref="IVersion"/> 时走乐观锁。
    /// </summary>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Id == entity.Id, ct);
        if (!exists) throw new BusinessException("记录不存在");

        var updateable = Db.Updateable(entity);
        if (entity is IVersion)
            await updateable.ExecuteWithOptLockAsync(ct);
        else
            await updateable.ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    public virtual async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Id == id, ct);
        if (!exists) throw new BusinessException("记录不存在");
        await Db.Deleteable<TEntity>().Where(u => u.Id == id).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 批量删除实体
    /// </summary>
    public virtual async Task DeleteManyAsync(IEnumerable<long> ids, CancellationToken ct = default)
    {
        if (ids == null) throw new ArgumentNullException(nameof(ids));
        var idArray = ids.Where(x => x > 0).Distinct().ToArray();
        if (idArray.Length == 0) return;
        await Db.Deleteable<TEntity>().In(idArray).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 简单分页查询辅助方法。
    /// 派生类可传入自定义的 selector 来构造查询并映射到 DTO。
    /// </summary>
    protected virtual async Task<SqlSugarPagedList<TResult>> PageAsync<TResult>(Func<ISugarQueryable<TEntity>, ISugarQueryable<TResult>> selector, int page, int pageSize)
    {
        if (selector == null) throw new ArgumentNullException(nameof(selector));
        var query = selector(Repo.AsQueryable());
        return await query.ToPagedListAsync(page, pageSize);
    }
}
