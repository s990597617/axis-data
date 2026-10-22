using Axis.Application.Common;
using Axis.Application.__Area__.__Module__.Dto;
using Axis.Domain.__Area__;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.__Area__.__Module__;

/// <summary>
/// __CnName__管理应用服务。
/// </summary>
public sealed class __Module__AppService(
    IRepository<__Entity__> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<__Entity__>(repository, db, currentAccount), I__Module__AppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询__CnName__，投影为 <see cref="__Module__Dto"/>。
    /// </summary>
    public async Task<SqlSugarPagedList<__Module__Dto>> PageAsync(__Module__PageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            __FIELD_KEYWORD_WHERE__
            __FIELD_STATUS_WHERE__
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new __Module__Dto
            {
__FIELD_SELECT_PROPS__
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <summary>
    /// 创建__CnName__。
    /// </summary>
    public async Task CreateAsync(Save__Module__Input input, CancellationToken ct = default)
    {
__FIELD_CODE_UNIQUE_CREATE__        await Db.Insertable(new __Entity__
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
__FIELD_CREATE_ASSIGNS__
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定__CnName__。
    /// </summary>
    public async Task UpdateAsync(long id, Save__Module__Input input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("__CnName__不存在");
__FIELD_CODE_UNIQUE_UPDATE__
__FIELD_UPDATE_ASSIGNS__
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除__CnName__。
    /// </summary>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<__Entity__>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("__CnName__不存在");
    }
__FIELD_CODE_UNIQUE_METHOD__}
