using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 事务前置处理器。
/// <para>
/// 仅对标注了 <see cref="TransactionalAttribute"/> 的端点生效，
/// 通过 <see cref="IUnitOfWork"/> 开启事务并挂入 <see cref="HttpContext.Items"/>
/// 供 <see cref="TransactionPostProcessor"/> 使用。
/// </para>
/// </summary>
public sealed class TransactionPreProcessor : IGlobalPreProcessor
{
    internal const string TxnFlagKey = "__axis_uow_txn";

    /// <inheritdoc />
    public Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;

        // 同时兼容类级 [Transactional] 与 Options(x => x.WithMetadata(...)) 两种写法
        var attr = EndpointMetadataResolver.Resolve<TransactionalAttribute>(http);
        if (attr is null) return Task.CompletedTask;

        var uow = http.RequestServices.GetService<IUnitOfWork>();
        if (uow is null)
        {
            // 若 UoW 未注册则降级为无事务，避免业务阻塞启动
            var logger = http.RequestServices.GetService<ILoggerFactory>()?
                .CreateLogger("Axis.Infrastructure.FastEndpoints.Transaction");
            logger?.LogWarning("已标记 [Transactional] 但未注册 IUnitOfWork，事务未开启。Endpoint={Endpoint}",
                EndpointMetadataResolver.ResolveEndpointType(http)?.FullName);
            return Task.CompletedTask;
        }

        uow.Begin(attr.IsolationLevel, attr.ConnectionId);
        http.Items[TxnFlagKey] = uow;
        return Task.CompletedTask;
    }
}
