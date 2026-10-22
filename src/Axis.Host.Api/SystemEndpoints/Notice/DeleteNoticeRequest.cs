namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 删除通知公告请求。
/// </summary>
public sealed class DeleteNoticeRequest
{
    /// <summary>公告主键。</summary>
    public long Id { get; set; }
}
