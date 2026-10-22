using System.Security.Cryptography;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using Microsoft.AspNetCore.Http;
using Yitter.IdGenerator;

namespace Axis.Infrastructure.Components.FileStorage;

/// <summary>
/// 本地磁盘文件存储实现
/// </summary>
[Injection(ServiceLifetime.Scoped)]
public class LocalFileStorageService : IFileStorageService, IAutoInject
{
    private readonly AxisUploadOptions _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IOptions<AxisUploadOptions> options,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LocalFileStorageService> logger)
    {
        _options = options.Value;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<FileStorageResult> UploadAsync(IFormFile file, string module = null, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            throw new BusinessException(ResultCodes.BadRequest, "上传文件不能为空");

        ValidateFile(file.FileName, file.Length, file.ContentType);

        await using var stream = file.OpenReadStream();
        return await SaveFileAsync(stream, file.FileName, file.ContentType, file.Length, module, ct);
    }

    public async Task<FileStorageResult> UploadAsync(Stream stream, string fileName, string contentType,
        string module = null, CancellationToken ct = default)
    {
        if (stream is null || stream.Length == 0)
            throw new BusinessException(ResultCodes.BadRequest, "上传文件流不能为空");

        ValidateFile(fileName, stream.Length, contentType);

        return await SaveFileAsync(stream, fileName, contentType, stream.Length, module, ct);
    }

    public Task<Stream> GetAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream>(null);

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream>(stream);
    }

    public string GetUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
            return $"/{_options.Path}/{relativePath}".Replace("\\", "/");

        var baseUrl = $"{request.Scheme}://{request.Host}";
        return $"{baseUrl}/{_options.Path}/{relativePath}".Replace("\\", "/");
    }

    public Task<bool> DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        try
        {
            File.Delete(fullPath);
            _logger.LogInformation("文件已删除：{Path}", relativePath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件失败：{Path}", relativePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> ExistsAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        return Task.FromResult(File.Exists(fullPath));
    }

    #region 私有方法

    /// <summary>
    /// 校验文件合法性
    /// </summary>
    private void ValidateFile(string fileName, long fileSize, string contentType)
    {
        // 校验文件大小
        if (_options.MaxSize > 0 && fileSize > _options.MaxSize)
            throw new BusinessException(ResultCodes.BadRequest,
                $"文件大小超出限制，最大允许 {_options.MaxSize / 1024 / 1024}MB");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext))
            throw new BusinessException(ResultCodes.BadRequest, "文件缺少扩展名");

        // 校验文件类型：优先 Content-Type；浏览器上报 octet-stream / 空值时回退按扩展名判断
        if (_options.ContentType is { Count: > 0 })
        {
            var type = contentType?.Trim() ?? string.Empty;
            var typeAllowed = !string.IsNullOrWhiteSpace(type)
                              && !string.Equals(type, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
                              && _options.ContentType.Contains(type, StringComparer.OrdinalIgnoreCase);

            var extAllowed = IsExtensionAllowed(ext);
            if (!typeAllowed && !extAllowed)
                throw new BusinessException(ResultCodes.BadRequest,
                    $"不支持的文件类型：{type}（{ext}）");
        }
    }

    /// <summary>
    /// 根据扩展名判断是否在允许的 ContentType 对应范围内。
    /// </summary>
    private bool IsExtensionAllowed(string extension)
    {
        var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = ["image/jpeg"],
            [".jpeg"] = ["image/jpeg"],
            [".png"] = ["image/png"],
            [".gif"] = ["image/gif"],
            [".webp"] = ["image/webp"],
            [".bmp"] = ["image/bmp"],
            [".pdf"] = ["application/pdf"],
            [".doc"] = ["application/msword"],
            [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            [".xls"] = ["application/vnd.ms-excel"],
            [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            [".ppt"] = ["application/vnd.ms-powerpoint"],
            [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
            [".txt"] = ["text/plain"],
            [".csv"] = ["text/csv"],
            [".zip"] = ["application/zip", "application/x-zip-compressed"]
        };

        if (!map.TryGetValue(extension, out var types))
            return false;

        return types.Any(type => _options.ContentType.Contains(type, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 保存文件到磁盘
    /// </summary>
    private async Task<FileStorageResult> SaveFileAsync(Stream stream, string fileName, string contentType,
        long fileSize, string module, CancellationToken ct)
    {
        var fileId = YitIdHelper.NextId().ToString();
        var ext = Path.GetExtension(fileName);
        var storedName = $"{fileId}{ext}";

        // 按日期+模块归档：module/yyyy-MM/fileId.ext
        var datePart = DateTime.Now.ToString("yyyy-MM");
        var relativeParts = string.IsNullOrWhiteSpace(module)
            ? Path.Combine(datePart, storedName)
            : Path.Combine(module, datePart, storedName);

        var fullPath = GetFullPath(relativeParts);
        var directory = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // 计算 MD5（可选）
        string md5 = null;
        if (_options.EnableMd5)
        {
            md5 = await ComputeMd5Async(stream, ct);
            stream.Position = 0;
        }

        // 写入磁盘
        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, ct);

        _logger.LogInformation("文件上传成功：{FileName} → {StoredPath}，Size={Size}", fileName, relativeParts, fileSize);

        return new FileStorageResult
        {
            FileId = fileId,
            OriginalName = fileName,
            StoredName = storedName,
            RelativePath = relativeParts.Replace("\\", "/"),
            Url = GetUrl(relativeParts),
            Size = fileSize,
            ContentType = contentType,
            Md5 = md5,
            Extension = ext
        };
    }

    /// <summary>
    /// 获取文件完整物理路径
    /// </summary>
    private string GetFullPath(string relativePath)
    {
        var rootPath = Path.IsPathRooted(_options.Path)
            ? _options.Path
            : Path.Combine(AppContext.BaseDirectory, _options.Path);

        return Path.Combine(rootPath, relativePath);
    }

    /// <summary>
    /// 计算文件 MD5
    /// </summary>
    private static async Task<string> ComputeMd5Async(Stream stream, CancellationToken ct)
    {
        var hash = await MD5.HashDataAsync(stream, ct);
        return Convert.ToHexStringLower(hash);
    }

    #endregion
}
