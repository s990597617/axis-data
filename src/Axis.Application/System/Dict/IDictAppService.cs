using Axis.Application.System.Dict.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Dict;

/// <summary>
/// 字典管理应用服务契约。
/// </summary>
/// <remarks>
/// 负责字典类型与数据项维护；不直接处理 HTTP。
/// </remarks>
public interface IDictAppService
{
    /// <summary>
    /// 按条件分页查询字典类型列表。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>字典类型 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<DictTypeDto>> PageTypesAsync(DictTypePageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 校验编码唯一后创建字典类型。
    /// </summary>
    /// <param name="input">新增字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">编码已存在时抛出。</exception>
    Task CreateTypeAsync(SaveDictTypeInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定字典类型的主表字段。
    /// </summary>
    /// <param name="id">字典类型主键。</param>
    /// <param name="input">更新字段；编码须在当前租户内唯一（排除自身）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在或编码冲突时抛出。</exception>
    Task UpdateTypeAsync(long id, SaveDictTypeInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除字典类型；内置类型或仍有数据项时不允许删除。
    /// </summary>
    /// <param name="id">字典类型主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在、内置或仍有数据项时抛出。</exception>
    Task DeleteTypeAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 查询指定字典类型下的数据项列表。
    /// </summary>
    /// <param name="query">类型 Id 与可选筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>数据项 DTO 只读列表。</returns>
    Task<IReadOnlyList<DictDataDto>> ListDataAsync(DictDataListQuery query, CancellationToken ct = default);

    /// <summary>
    /// 在指定类型下创建字典数据项；必要时清除其它默认项。
    /// </summary>
    /// <param name="input">新增字段；同类型下 Value 须唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">类型不存在或 Value 冲突时抛出。</exception>
    Task CreateDataAsync(SaveDictDataInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定字典数据项。
    /// </summary>
    /// <param name="id">数据项主键。</param>
    /// <param name="input">更新字段；同类型下 Value 须唯一（排除自身）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">数据项不存在、类型无效或 Value 冲突时抛出。</exception>
    Task UpdateDataAsync(long id, SaveDictDataInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除指定字典数据项。
    /// </summary>
    /// <param name="id">数据项主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">数据项不存在时抛出。</exception>
    Task DeleteDataAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 按字典类型编码读取启用数据项（走缓存，供业务下拉与翻译）。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用数据项；类型不存在时为空列表。</returns>
    Task<IReadOnlyList<DictCacheItemDto>> ListItemsByCodeAsync(string code, CancellationToken ct = default);
}
