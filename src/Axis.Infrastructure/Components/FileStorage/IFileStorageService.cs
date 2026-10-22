using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FileStorage;

/// <summary>
/// 文件存储服务抽象接口。
/// <para>支持本地磁盘、MinIO、阿里 OSS、AWS S3 等多种后端实现。</para>
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">HTTP 上传文件</param>
    /// <param name="module">业务模块名（用于归档子目录，如 "avatar"、"attachment"）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileStorageResult> UploadAsync(IFormFile file, string module = null, CancellationToken ct = default);

    /// <summary>
    /// 上传文件（流方式）
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="contentType">文件 ContentType</param>
    /// <param name="module">业务模块名</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>上传结果</returns>
    Task<FileStorageResult> UploadAsync(Stream stream, string fileName, string contentType, string module = null,
        CancellationToken ct = default);

    /// <summary>
    /// 获取文件流
    /// </summary>
    /// <param name="relativePath">文件相对路径（由上传返回）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>文件流，不存在时返回 null</returns>
    Task<Stream> GetAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// 获取文件可访问 URL
    /// </summary>
    /// <param name="relativePath">文件相对路径</param>
    /// <returns>完整可访问 URL</returns>
    string GetUrl(string relativePath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="relativePath">文件相对路径</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="relativePath">文件相对路径</param>
    /// <param name="ct">取消令牌</param>
    Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default);
}
