using Axis.Application.Common;
using Axis.Application.System.Org.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Org;

/// <summary>
/// 机构管理应用服务。
/// </summary>
/// <remarks>
/// 负责机构树维护与启用机构树查询；HTTP 由 Endpoint 处理。
/// </remarks>
public sealed class OrgAppService(
    IRepository<SysOrg> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysOrg>(repository, db, currentAccount), IOrgAppService, IAutoInject
{
    /// <summary>
    /// 按条件查询机构平铺列表，投影为 <see cref="OrgDto"/>。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>机构 DTO 只读列表，由前端组装树。</returns>
    public async Task<IReadOnlyList<OrgDto>> ListAsync(OrgListQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Type.HasValue, item => item.Type == query.Type!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .Select(item => new OrgDto
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                Code = item.Code,
                Type = item.Type,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToListAsync(ct);

    /// <summary>
    /// 查询全部启用机构并组装为树。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>根级机构树节点列表。</returns>
    public async Task<IReadOnlyList<OrgTreeNode>> GetTreeAsync(CancellationToken ct = default)
    {
        var orgs = await Repo.AsQueryable()
            .Where(item => item.Status == StatusEnum.Enable)
            .OrderBy(item => item.OrderNo)
            .ToListAsync(ct);
        return BuildOrgTree(orgs, 0);
    }

    /// <summary>
    /// 校验上级与编码后创建机构。
    /// </summary>
    /// <param name="input">新增字段；编码非空时须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">上级不存在或编码冲突时抛出。</exception>
    public async Task CreateAsync(SaveOrgInput input, CancellationToken ct = default)
    {
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new SysOrg
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            ParentId = input.ParentId,
            Name = input.Name,
            Code = input.Code,
            Type = input.Type,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定机构；禁止把节点移动到自身下级形成环。
    /// </summary>
    /// <param name="id">机构主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">机构不存在、形成环或编码冲突时抛出。</exception>
    public async Task UpdateAsync(long id, SaveOrgInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("机构不存在");
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureNoCycleAsync(id, input.ParentId, ct);
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.ParentId = input.ParentId;
        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Type = input.Type;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除机构；存在下级或下属用户时不允许删除。
    /// </summary>
    /// <param name="id">机构主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">存在下级、下属用户或机构不存在时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var hasChildren = await Repo.AsQueryable().AnyAsync(item => item.ParentId == id, ct);
        if (hasChildren) throw new BusinessException("请先删除当前机构的下级机构");

        var hasUsers = await Db.Queryable<SysUser>().AnyAsync(item => item.OrgId == id, ct);
        if (hasUsers) throw new BusinessException("机构下仍有用户，不能删除");

        var affected = await Db.Deleteable<SysOrg>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("机构不存在");
    }

    /// <summary>
    /// 递归组装机构树。
    /// </summary>
    /// <param name="orgs">已加载的启用机构平铺列表。</param>
    /// <param name="parentId">当前父节点 Id。</param>
    /// <returns>指定父节点下的子树。</returns>
    private static List<OrgTreeNode> BuildOrgTree(IReadOnlyList<SysOrg> orgs, long parentId)
        => orgs
            .Where(item => item.ParentId == parentId)
            .OrderBy(item => item.OrderNo)
            .Select(item => new OrgTreeNode
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                Code = item.Code,
                Type = item.Type,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Children = BuildOrgTree(orgs, item.Id)
            })
            .ToList();

    /// <summary>
    /// 校验非根上级机构存在。
    /// </summary>
    /// <param name="parentId">上级机构 Id；0 表示根。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">上级不存在时抛出。</exception>
    private async Task EnsureParentExistsAsync(long parentId, CancellationToken ct)
    {
        if (parentId == 0) return;
        if (!await Repo.AsQueryable().AnyAsync(item => item.Id == parentId, ct))
            throw new BusinessException("上级机构不存在");
    }

    /// <summary>
    /// 沿父链向上检查，防止机构树形成循环引用。
    /// </summary>
    /// <param name="id">当前机构 Id。</param>
    /// <param name="parentId">目标上级 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">形成环时抛出。</exception>
    private async Task EnsureNoCycleAsync(long id, long parentId, CancellationToken ct)
    {
        var nodes = await Repo.AsQueryable().Select(item => new { item.Id, item.ParentId }).ToListAsync(ct);
        var parents = nodes.ToDictionary(item => item.Id, item => item.ParentId);
        var cursor = parentId;
        while (cursor != 0 && parents.TryGetValue(cursor, out var next))
        {
            if (cursor == id) throw new BusinessException("上级机构不能选择当前机构或其下级机构");
            cursor = next;
        }
    }

    /// <summary>
    /// 校验非空机构编码在当前租户内唯一。
    /// </summary>
    /// <param name="code">待校验编码；空则跳过。</param>
    /// <param name="excludedId">更新时排除的机构 Id；新增时为空。</param>
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
        if (exists) throw new BusinessException($"机构编码 '{code}' 已存在");
    }
}
