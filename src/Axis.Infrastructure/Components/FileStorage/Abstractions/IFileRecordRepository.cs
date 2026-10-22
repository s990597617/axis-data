namespace Axis.Infrastructure.Components.FileStorage.Abstractions;

/// <summary>
/// 文件记录仓储抽象（Infrastructure 层定义，由 Infrastructure.Data 层实现）。
/// <para>
/// 将文件记录的持久化从文件存储服务中解耦，
/// 不同存储实现（Local / MinIO / OSS）共享同一套入库逻辑。
/// </para>
/// </summary>
public interface IFileRecordRepository
{
    /// <summary>
    /// 新增文件记录
    /// </summary>
    Task AddAsync(FileRecord record, CancellationToken ct = default);

    /// <summary>
    /// 根据文件标识查询
    /// </summary>
    Task<FileRecord> FindByFileIdAsync(string fileId, CancellationToken ct = default);

    /// <summary>
    /// 根据 MD5 查找已有记录（用于秒传判断）
    /// </summary>
    Task<FileRecord> FindByMd5Async(string md5, CancellationToken ct = default);

    /// <summary>
    /// 根据业务 ID + 业务类型查询
    /// </summary>
    Task<List<FileRecord>> FindByBizAsync(long bizId, string bizType, CancellationToken ct = default);

    /// <summary>
    /// 软删除文件记录
    /// </summary>
    Task<bool> SoftDeleteByPathAsync(string relativePath, CancellationToken ct = default);

    /// <summary>
    /// 绑定业务关联（上传后再关联到具体业务记录）
    /// </summary>
    Task BindBizAsync(string fileId, long bizId, string bizType, CancellationToken ct = default);
}
