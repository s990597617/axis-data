namespace Axis.Application.System.Notice.Dto;

/// <summary>
/// 发布或下架通知公告的请求体。
/// </summary>
/// <remarks>
/// 主键通常由路由绑定到 <see cref="Id"/>；本输入仅承载发布开关。
/// </remarks>
public sealed class PublishNoticeInput
{
    /// <summary>
    /// 公告主键；由路由绑定，请求体可省略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 是否发布；<c>true</c> 启用并推送未读，<c>false</c> 下架停用。
    /// </summary>
    public bool Published { get; set; }
}
