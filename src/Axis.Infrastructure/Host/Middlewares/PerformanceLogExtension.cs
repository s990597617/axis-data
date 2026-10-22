using System.Diagnostics;
using System.Text;
using Axis.Infrastructure.Attributes;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Components.EventBus.Events;
using Axis.Infrastructure.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Host.Middlewares;

/// <summary>
/// 全局访问 / 操作 / 异常日志记录中间件。
/// </summary>
public static class PerformanceLogExtension
{
    /// <summary>
    /// 单次读取请求体的最大字节数，防止上传文件被整体吞入日志。
    /// </summary>
    private const int MaxLoggedBodyBytes = 32 * 1024;

    /// <summary>
    /// 开启访问 / 操作 / 异常日志中间件。
    /// 三类日志通过 FastEndpoints Command 派发，若业务未提供 Handler，将自动降级写入 ILogger。
    /// </summary>
    public static IApplicationBuilder UsePerformanceLog(this IApplicationBuilder applicationBuilder)
    {
        var loggerFactory = applicationBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Axis.Kernel.AuditLog");

        applicationBuilder.Use(async (context, next) =>
        {
            var stopWatch = Stopwatch.StartNew();
            Exception capturedException = null;
            try
            {
                // next() 抛异常时由后续异常处理器处理，但必须保证审计日志仍能写
                await next();
            }
            catch (Exception ex)
            {
                capturedException = ex;
                throw;
            }
            finally
            {
                stopWatch.Stop();
                // 用本地函数以 await 安全；finally 内不能再 await 外层 next
                _ = WriteAuditAsync(context, logger, stopWatch.ElapsedMilliseconds, capturedException);
            }
        });
        return applicationBuilder;
    }

