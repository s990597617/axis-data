using System.Data;
using SqlSugar;

namespace Axis.Infrastructure.Components.Database.UnitOfWork;

/// <summary>
/// 基于 SqlSugar 的默认实现。
/// </summary>
internal sealed class SqlSugarUnitOfWork(ISqlSugarClient db, ILogger<SqlSugarUnitOfWork> logger) : IUnitOfWork
{
    /// <summary>
    /// SqlSugar 对象
    /// </summary>
    private readonly ITenant _tenant = db.AsTenant();
    
    /// <summary>
    /// 连接 id
    /// </summary>
    private string _boundConnectionId;

    /// <summary>
    /// 是否有活动得事务
    /// </summary>
    public bool HasActiveTransaction { get; private set; }

    /// <summary>
    /// 开启事务；已开启则幂等返回，不会嵌套。
    /// </summary>
    /// <param name="isolationLevel">隔离级别，默认 <see cref="IsolationLevel.ReadCommitted"/>。</param>
    /// <param name="connectionId">连接 ConfigId，空表示使用当前默认连接。</param>
    public void Begin(IsolationLevel? isolationLevel = null, string connectionId = null)
    {
        if (HasActiveTransaction) return;

        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            // 切换到目标连接后再开启事务，确保事务绑定到正确的数据库
            _tenant.ChangeDatabase(connectionId);
            _boundConnectionId = connectionId;
        }

        if (isolationLevel.HasValue)
            _tenant.BeginTran(isolationLevel.Value);
        else
            _tenant.BeginTran();

        HasActiveTransaction = true;
        logger.LogDebug("事务已开启（Isolation={Isolation}, Connection={Conn}）",
            isolationLevel, _boundConnectionId ?? "default");
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (!HasActiveTransaction) return;
        try
        {
            await _tenant.CommitTranAsync();
            logger.LogDebug("事务已提交。");
        }
        catch (Exception ex)
        {
            // 提交失败，尝试补偿回滚避免连接残留事务状态
            logger.LogError(ex, "事务提交失败，尝试回滚。");
            try { await _tenant.RollbackTranAsync(); } catch { /* ignore */ }
            throw;
        }
        finally
        {
            HasActiveTransaction = false;
        }
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (!HasActiveTransaction) return;
        try
        {
            await _tenant.RollbackTranAsync();
            logger.LogDebug("事务已回滚。");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "事务回滚失败。");
        }
        finally
        {
            HasActiveTransaction = false;
        }
    }
}
