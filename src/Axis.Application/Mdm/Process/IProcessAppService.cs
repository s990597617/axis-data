using Axis.Application.Mdm.Process.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Mdm.Process;

/// <summary>
/// 工序管理应用服务契约。
/// </summary>
public interface IProcessAppService
{
    /// <summary>
    /// 按条件分页查询工序。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>工序 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<ProcessDto>> PageAsync(ProcessPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 预览工序下一编码；不占用 PRC 流水。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览编码。</returns>
    /// <exception cref="BusinessException">未绑定租户或规则不可用时抛出。</exception>
    Task<ProcessNextCodeDto> PreviewNextCodeAsync(CancellationToken ct = default);

    /// <summary>
    /// 创建工序；编码留空则按规则 PRC 取号，非空须租户内唯一。
    /// </summary>
    /// <param name="input">新增字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">未绑定租户或编码冲突时抛出。</exception>
    Task CreateAsync(SaveProcessInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定工序。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveProcessInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除工序。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">记录不存在或仍被工艺路线引用时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 查询启用中的工序，供工艺路线等下拉使用。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用工序的只读列表。</returns>
    Task<IReadOnlyList<ProcessOptionDto>> ListEnabledAsync(CancellationToken ct = default);
}