    /// <summary>
    /// 在请求结束后按 Endpoint 元数据写审计日志。
    /// 无论请求成功 / 失败都会调用；失败时 <paramref name="capturedException"/> 不为空。
    /// </summary>
    private static async Task WriteAuditAsync(HttpContext context, ILogger logger, long elapsedMs, Exception capturedException)
    {
        try
        {
            var endpoint = context.GetEndpoint();
            var hasOperate = endpoint?.Metadata.GetMetadata<OperateLogAttribute>() != null;
            var hasVisit = endpoint?.Metadata.GetMetadata<VisitLogAttribute>() != null;
            // finally 早于外层 UseExceptionHandler 执行，此时 IExceptionHandlerFeature 往往尚未写入，
            // 必须以本中间件捕获的 capturedException 为准。
            var error = capturedException
                        ?? context.Features.Get<IExceptionHandlerFeature>()?.Error;
            var shouldPersistError = ShouldPersistErrorLog(error, context.Response.StatusCode);
            if (!hasOperate && !hasVisit && !shouldPersistError) return;

            var requestBody = await ReadRequestBodySafelyAsync(context);
            var endPointMetadata = endpoint?.Metadata.GetMetadata<EndpointDefinition>()
                                   ?? context.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata.GetMetadata<EndpointDefinition>();
            var summary = endPointMetadata?.EndpointSummary?.Summary;
            var name = endPointMetadata?.EndpointType.Name;
            var fullName = endPointMetadata?.EndpointType.FullName;
            var url = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";
            var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            var requestParam = context.Request.Method == "GET"
                ? (context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty)
                : requestBody;

            // 解析当前登录用户（可能为空）
            var currentAccount = context.RequestServices.GetService<ICurrentAccount>();

            if (shouldPersistError)
            {
                await PublishAuditEventAsync(logger, new AuditLogEvent
                {
                    LogType = AuditLogType.Error,
                    UserId = currentAccount?.Id != 0 ? currentAccount?.Id : null,
                    Account = currentAccount?.AccountName,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    DisplayTitle = summary,
                    FullName = fullName,
                    ActionName = name,
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    RemotePort = context.Connection.RemotePort,
                    HttpMethod = context.Request.Method,
                    RequestUrl = url,
                    RequestParam = requestParam,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    TraceId = traceId,
                    Exception = error?.ToString() ?? $"HTTP {context.Response.StatusCode}",
                    Elapsed = elapsedMs
                });
            }

            if (hasOperate)
            {
                await PublishAuditEventAsync(logger, new AuditLogEvent
                {
                    LogType = AuditLogType.Operate,
                    UserId = currentAccount?.Id != 0 ? currentAccount?.Id : null,
                    Account = currentAccount?.AccountName,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    DisplayTitle = summary,
                    FullName = fullName,
                    ActionName = name,
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    RemotePort = context.Connection.RemotePort,
                    HttpMethod = context.Request.Method,
                    RequestUrl = url,
                    RequestParam = requestParam,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    TraceId = traceId,
                    Elapsed = elapsedMs,
                    State = context.Response.StatusCode.ToString()
                });
            }

            if (hasVisit)
            {
                await PublishAuditEventAsync(logger, new AuditLogEvent
                {
                    LogType = AuditLogType.Visit,
                    UserId = currentAccount?.Id != 0 ? currentAccount?.Id : null,
                    Account = currentAccount?.AccountName,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    DisplayTitle = summary,
                    FullName = fullName,
                    ActionName = name,
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    RemotePort = context.Connection.RemotePort,
                    HttpMethod = context.Request.Method,
                    RequestUrl = url,
                    RequestParam = requestParam,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    TraceId = traceId,
                    Elapsed = elapsedMs,
                    State = context.Response.StatusCode.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "审计日志写入失败");
        }
    }

    /// <summary>
    /// 是否写入异常日志表。
    /// 业务异常 / 校验异常仅返回统一 Result，不进 sys_log_error；未处理异常或 HTTP ≥500 才入库。
    /// </summary>
    private static bool ShouldPersistErrorLog(Exception? error, int statusCode)
    {
        if (error is BusinessException or ValidationFailureException)
            return false;
        return error is not null || statusCode >= 500;
    }

    /// <summary>
    /// 安全读取请求体：跳过 multipart（上传场景）、限制最大读取字节、读完还原位置。
    /// </summary>
    private static async Task<string> ReadRequestBodySafelyAsync(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method)
            && !HttpMethods.IsPut(context.Request.Method)
            && !HttpMethods.IsPatch(context.Request.Method)
            && !HttpMethods.IsDelete(context.Request.Method))
        {
            return string.Empty;
        }

        var contentType = context.Request.ContentType;
        if (!string.IsNullOrEmpty(contentType) && contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
        {
            return "[multipart/form-data skipped]";
        }

        if (!context.Request.Body.CanSeek) return string.Empty;
        if (context.Request.ContentLength is > MaxLoggedBodyBytes)
        {
            return $"[body too large: {context.Request.ContentLength} bytes]";
        }

        try
        {
            context.Request.Body.Position = 0;
            var buffer = new byte[MaxLoggedBodyBytes];
            var read = await context.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));
            context.Request.Body.Position = 0;
            return Encoding.UTF8.GetString(buffer, 0, read);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 通过 Event Bus 发布审计日志事件（Fire-and-Forget，不阻塞请求）。
    /// 若事件发布失败（如未注册 Handler），降级写 <see cref="ILogger"/>。
    /// </summary>
    private static async Task PublishAuditEventAsync(ILogger logger, AuditLogEvent auditEvent)
    {
        try
        {
            await EventPublisher.PublishFireAndForgetAsync(auditEvent);
        }
        catch (Exception ex)
        {
            // 降级：Event Handler 未注册或发布异常，仅记录到结构化日志
            logger.LogWarning(ex, "审计事件发布失败，降级写日志：{LogType} {Url}",
                auditEvent.LogType, auditEvent.RequestUrl);
            logger.LogInformation("AuditLog({LogType}): User={Account} Url={Url} Elapsed={Elapsed}ms",
                auditEvent.LogType, auditEvent.Account, auditEvent.RequestUrl, auditEvent.Elapsed);
        }
    }
}