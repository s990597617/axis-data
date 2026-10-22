using Axis.Infrastructure.Components.Database.UnitOfWork;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 事务后置处理器。
/// <list type="bullet">
///   <item><description>端点未抛异常 → <see cref="IUnitOfWork.CommitAsync"/></description></item>
///   <item><description>端点抛出异常 → <see cref="IUnitOfWork.RollbackAsync"/>，异常原样抛给异常处理器</description></item>
///   <item><description>未开启事务 → 什么都不做</description></item>
/// </list>
/// </summary>
public sealed class TransactionPostProcessor : IGlobalPostProcessor
{
    /// <inheritdoc />
    public async Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        if (!http.Items.TryGetValue(TransactionPreProcessor.TxnFlagKey, out var raw) || raw is not IUnitOfWork uow)
            return;

        // 防止重复处理
        http.Items.Remove(TransactionPreProcessor.TxnFlagKey);

        var logger = http.RequestServices.GetService<ILoggerFactory>()?
            .CreateLogger("Axis.Infrastructure.FastEndpoints.Transaction");
        var endpointName = http.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType?.Name;

        if (context.HasExceptionOccurred)
        {
            logger?.LogDebug("端点异常，回滚事务。Endpoint={Endpoint}", endpointName);
            await uow.RollbackAsync(ct);
            // 不调用 MarkExceptionAsHandled，让异常继续向外传给统一异常处理器
            return;
        }

        // 成功路径：检查 HTTP 状态码，5xx 也视为失败回滚
        if (http.Response.StatusCode >= 500)
        {
            logger?.LogDebug("HTTP {Status} 视为失败，回滚事务。Endpoint={Endpoint}",
                http.Response.StatusCode, endpointName);
            await uow.RollbackAsync(ct);
            return;
        }

        await uow.CommitAsync(ct);
    }
}
