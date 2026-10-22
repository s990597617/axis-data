using System.Data;

namespace Axis.Infrastructure.Components.Database.UnitOfWork;

/// <summary>
/// Axis 统一工作单元接口，提供跨仓储的事务语义。
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 当前是否已开启事务。
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// 开启事务；已开启则幂等返回，不会嵌套。
    /// </summary>
    /// <param name="isolationLevel">隔离级别，默认 <see cref="IsolationLevel.ReadCommitted"/>。</param>
    /// <param name="connectionId">连接 ConfigId，空表示使用当前默认连接。</param>
    void Begin(IsolationLevel? isolationLevel = null, string connectionId = null);

    /// <summary>
    /// 提交事务；未开启时幂等返回。
    /// </summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// 回滚事务；未开启时幂等返回。
    /// </summary>
    Task RollbackAsync(CancellationToken ct = default);
}
