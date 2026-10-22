using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 文件记录仓储实现。
/// </summary>
public sealed class SysFileRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysFile>(db, sp), ISysFileRepository, IAutoInject
{
    public Task<SysFile?> FindByFileIdAsync(string fileId, CancellationToken ct = default)
        => AsQueryable().FirstAsync(f => f.FileId == fileId, ct)!;

    public Task<SysFile?> FindByMd5Async(string md5, CancellationToken ct = default)
        => AsQueryable().FirstAsync(f => f.Md5 == md5 && !f.Deleted, ct)!;

    public Task<List<SysFile>> FindByBizAsync(long bizId, string bizType, CancellationToken ct = default)
        => AsQueryable().Where(f => f.BizId == bizId && f.BizType == bizType && !f.Deleted).ToListAsync(ct);

    public Task<SqlSugarPagedList<SysFile>> PageAsync(
        string? keyword,
        string? module,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = AsQueryable()
            .Where(f => !f.Deleted)
            .WhereIF(!string.IsNullOrWhiteSpace(module), f => f.Module == module)
            .WhereIF(
                !string.IsNullOrWhiteSpace(keyword),
                f => f.OriginalName.Contains(keyword!)
                     || f.FileId.Contains(keyword!)
                     || f.RelativePath.Contains(keyword!))
            .OrderBy(f => f.CreatedTime, OrderByType.Desc);

        return query.ToPagedListAsync(page, pageSize);
    }

    public async Task<List<(string Module, int Count)>> ListModuleStatsAsync(CancellationToken ct = default)
    {
        var rows = await AsQueryable()
            .Where(f => !f.Deleted)
            .GroupBy(f => f.Module)
            .Select(f => new
            {
                Module = f.Module,
                Count = SqlFunc.AggregateCount(f.Id)
            })
            .ToListAsync(ct);

        return rows
            .Select(row => (Module: row.Module ?? string.Empty, row.Count))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Module, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task AddAsync(SysFile entity, CancellationToken ct = default)
        => await Context.Insertable(entity).ExecuteCommandAsync(ct);

    public async Task<bool> SoftDeleteByPathAsync(string relativePath, CancellationToken ct = default)
        => await Context.Updateable<SysFile>()
            .SetColumns(f => new SysFile { Deleted = true })
            .Where(f => f.RelativePath == relativePath)
            .ExecuteCommandAsync(ct) > 0;
}
