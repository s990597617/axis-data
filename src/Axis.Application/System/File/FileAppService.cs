using Axis.Application.System.File.Dto;
using Axis.Domain.Data.System.Repository;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FileStorage;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Axis.Application.System.File;

/// <summary>
/// 通用文件用例：校验上传配置，委托存储服务，并提供管理端列表/下载/删除。
/// </summary>
public sealed class FileAppService(
    IFileStorageService fileStorage,
    ISysFileRepository fileRepo,
    IOptions<AxisUploadOptions> uploadOptions) : IFileAppService, IAutoInject
{
    /// <summary>
    /// 上传文件并返回可访问地址。
    /// </summary>
    /// <param name="command">上传命令（流、文件名、模块等）。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task<UploadFileResultDto> UploadAsync(UploadFileCommand command, CancellationToken ct = default)
    {
        if (command.Stream is null || command.Size <= 0)
            throw new BusinessException("请选择要上传的文件");

        var options = uploadOptions.Value;
        if (options.MaxSize > 0 && command.Size > options.MaxSize)
            throw new BusinessException($"文件大小不能超过 {options.MaxSize / 1024 / 1024}MB");

        // 细粒度类型校验交给存储层（支持 Content-Type + 扩展名回退）
        var module = string.IsNullOrWhiteSpace(command.Module) ? "common" : command.Module.Trim();
        var result = await fileStorage.UploadAsync(
            command.Stream,
            command.FileName,
            command.ContentType,
            module,
            ct);

        return new UploadFileResultDto
        {
            FileId = result.FileId,
            OriginalName = result.OriginalName,
            Url = result.Url,
            RelativePath = result.RelativePath,
            Size = result.Size,
            ContentType = result.ContentType
        };
    }

    /// <summary>
    /// 分页查询未删除的文件记录。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task<SqlSugarPagedList<FileDto>> PageAsync(FilePageQuery query, CancellationToken ct = default)
    {
        var page = await fileRepo.PageAsync(query.Keyword, query.Module, query.Page, query.PageSize, ct);
        return new SqlSugarPagedList<FileDto>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            TotalPages = page.TotalPages,
            HasNextPage = page.HasNextPage,
            HasPrevPage = page.HasPrevPage,
            Items = page.Items.Select(item => new FileDto
            {
                Id = item.Id,
                FileId = item.FileId,
                OriginalName = item.OriginalName,
                StoredName = item.StoredName,
                RelativePath = item.RelativePath,
                Url = item.Url,
                Size = item.Size,
                ContentType = item.ContentType,
                Extension = item.Extension,
                Module = item.Module,
                Provider = item.Provider,
                CreatedTime = item.CreatedTime,
                Creator = item.Creator
            }).ToList()
        };
    }

    /// <summary>
    /// 列出各业务模块的文件数量。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public async Task<List<FileModuleStatDto>> ListModulesAsync(CancellationToken ct = default)
    {
        var rows = await fileRepo.ListModuleStatsAsync(ct);
        return rows.Select(row => new FileModuleStatDto
        {
            Module = string.IsNullOrWhiteSpace(row.Module) ? string.Empty : row.Module.Trim(),
            Count = row.Count
        }).ToList();
    }

    /// <summary>
    /// 按 FileId 打开下载流。
    /// </summary>
    /// <param name="fileId">文件业务标识。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task<FileDownloadDto> OpenDownloadAsync(string fileId, CancellationToken ct = default)
    {
        var entity = await FindActiveAsync(fileId, ct);
        var stream = await fileStorage.GetAsync(entity.RelativePath, ct);
        if (stream is null)
            throw new BusinessException("物理文件不存在或已被清理");

        return new FileDownloadDto
        {
            Stream = stream,
            FileName = entity.OriginalName,
            ContentType = string.IsNullOrWhiteSpace(entity.ContentType)
                ? "application/octet-stream"
                : entity.ContentType
        };
    }

    /// <summary>
    /// 删除物理文件并标记数据库记录删除。
    /// </summary>
    /// <param name="fileId">文件业务标识。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task DeleteAsync(string fileId, CancellationToken ct = default)
    {
        var entity = await FindActiveAsync(fileId, ct);
        var deleted = await fileStorage.DeleteAsync(entity.RelativePath, ct);
        if (!deleted)
        {
            // 物理文件已不存在时仍标记库记录，避免管理台残留脏数据
            await fileRepo.SoftDeleteByPathAsync(entity.RelativePath, ct);
        }
    }

    private async Task<Domain.System.SysFile> FindActiveAsync(string fileId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new BusinessException("文件标识无效");

        var entity = await fileRepo.FindByFileIdAsync(fileId.Trim(), ct);
        if (entity is null || entity.Deleted)
            throw new BusinessException("文件不存在或已删除");

        return entity;
    }
}
