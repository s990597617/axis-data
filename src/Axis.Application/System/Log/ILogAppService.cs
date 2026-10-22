using Axis.Application.System.Log.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.System.Log;

/// <summary>
/// 系统日志查询与清空服务契约。
/// </summary>
public interface ILogAppService
{
    /// <summary>
    /// 分页查询登录日志。
    /// </summary>
    Task<SqlSugarPagedList<LoginLogDto>> PageLoginAsync(LoginLogPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 清空全部登录日志。
    /// </summary>
    Task ClearLoginAsync(CancellationToken ct = default);

    /// <summary>
    /// 分页查询访问日志。
    /// </summary>
    Task<SqlSugarPagedList<VisitLogDto>> PageVisitAsync(VisitLogPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 清空全部访问日志。
    /// </summary>
    Task ClearVisitAsync(CancellationToken ct = default);

    /// <summary>
    /// 分页查询操作日志。
    /// </summary>
    Task<SqlSugarPagedList<OperateLogDto>> PageOperateAsync(OperateLogPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 清空全部操作日志。
    /// </summary>
    Task ClearOperateAsync(CancellationToken ct = default);

    /// <summary>
    /// 分页查询异常日志。
    /// </summary>
    Task<SqlSugarPagedList<ErrorLogDto>> PageErrorAsync(ErrorLogPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 清空全部异常日志。
    /// </summary>
    Task ClearErrorAsync(CancellationToken ct = default);
}
