namespace Axis.Infrastructure.Components.EventBus.Events;

/// <summary>
/// 文件上传完成事件
/// </summary>
public class FileUploadedEvent : AxisEvent
{
    /// <summary>
    /// 文件标识
    /// </summary>
    public string FileId { get; init; }

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalName { get; init; }

    /// <summary>
    /// 文件相对路径
    /// </summary>
    public string RelativePath { get; init; }

    /// <summary>
    /// 业务模块
    /// </summary>
    public string Module { get; init; }

    /// <summary>
    /// 上传者 ID
    /// </summary>
    public long? UploaderId { get; init; }
}
