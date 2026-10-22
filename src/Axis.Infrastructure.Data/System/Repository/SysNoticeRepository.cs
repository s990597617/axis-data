using System.Text.Json;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 通知公告仓储实现。
/// </summary>
public sealed class SysNoticeRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysNotice>(db, sp), ISysNoticeRepository, IAutoInject
{
    /// <inheritdoc />
    public Task<SqlSugarPagedList<SysNotice>> PageAsync(
        int page,
        int pageSize,
        string keyword,
        NoticeTypeEnum? noticeType,
        StatusEnum? status,
        CancellationToken ct = default)
        => Context.Queryable<SysNotice>()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                item.Title.Contains(keyword!) || item.Content.Contains(keyword!))
            .WhereIF(noticeType.HasValue, item => item.NoticeType == noticeType!.Value)
            .WhereIF(status.HasValue, item => item.Status == status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(page, pageSize);

    /// <inheritdoc />
    public async Task<IReadOnlyList<long>> ResolveTargetUserIdsAsync(SysNotice notice, CancellationToken ct = default)
    {
        var users = Context.Queryable<SysUser>().Where(item => item.Status == StatusEnum.Enable);
        switch (notice.TargetType)
        {
            case NoticeTargetTypeEnum.All:
                return await users.Select(item => item.Id).ToListAsync(ct);
            case NoticeTargetTypeEnum.User:
            {
                var ids = ParseIds(notice.TargetIds);
                return ids.Count == 0
                    ? []
                    : await users.Where(item => ids.Contains(item.Id)).Select(item => item.Id).ToListAsync(ct);
            }
            case NoticeTargetTypeEnum.Org:
            {
                var ids = ParseIds(notice.TargetIds);
                return ids.Count == 0
                    ? []
                    : await users.Where(item => ids.Contains(item.OrgId)).Select(item => item.Id).ToListAsync(ct);
            }
            case NoticeTargetTypeEnum.Role:
            {
                var ids = ParseIds(notice.TargetIds);
                if (ids.Count == 0) return [];
                var userIds = await Context.Queryable<SysUserRole>()
                    .Where(item => ids.Contains(item.RoleId))
                    .Select(item => item.UserId)
                    .Distinct()
                    .ToListAsync(ct);
                return userIds.Count == 0
                    ? []
                    : await users.Where(item => userIds.Contains(item.Id)).Select(item => item.Id).ToListAsync(ct);
            }
            default:
                return [];
        }
    }

    private static List<long> ParseIds(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<long>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
