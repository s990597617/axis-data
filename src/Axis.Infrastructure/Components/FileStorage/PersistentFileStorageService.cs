using Axis.Infrastructure.Components.FileStorage.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FileStorage;

/// <summary>
/// 持久化装饰器：在文件上传/删除时自动同步数据库记录。
/// <para>
/// 通过装饰器模式包装 <see cref="IFileStorageService"/>，实现存储与入库解耦。
/// 内部委托实际的文件存取操作给被装饰者（Local / MinIO / OSS 等），
/// 在操作前后处理数据库记录的写入与状态变更。
/// </para>
/// </summary>
public class PersistentFileStorageService : IFileStorageService
{
    private readonly IFileStorageService _inner;
    private readonly IFileRecordRepository _recordRepo;
    private readonly ILogger<PersistentFileStorageService> _logger;
    private readonly string _providerName;

    public PersistentFileStorageService(
        IFileStorageService inner,
        IFileRecordRepository recordRepo,
        ILogger<PersistentFileStorageService> logger,
        string providerName = "local")
    {
        _inner = inner;
        _recordRepo = recordRepo;
        _logger = logger;
        _providerName = providerName;
    }

    public async Task<FileStorageResult> UploadAsync(IFormFile file, string module = null, CancellationToken ct = default)
    {
        // 如果启用 MD5 且存在相同文件，直接秒传
        var result = await _inner.UploadAsync(file, module, ct);
        await PersistRecordAsync(result, module, ct);
        return result;
    }

    public async Task<FileStorageResult> UploadAsync(Stream stream, string fileName, string contentType,
        string module = null, CancellationToken ct = default)
    {
        var result = await _inner.UploadAsync(stream, fileName, contentType, module, ct);
        await PersistRecordAsync(result, module, ct);
        return result;
    }

    public Task<Stream> GetAsync(string relativePath, CancellationToken ct = default)
        => _inner.GetAsync(relativePath, ct);

    public string GetUrl(string relativePath)
        => _inner.GetUrl(relativePath);

    public async Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var deleted = await _inner.DeleteAsync(relativePath, ct);
        if (deleted)
        {
            await _recordRepo.SoftDeleteByPathAsync(relativePath, ct);
            _logger.LogInformation("文件记录已标记删除：{Path}", relativePath);
        }

        return deleted;
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
        => _inner.ExistsAsync(relativePath, ct);

    /// <summary>
    /// 将上传结果持久化到数据库
    /// </summary>
    private async Task PersistRecordAsync(FileStorageResult result, string module, CancellationToken ct)
    {
        try
        {
            await _recordRepo.AddAsync(new FileRecord
            {
                FileId = result.FileId,
                OriginalName = result.OriginalName,
                StoredName = result.StoredName,
                RelativePath = result.RelativePath,
                Url = result.Url,
                Size = result.Size,
                ContentType = result.ContentType,
                Extension = result.Extension,
                Md5 = result.Md5,
                Module = module,
                Provider = _providerName
            }, ct);
        }
        catch (Exception ex)
        {
            // 入库失败不影响文件已上传成功的结果，仅记录错误
            _logger.LogError(ex, "文件记录入库失败：FileId={FileId}, Path={Path}", result.FileId, result.RelativePath);
        }
    }
}
