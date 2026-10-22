using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database;

namespace Axis.Domain.System.Seeds;

/// <summary>
/// 通知公告种子（欢迎公告，默认已发布）。
/// </summary>
public sealed class SysNoticeSeed : ISqlSugarEntitySeedData<SysNotice>
{
    public const long WelcomeNoticeId = 1000_0012_0000_00001L;

    public IEnumerable<SysNotice> HasData()
    {
        yield return new SysNotice
        {
            Id = WelcomeNoticeId,
            Title = "欢迎使用云枢 Axis · MES",
            Content = "制造执行控制台已就绪。可在主数据维护物料档案，后续将接入工艺、工单与报工；右上角铃铛与「我的消息」可查看未读。",
            NoticeType = NoticeTypeEnum.Announcement,
            TargetType = NoticeTargetTypeEnum.All,
            TargetIds = null,
            PublishTime = DateTime.Now,
            OrderNo = 1,
            Status = StatusEnum.Enable,
            Remark = "内置欢迎公告",
            CreatedBy = 0,
            Creator = "system",
            CreatedTime = DateTime.Now
        };
    }
}
