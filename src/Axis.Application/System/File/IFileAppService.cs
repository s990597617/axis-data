using Axis.Application.System.File.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.System.File;

/// <summary>
/// 通用文件应用服务：上传、分页管理、下载与删除。
/// </summary>
public interface IFileAppService
{
    /// <summary>
    /// 上传文件并返回可访问地址。
    /// </summary>
    /// <param name="command">上传命令（流、文件名、模块等）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>上传结果，含 FileId 与访问 URL。</returns>
    Task<UploadFileResultDto> UploadAsync(UploadFileCommand command, CancellationToken ct = default);

    /// <summary>
    /// 分页查询未删除的文件记录。
    /// </summary>
    /// <param name="query">分页与筛选条件。</param>
    /// <param name="ct">取消令牌。</param>
    Task<SqlSugarPagedList<FileDto>> PageAsync(FilePageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 列出各业务模块的文件数量（含全部合计由调用方自行汇总）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task<List<FileModuleStatDto>> ListModulesAsync(CancellationToken ct = default);

    /// <summary>
    /// 按 FileId 打开下载流。
    /// </summary>
    /// <param name="fileId">文件业务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>下载载荷；记录或物理文件不存在时抛业务异常。</returns>
    Task<FileDownloadDto> OpenDownloadAsync(string fileId, CancellationToken ct = default);

    /// <summary>
    /// 删除物理文件并标记数据库记录删除。
    /// </summary>
    /// <param name="fileId">文件业务标识。</param>
    /// <param name="ct">取消令牌。</param>
    Task DeleteAsync(string fileId, CancellationToken ct = default);
}
