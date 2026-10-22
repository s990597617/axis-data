using Axis.Application.System.Log.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using SqlSugar;

namespace Axis.Application.System.Log;

/// <summary>
/// 系统日志查询与清空服务。
/// </summary>
/// <remarks>
/// 写入由登录 / 审计事件处理器完成；本服务仅负责管理端查询与清空。
/// </remarks>
public sealed class LogAppService(
    IRepository<SysLogLogin> loginRepo,
    IRepository<SysLogVis> visitRepo,
    IRepository<SysLogOp> operateRepo,
    IRepository<SysLogError> errorRepo)
    : ILogAppService, IAutoInject
{
    /// <inheritdoc />
    public async Task<SqlSugarPagedList<LoginLogDto>> PageLoginAsync(
        LoginLogPageQuery query,
        CancellationToken ct = default)
    {
        var keyword = query.Keyword?.Trim();
        var endTime = NormalizeEndTime(query.EndTime);

        return await loginRepo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                item.Account.Contains(keyword!) ||
                (item.RemoteIp != null && item.RemoteIp.Contains(keyword!)))
            .WhereIF(query.Success.HasValue, item => item.Success == query.Success!.Value)
            .WhereIF(query.StartTime.HasValue, item => item.CreatedTime >= query.StartTime!.Value)
            .WhereIF(endTime.HasValue, item => item.CreatedTime <= endTime!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new LoginLogDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Account = item.Account,
                RemoteIp = item.RemoteIp,
                UserAgent = item.UserAgent,
                Success = item.Success,
                FailReason = item.FailReason,
                CreatedTime = item.CreatedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <inheritdoc />
    public Task ClearLoginAsync(CancellationToken ct = default)
        => loginRepo.AsDeleteable().ExecuteCommandAsync(ct);

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<VisitLogDto>> PageVisitAsync(
        VisitLogPageQuery query,
        CancellationToken ct = default)
    {
        var keyword = query.Keyword?.Trim();
        var httpMethod = query.HttpMethod?.Trim();
        var endTime = NormalizeEndTime(query.EndTime);

        return await visitRepo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                (item.Account != null && item.Account.Contains(keyword!)) ||
                (item.DisplayTitle != null && item.DisplayTitle.Contains(keyword!)) ||
                (item.RequestUrl != null && item.RequestUrl.Contains(keyword!)) ||
                (item.RemoteIp != null && item.RemoteIp.Contains(keyword!)))
            .WhereIF(!string.IsNullOrWhiteSpace(httpMethod), item => item.HttpMethod == httpMethod)
            .WhereIF(query.StartTime.HasValue, item => item.CreatedTime >= query.StartTime!.Value)
            .WhereIF(endTime.HasValue, item => item.CreatedTime <= endTime!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new VisitLogDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Account = item.Account,
                DisplayTitle = item.DisplayTitle,
                FullName = item.FullName,
                ActionName = item.ActionName,
                HttpMethod = item.HttpMethod,
                RequestUrl = item.RequestUrl,
                RequestParam = item.RequestParam,
                RemoteIp = item.RemoteIp,
                UserAgent = item.UserAgent,
                Elapsed = item.Elapsed,
                State = item.State,
                TraceId = item.TraceId,
                CreatedTime = item.CreatedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <inheritdoc />
    public Task ClearVisitAsync(CancellationToken ct = default)
        => visitRepo.AsDeleteable().ExecuteCommandAsync(ct);

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<OperateLogDto>> PageOperateAsync(
        OperateLogPageQuery query,
        CancellationToken ct = default)
    {
        var keyword = query.Keyword?.Trim();
        var httpMethod = query.HttpMethod?.Trim();
        var endTime = NormalizeEndTime(query.EndTime);

        return await operateRepo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                (item.Account != null && item.Account.Contains(keyword!)) ||
                (item.DisplayTitle != null && item.DisplayTitle.Contains(keyword!)) ||
                (item.RequestUrl != null && item.RequestUrl.Contains(keyword!)) ||
                (item.RemoteIp != null && item.RemoteIp.Contains(keyword!)))
            .WhereIF(!string.IsNullOrWhiteSpace(httpMethod), item => item.HttpMethod == httpMethod)
            .WhereIF(query.StartTime.HasValue, item => item.CreatedTime >= query.StartTime!.Value)
            .WhereIF(endTime.HasValue, item => item.CreatedTime <= endTime!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new OperateLogDto
            {
                Id = item.Id,
                UserId = item.UserId,
                Account = item.Account,
                DisplayTitle = item.DisplayTitle,
                FullName = item.FullName,
                ActionName = item.ActionName,
                HttpMethod = item.HttpMethod,
                RequestUrl = item.RequestUrl,
                RequestParam = item.RequestParam,
                RemoteIp = item.RemoteIp,
                UserAgent = item.UserAgent,
                Elapsed = item.Elapsed,
                State = item.State,
                TraceId = item.TraceId,
                CreatedTime = item.CreatedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <inheritdoc />
    public Task ClearOperateAsync(CancellationToken ct = default)
        => operateRepo.AsDeleteable().ExecuteCommandAsync(ct);

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<ErrorLogDto>> PageErrorAsync(
        ErrorLogPageQuery query,
        CancellationToken ct = default)
    {
        var keyword = query.Keyword?.Trim();
        var httpMethod = query.HttpMethod?.Trim();
        var endTime = NormalizeEndTime(query.EndTime);

        return await errorRepo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                (item.DisplayTitle != null && item.DisplayTitle.Contains(keyword!)) ||
                (item.RequestUrl != null && item.RequestUrl.Contains(keyword!)) ||
                (item.Exception != null && item.Exception.Contains(keyword!)) ||
                (item.RemoteIp != null && item.RemoteIp.Contains(keyword!)))
            .WhereIF(!string.IsNullOrWhiteSpace(httpMethod), item => item.HttpMethod == httpMethod)
            .WhereIF(query.StartTime.HasValue, item => item.CreatedTime >= query.StartTime!.Value)
            .WhereIF(endTime.HasValue, item => item.CreatedTime <= endTime!.Value)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new ErrorLogDto
            {
                Id = item.Id,
                DisplayTitle = item.DisplayTitle,
                FullName = item.FullName,
                ActionName = item.ActionName,
                HttpMethod = item.HttpMethod,
                RequestUrl = item.RequestUrl,
                RequestParam = item.RequestParam,
                RemoteIp = item.RemoteIp,
                UserAgent = item.UserAgent,
                Elapsed = item.Elapsed,
                Exception = item.Exception,
                TraceId = item.TraceId,
                ThreadId = item.ThreadId,
                CreatedTime = item.CreatedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);
    }

    /// <inheritdoc />
    public Task ClearErrorAsync(CancellationToken ct = default)
        => errorRepo.AsDeleteable().ExecuteCommandAsync(ct);

    /// <summary>
    /// 结束日若为 00:00:00，扩展到当天 23:59:59.999，便于前端按日期筛选。
    /// </summary>
    private static DateTime? NormalizeEndTime(DateTime? endTime)
    {
        if (!endTime.HasValue) return null;
        var value = endTime.Value;
        return value.TimeOfDay == TimeSpan.Zero
            ? value.Date.AddDays(1).AddTicks(-1)
            : value;
    }
}
