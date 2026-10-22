using Axis.Application.Common;
using Axis.Application.Mdm.Material.Dto;
using Axis.Application.System.CodeRule;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;

namespace Axis.Application.Mdm.Material;

/// <summary>
/// 物料管理应用服务。
/// </summary>
/// <remarks>
/// 编码留空走 <see cref="ICodeGenerator"/>（规则 <c>MAT</c>）；计量单位与来源类型校验字典缓存。
/// </remarks>
public sealed class MaterialAppService(
    IRepository<MdmMaterial> repository,
    IRepository<MdmMaterialCategory> categoryRepo,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    ICodeGenerator codeGenerator,
    ISysDictCache dictCache,
    IMapper mapper)
    : BaseAppService<MdmMaterial>(repository, db, currentAccount), IMaterialAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询物料，投影为 <see cref="MaterialDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>物料 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<MaterialDto>> PageAsync(MaterialPageQuery query, CancellationToken ct = default)
    {
        var categoryIds = await ResolveCategoryFilterAsync(query.CategoryId, ct);
        return await Repo.AsQueryable()
            .LeftJoin<MdmMaterialCategory>((item, category) => item.CategoryId == category.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), (item, category) =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(!string.IsNullOrWhiteSpace(query.Unit), (item, category) => item.Unit == query.Unit)
            .WhereIF(!string.IsNullOrWhiteSpace(query.SourceType), (item, category) =>
                item.SourceType == query.SourceType)
            .WhereIF(query.Status.HasValue, (item, category) => item.Status == query.Status!.Value)
            .WhereIF(categoryIds is not null, (item, category) =>
                item.CategoryId != null && categoryIds!.Contains(item.CategoryId.Value))
            .OrderBy((item, category) => item.OrderNo)
            .OrderBy((item, category) => item.CreatedTime, OrderByType.Desc)
            .Select((item, category) => new MaterialDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Spec = item.Spec,
                CategoryId = item.CategoryId,
                CategoryName = category.Name,
                Unit = item.Unit,
                SourceType = item.SourceType,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <summary>
    /// 预览物料下一编码；只读流水，不占用。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    public async Task<MaterialNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default)
    {
        RequireTenant();
        var preview = await codeGenerator.PreviewAsync(MdmCodeRuleCodes.Material, ct: ct);
        return new MaterialNextCodeDto { Code = preview.Sample };
    }

    /// <summary>
    /// 创建物料；编码留空则占用 MAT 流水。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task CreateAsync(SaveMaterialInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var categoryId = NormalizeCategoryId(input.CategoryId);
        await EnsureUnitAsync(input.Unit, ct);
        await EnsureSourceTypeAsync(input.SourceType, ct);
        await EnsureCategoryAsync(categoryId, ct);
        var code = await ResolveNewCodeAsync(input.Code, ct);
        await EnsureCodeUniqueAsync(code, tenantId, null, ct);

        var entity = mapper.Map<MdmMaterial>(input);
        entity.Id = 0;
        entity.TenantId = tenantId;
        entity.Code = code;
        entity.CategoryId = categoryId;
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定物料；编码留空则保留原值。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task UpdateAsync(long id, SaveMaterialInput input, CancellationToken ct = default)
    {
        var tenantId = RequireTenant();
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("物料档案不存在");
        var categoryId = NormalizeCategoryId(input.CategoryId);
        await EnsureUnitAsync(input.Unit, ct);
        await EnsureSourceTypeAsync(input.SourceType, ct);
        await EnsureCategoryAsync(categoryId, ct);
        var code = string.IsNullOrWhiteSpace(input.Code) ? entity.Code : input.Code.Trim();
        await EnsureCodeUniqueAsync(code, tenantId, id, ct);

        mapper.Map(input, entity);
        entity.Code = code;
        entity.CategoryId = categoryId;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除物料。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<MdmMaterial>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("物料档案不存在");
    }

    /// <summary>
    /// 物料为租户强隔离数据，必须绑定当前租户。
    /// </summary>
    /// <returns>当前租户 Id。</returns>
    private long RequireTenant()
        => CurrentAccount.TenantId
           ?? throw new BusinessException("当前账号未绑定租户，不能维护物料档案");

    /// <summary>
    /// 空或 0 视为未分类。
    /// </summary>
    /// <param name="categoryId">前端传入的分类 Id。</param>
    /// <returns>规范化后的分类 Id。</returns>
    private static long? NormalizeCategoryId(long? categoryId)
        => categoryId is null or 0 ? null : categoryId;

    /// <summary>
    /// 收集分类筛选范围：选中节点及其子孙。
    /// </summary>
    /// <param name="categoryId">筛选分类；空或 0 表示不过滤。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>需要匹配的分类 Id 列表；不过滤时为空。</returns>
    private async Task<List<long>?> ResolveCategoryFilterAsync(long? categoryId, CancellationToken ct)
    {
        var rootId = NormalizeCategoryId(categoryId);
        if (rootId is null) return null;

        var nodes = await categoryRepo.AsQueryable()
            .Select(item => new { item.Id, item.ParentId })
            .ToListAsync(ct);
        var childrenByParent = nodes.ToLookup(item => item.ParentId);
        var ids = new List<long>();
        var stack = new Stack<long>();
        stack.Push(rootId.Value);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            ids.Add(current);
            foreach (var child in childrenByParent[current])
                stack.Push(child.Id);
        }

        return ids;
    }

    /// <summary>
    /// 手填编码则去空白；否则占用 MAT 流水。
    /// </summary>
    /// <param name="code">用户输入编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>最终落库编码。</returns>
    private async Task<string> ResolveNewCodeAsync(string? code, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(code))
            return code.Trim();

        var generated = await codeGenerator.NextAsync(MdmCodeRuleCodes.Material, ct: ct);
        return generated.Value;
    }

    /// <summary>
    /// 校验计量单位属于启用中的物料单位字典。
    /// </summary>
    /// <param name="unit">字典 Value。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureUnitAsync(string unit, CancellationToken ct)
    {
        var items = await dictCache.GetItemsAsync(MdmDictCodes.MaterialUnit, ct);
        if (items.All(item => !string.Equals(item.Value, unit, StringComparison.Ordinal)))
            throw new BusinessException($"计量单位 '{unit}' 不在字典 {MdmDictCodes.MaterialUnit} 中");
    }

    /// <summary>
    /// 校验来源类型属于启用中的物料来源字典。
    /// </summary>
    /// <param name="sourceType">字典 Value。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureSourceTypeAsync(string sourceType, CancellationToken ct)
    {
        var items = await dictCache.GetItemsAsync(MdmDictCodes.MaterialSource, ct);
        if (items.All(item => !string.Equals(item.Value, sourceType, StringComparison.Ordinal)))
            throw new BusinessException($"来源类型 '{sourceType}' 不在字典 {MdmDictCodes.MaterialSource} 中");
    }

    /// <summary>
    /// 校验分类存在且启用；未分类时跳过。
    /// </summary>
    /// <param name="categoryId">规范化后的分类 Id。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureCategoryAsync(long? categoryId, CancellationToken ct)
    {
        if (categoryId is null) return;
        var category = await categoryRepo.AsQueryable()
                           .Where(item => item.Id == categoryId.Value)
                           .FirstAsync(ct)
                       ?? throw new BusinessException("物料分类不存在");
        if (category.Status != StatusEnum.Enable)
            throw new BusinessException("物料分类已停用");
    }

    /// <summary>
    /// 校验编码在指定租户内唯一。
    /// </summary>
    /// <param name="code">物料编码。</param>
    /// <param name="tenantId">租户 Id。</param>
    /// <param name="excludedId">更新时排除的主键。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task EnsureCodeUniqueAsync(string code, long tenantId, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code && item.TenantId == tenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"物料编码 '{code}' 已存在");
    }
}
