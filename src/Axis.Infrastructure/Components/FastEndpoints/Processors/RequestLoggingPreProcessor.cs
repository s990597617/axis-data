using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 全局请求入口日志。
/// 避免高频接口日志成本。
/// </summary>
public sealed class RequestLoggingPreProcessor : IGlobalPreProcessor
{
    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        var logger = http.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Axis.Infrastructure.FastEndpoints.Request");

        if (logger is null || !logger.IsEnabled(LogLevel.Debug))
            return Task.CompletedTask;

        // 在 HttpContext.Items 上留存开始时间供 Post 使用
        http.Items["__axis_req_start"] = Stopwatch.GetTimestamp();

        var endpointName = http.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType?.Name ?? "(unknown)";

        logger.LogDebug("→ {Method} {Path} Endpoint={Endpoint} TraceId={TraceId}",
            http.Request.Method,
            http.Request.Path,
            endpointName,
            Activity.Current?.TraceId.ToString() ?? http.TraceIdentifier);

        return Task.CompletedTask;
    }
}
