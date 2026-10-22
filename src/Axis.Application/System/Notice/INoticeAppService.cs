using Axis.Application.System.Notice.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Notice;

/// <summary>
/// 通知公告管理端应用服务契约。
/// </summary>
/// <remarks>
/// 负责公告草稿维护、发布/下架；投递/撤回只发布 Inbox 事件，用户侧查询与已读由 Inbox 模块负责。
/// 发布后通过 <c>IUserNotifyPusher</c> 推送目标用户未读数，本接口不直接依赖 SignalR。
/// </remarks>
public interface INoticeAppService
{
    /// <summary>
    /// 按条件分页查询通知公告（管理端全量，不按当前用户可见范围过滤）。
    /// </summary>
    /// <param name="query">分页与筛选；关键字匹配标题。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>公告 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<NoticeDto>> PageAsync(NoticePageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 新增通知公告；若状态为启用则写入发布时间并立即向目标用户推送未读。
    /// </summary>
    /// <param name="input">公告字段；格式校验由 <c>SaveNoticeInputValidator</c> 完成。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>
    /// 不负责 HTTP 入参绑定；目标用户解析与推送在实现内完成。
    /// </remarks>
    Task CreateAsync(SaveNoticeInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新通知公告；由停用变为启用时视为首次发布并推送未读。
    /// </summary>
    /// <param name="id">公告主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">公告不存在时抛出。</exception>
    Task UpdateAsync(long id, SaveNoticeInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除通知公告及其已读明细，避免铃铛仍统计到已删公告。
    /// </summary>
    /// <param name="id">公告主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">公告不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 发布或下架指定通知公告；发布时刷新发布时间并向目标用户推送未读。
    /// </summary>
    /// <param name="id">公告主键。</param>
    /// <param name="publish"><c>true</c> 发布（启用）；<c>false</c> 下架（停用）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">公告不存在时抛出。</exception>
    Task PublishAsync(long id, bool publish, CancellationToken ct = default);
}
