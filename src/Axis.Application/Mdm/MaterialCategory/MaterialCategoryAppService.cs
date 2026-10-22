using Axis.Application.Common;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;

namespace Axis.Application.Mdm.MaterialCategory;

/// <summary>
/// 物料分类应用服务。
/// </summary>
/// <remarks>
/// 负责分类树维护与启用分类树查询；HTTP 由 Endpoint 处理。
/// </remarks>
public sealed class MaterialCategoryAppService(
    IRepository<MdmMaterialCategory> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IMapper mapper)
    : BaseAppService<MdmMaterialCategory>(repository, db, currentAccount),
      IMaterialCategoryAppService,
      IAutoInject
{
    /// <summary>
    /// 按条件查询分类平铺列表，投影为 <see cref="MaterialCategoryDto"/>。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>分类 DTO 只读列表，由前端组装树。</returns>
    public async Task<IReadOnlyList<MaterialCategoryDto>> ListAsync(
        MaterialCategoryListQuery query,
        CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .Select(item => new MaterialCategoryDto
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                Code = item.Code,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToListAsync(ct);

    /// <summary>
    /// 查询全部启用分类并组装为树。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>根级分类树节点列表。</returns>
    public async Task<IReadOnlyList<MaterialCategoryTreeNode>> GetTreeAsync(CancellationToken ct = default)
    {
        var categories = await Repo.AsQueryable()
            .Where(item => item.Status == StatusEnum.Enable)
            .OrderBy(item => item.OrderNo)
            .ToListAsync(ct);
        return BuildTree(categories, 0);
    }

    /// <summary>
    /// 校验上级与编码后创建分类。
    /// </summary>
    /// <param name="input">新增字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task CreateAsync(SaveMaterialCategoryInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureCodeUniqueAsync(input.Code, tenantId, null, ct);

        var entity = mapper.Map<MdmMaterialCategory>(input);
        entity.Id = 0;
        entity.TenantId = tenantId;
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定分类；禁止把节点移动到自身下级形成环。
    /// </summary>
    /// <param name="id">分类主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task UpdateAsync(long id, SaveMaterialCategoryInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("分类不存在");
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureNoCycleAsync(id, input.ParentId, ct);
        await EnsureCodeUniqueAsync(input.Code, tenantId, id, ct);

        mapper.Map(input, entity);
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除分类；存在下级或仍被物料引用时不允许删除。
    /// </summary>
    /// <param name="id">分类主键。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        RequireTenant();
        var hasChildren = await Repo.AsQueryable().AnyAsync(item => item.ParentId == id, ct);
        if (hasChildren) throw new BusinessException("请先删除当前分类的下级分类");

        var hasMaterials = await Db.Queryable<MdmMaterial>().AnyAsync(item => item.CategoryId == id, ct);
        if (hasMaterials) throw new BusinessException("分类下仍有物料档案，不能删除");

        var affected = await Db.Deleteable<MdmMaterialCategory>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("分类不存在");
    }

    /// <summary>
    /// 分类为租户强隔离数据，必须绑定当前租户。
    /// </summary>
    /// <returns>当前租户 Id。</returns>
    private long RequireTenant()
        => CurrentAccount.TenantId
           ?? throw new BusinessException("当前账号未绑定租户，不能维护物料分类");

    /// <summary>
    /// 递归组装分类树。
    /// </summary>
    /// <param name="categories">已加载的启用分类平铺列表。</param>
    /// <param name="parentId">当前父节点 Id。</param>
    /// <returns>指定父节点下的子树。</returns>
    private static List<MaterialCategoryTreeNode> BuildTree(
        IReadOnlyList<MdmMaterialCategory> categories,
        long parentId)
        => categories
            .Where(item => item.ParentId == parentId)
            .OrderBy(item => item.OrderNo)
            .Select(item => new MaterialCategoryTreeNode
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                Code = item.Code,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Children = BuildTree(categories, item.Id)
            })
            .ToList();

    /// <summary>
    /// 校验非根上级分类存在。
    /// </summary>
    /// <param name="parentId">上级分类 Id；0 表示根。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureParentExistsAsync(long parentId, CancellationToken ct)
    {
        if (parentId == 0) return;
        if (!await Repo.AsQueryable().AnyAsync(item => item.Id == parentId, ct))
            throw new BusinessException("上级分类不存在");
    }

    /// <summary>
    /// 沿父链向上检查，防止分类树形成循环引用。
    /// </summary>
    /// <param name="id">当前分类 Id。</param>
    /// <param name="parentId">目标上级 Id。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureNoCycleAsync(long id, long parentId, CancellationToken ct)
    {
        var nodes = await Repo.AsQueryable().Select(item => new { item.Id, item.ParentId }).ToListAsync(ct);
        var parents = nodes.ToDictionary(item => item.Id, item => item.ParentId);
        var cursor = parentId;
        while (cursor != 0 && parents.TryGetValue(cursor, out var next))
        {
            if (cursor == id) throw new BusinessException("上级分类不能选择当前分类或其下级分类");
            cursor = next;
        }
    }

    /// <summary>
    /// 校验分类编码在指定租户内唯一。
    /// </summary>
    /// <param name="code">分类编码。</param>
    /// <param name="tenantId">租户 Id。</param>
    /// <param name="excludedId">更新时排除的主键。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureCodeUniqueAsync(string code, long tenantId, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code && item.TenantId == tenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"分类编码 '{code}' 已存在");
    }
}
