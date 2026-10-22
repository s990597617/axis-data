using Microsoft.AspNetCore.Http;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 文件上传 HTTP 请求（multipart/form-data）。
/// </summary>
public sealed class UploadFileRequest
{
    /// <summary>
    /// 上传文件。
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// 业务模块归档目录（如 avatar）。
    /// </summary>
    public string? Module { get; set; }
}
