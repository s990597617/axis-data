using Axis.Application.Mdm.Routing.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Mdm.Routing;

/// <summary>
/// 工艺路线应用服务契约。
/// </summary>
public interface IRoutingAppService
{
    /// <summary>
    /// 按条件分页查询工艺路线头。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>路线 DTO 分页结果；不含工序行。</returns>
    Task<SqlSugarPagedList<RoutingDto>> PageAsync(RoutingPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 获取工艺路线详情（含工序行）。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>含工序行的路线。</returns>
    /// <exception cref="BusinessException">记录不存在时抛出。</exception>
    Task<RoutingDto> GetDetailAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 预览路线下一编码；不占用 RTG 流水。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    Task<RoutingNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建工艺路线及工序行；编码留空则按规则 RTG 取号。
    /// </summary>
    /// <param name="input">头与行。</param>
    /// <param name="ct">取消令牌。</param>
    Task CreateAsync(SaveRoutingInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新工艺路线；先删后插工序行。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">头与行。</param>
    /// <param name="ct">取消令牌。</param>
    Task UpdateAsync(long id, SaveRoutingInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除工艺路线及其工序行。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task DeleteAsync(long id, CancellationToken ct = default);
}
