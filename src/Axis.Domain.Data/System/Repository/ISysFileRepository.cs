using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 文件记录仓储：按 FileId / MD5 / 业务关联查询，以及管理端分页。
/// </summary>
public interface ISysFileRepository : IRepository<SysFile>
{
    /// <summary>
    /// 根据文件标识查询记录（含已删除）。
    /// </summary>
    /// <param name="fileId">文件业务标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>匹配记录；不存在时返回 <c>null</c>。</returns>
    Task<SysFile?> FindByFileIdAsync(string fileId, CancellationToken ct = default);

    /// <summary>
    /// 根据 MD5 查找未删除的文件记录（用于秒传）。
    /// </summary>
    /// <param name="md5">文件内容 MD5。</param>
    /// <param name="ct">取消令牌。</param>
    Task<SysFile?> FindByMd5Async(string md5, CancellationToken ct = default);

    /// <summary>
    /// 根据业务 ID 和业务类型查询未删除的关联文件列表。
    /// </summary>
    /// <param name="bizId">业务主键。</param>
    /// <param name="bizType">业务类型编码。</param>
    /// <param name="ct">取消令牌。</param>
    Task<List<SysFile>> FindByBizAsync(long bizId, string bizType, CancellationToken ct = default);

    /// <summary>
    /// 分页查询未删除的文件记录。
    /// </summary>
    /// <param name="keyword">关键字；匹配原始名 / FileId / 相对路径；可为空。</param>
    /// <param name="module">业务模块过滤；可为空。</param>
    /// <param name="page">页码，从 1 开始。</param>
    /// <param name="pageSize">每页条数。</param>
    /// <param name="ct">取消令牌。</param>
    Task<SqlSugarPagedList<SysFile>> PageAsync(
        string? keyword,
        string? module,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// 按业务模块统计未删除文件数量。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>模块与数量列表；Module 为空表示未分类。</returns>
    Task<List<(string Module, int Count)>> ListModuleStatsAsync(CancellationToken ct = default);

    /// <summary>
    /// 新增文件记录。
    /// </summary>
    /// <param name="entity">文件实体。</param>
    /// <param name="ct">取消令牌。</param>
    Task AddAsync(SysFile entity, CancellationToken ct = default);

    /// <summary>
    /// 根据相对路径将记录标记为已删除。
    /// </summary>
    /// <param name="relativePath">相对存储路径。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>是否实际更新到记录。</returns>
    Task<bool> SoftDeleteByPathAsync(string relativePath, CancellationToken ct = default);
}
