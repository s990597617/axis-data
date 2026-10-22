using Axis.Infrastructure.Components.FileStorage.Abstractions;
using Axis.Domain.System;
using Mapster;
using Yitter.IdGenerator;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 文件记录仓储实现（基于 SysFile 实体）
/// </summary>
public sealed class SysFileRecordRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysFile>(db, sp), IFileRecordRepository, IAutoInject
{
    public async Task AddAsync(FileRecord record, CancellationToken ct = default)
    {
        var entity = new SysFile
        {
            Id = YitIdHelper.NextId(),
            FileId = record.FileId,
            OriginalName = record.OriginalName,
            StoredName = record.StoredName,
            RelativePath = record.RelativePath,
            Url = record.Url,
            Size = record.Size,
            ContentType = record.ContentType,
            Extension = record.Extension,
            Md5 = record.Md5,
            Module = record.Module,
            BizId = record.BizId,
            BizType = record.BizType,
            Provider = record.Provider,
            CreatedTime = DateTime.Now
        };

        await Context.Insertable(entity).ExecuteCommandAsync(ct);
    }

    public async Task<FileRecord> FindByFileIdAsync(string fileId, CancellationToken ct = default)
    {
        var entity = await AsQueryable().FirstAsync(f => f.FileId == fileId && !f.Deleted, ct);
        return entity?.Adapt<FileRecord>();
    }

    public async Task<FileRecord> FindByMd5Async(string md5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(md5)) return null;
        var entity = await AsQueryable().FirstAsync(f => f.Md5 == md5 && !f.Deleted, ct);
        return entity?.Adapt<FileRecord>();
    }

    public async Task<List<FileRecord>> FindByBizAsync(long bizId, string bizType, CancellationToken ct = default)
    {
        var list = await AsQueryable()
            .Where(f => f.BizId == bizId && f.BizType == bizType && !f.Deleted)
            .ToListAsync(ct);
        return list.Select(MapToRecord).ToList();
    }

    public async Task<bool> SoftDeleteByPathAsync(string relativePath, CancellationToken ct = default)
        => await Context.Updateable<SysFile>()
            .SetColumns(f => new SysFile { Deleted = true })
            .Where(f => f.RelativePath == relativePath)
            .ExecuteCommandAsync(ct) > 0;

    public async Task BindBizAsync(string fileId, long bizId, string bizType, CancellationToken ct = default)
        => await Context.Updateable<SysFile>()
            .SetColumns(f => new SysFile { BizId = bizId, BizType = bizType })
            .Where(f => f.FileId == fileId)
            .ExecuteCommandAsync(ct);

    private static FileRecord MapToRecord(SysFile entity) => new()
    {
        FileId = entity.FileId,
        OriginalName = entity.OriginalName,
        StoredName = entity.StoredName,
        RelativePath = entity.RelativePath,
        Url = entity.Url,
        Size = entity.Size,
        ContentType = entity.ContentType,
        Extension = entity.Extension,
        Md5 = entity.Md5,
        Module = entity.Module,
        BizId = entity.BizId,
        BizType = entity.BizType,
        Provider = entity.Provider
    };
}