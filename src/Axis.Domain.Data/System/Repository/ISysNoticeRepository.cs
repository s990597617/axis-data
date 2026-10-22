using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 通知公告仓储：管理端查询与发布范围用户解析。
/// </summary>
/// <remarks>
/// 用户侧已读/未读由 <c>sys_inbox</c> 承担，本仓储不再维护公告已读表。
/// </remarks>
public interface ISysNoticeRepository : IRepository<SysNotice>
{
    /// <summary>
    /// 管理端分页查询通知公告。
    /// </summary>
    Task<SqlSugarPagedList<SysNotice>> PageAsync(
        int page,
        int pageSize,
        string? keyword,
        NoticeTypeEnum? noticeType,
        StatusEnum? status,
        CancellationToken ct = default);

    /// <summary>
    /// 按发布范围解析应接收推送的启用用户 Id。
    /// </summary>
    /// <param name="notice">已持久化的公告实体。</param>
    /// <param name="ct">取消令牌。</param>
    Task<IReadOnlyList<long>> ResolveTargetUserIdsAsync(SysNotice notice, CancellationToken ct = default);
}
