using System.Diagnostics;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 全局响应出口日志。打印状态码与耗时（配合 <see cref="RequestLoggingPreProcessor"/> 存入的起始时间）。
/// </summary>
public sealed class ResponseLoggingPostProcessor : IGlobalPostProcessor
{
    /// <inheritdoc />
    public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        var logger = http.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Axis.Infrastructure.FastEndpoints.Response");

        if (logger is null || !logger.IsEnabled(LogLevel.Debug))
            return Task.CompletedTask;

        long? elapsedMs = null;
        if (http.Items.TryGetValue("__axis_req_start", out var startObj) && startObj is long start)
        {
            var diff = Stopwatch.GetTimestamp() - start;
            elapsedMs = diff * 1000 / Stopwatch.Frequency;
        }

        var status = http.Response.StatusCode;
        var endpointName = http.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType?.Name ?? "(unknown)";

        if (context.HasExceptionOccurred)
        {
            logger.LogDebug("← {Status} (exception) Endpoint={Endpoint} Elapsed={Elapsed}ms Ex={Ex}",
                status,
                endpointName,
                elapsedMs,
                context.ExceptionDispatchInfo?.SourceException.GetType().Name);
        }
        else
        {
            logger.LogDebug("← {Status} Endpoint={Endpoint} Elapsed={Elapsed}ms",
                status,
                endpointName,
                elapsedMs);
        }

        return Task.CompletedTask;
    }
}
