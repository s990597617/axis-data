using Axis.Application.Mdm.Material.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Mdm.Material;

/// <summary>
/// 物料管理应用服务契约。
/// </summary>
public interface IMaterialAppService
{
    /// <summary>
    /// 按条件分页查询物料。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>物料 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<MaterialDto>> PageAsync(MaterialPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 预览物料下一编码；不占用 MAT 流水。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    /// <exception cref="BusinessException">未绑定租户或规则不可用时抛出。</exception>
    Task<MaterialNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建物料；编码留空则按规则 MAT 取号，非空须租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">未绑定租户、单位/来源/分类无效或编码冲突时抛出。</exception>
    Task CreateAsync(SaveMaterialInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定物料。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在、单位/来源/分类无效或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveMaterialInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除物料。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
