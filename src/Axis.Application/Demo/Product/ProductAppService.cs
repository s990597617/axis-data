using Axis.Application.Common;
using Axis.Application.Demo.Product.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.Demo.Product;

/// <summary>
/// 产品管理应用服务。
/// </summary>
public sealed class ProductAppService(
    IRepository<SysProduct> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysProduct>(repository, db, currentAccount), IProductAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询产品，投影为 <see cref="ProductDto"/>。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>产品 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<ProductDto>> PageAsync(ProductPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new ProductDto
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
    /// 创建产品；编码非空时须在当前租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码冲突时抛出。</exception>
    public async Task CreateAsync(SaveProductInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        await Db.Insertable(new SysProduct
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
    /// 更新指定产品。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    public async Task UpdateAsync(long id, SaveProductInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("产品不存在");
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
    /// 删除产品。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var affected = await Db.Deleteable<SysProduct>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("产品不存在");
    }

    /// <summary>
    /// 校验非空编码在当前租户内唯一。
    /// </summary>
    /// <param name="code">待校验编码；空则跳过。</param>
    /// <param name="excludedId">更新时排除的 Id；新增时为空。</param>
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
        if (exists) throw new BusinessException($"产品编码 '{code}' 已存在");
    }
}
