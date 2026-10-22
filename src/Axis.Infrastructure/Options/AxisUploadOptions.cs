namespace Axis.Infrastructure.Options;

/// <summary>
/// 上传文件配置。
/// </summary>
public class AxisUploadOptions
{
    /// <summary>
    /// 本地存储根路径。
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 单文件大小上限（字节）。
    /// </summary>
    public long MaxSize { get; set; }

    /// <summary>
    /// 允许的 Content-Type 列表。
    /// </summary>
    public List<string> ContentType { get; set; }

    /// <summary>
    /// 是否启用文件 MD5 校验（用于防重复上传等）。
    /// </summary>
    public bool EnableMd5 { get; set; }
}
