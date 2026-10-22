using Axis.Infrastructure.Components.FastEndpoints.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Processors;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints;

/// <summary>
/// FastEndpoints 运行程序拓展
/// </summary>
public static class FastEndpointsApplicationBuilderExtensions
{
    /// <summary>
    /// 启用 FastEndpoints 管道。
    /// 注意：不在此处挂异常处理器，异常由宿主层的 <c>UseAxisExceptionHandler</c> 统一处理，
    /// 避免与 FastEndpoints 自带的 <c>UseDefaultExceptionHandler</c> / 其它 <c>UseExceptionHandler</c>
    /// 产生双重拦截导致 <see cref="Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature"/> 丢失。
    /// </summary>
    public static WebApplication UseFastEndPointsApp(this WebApplication app, Action<EndpointDefinition> ep = null)
    {
        app.UseFastEndpoints(c =>
        {
            c.Endpoints.ShortNames = true;
            c.Endpoints.RoutePrefix = "api";

            // 全局前置 / 后置处理器统一挂载。执行顺序（从上到下）：
            //   Pre.Before   : 请求日志 → 防重放 → 权限校验 → 幂等判重 （进入业务前的横切关注点）
            //   用户 Pre                                             （业务声明的 PreProcessor）
            //   Pre.After    : 事务开启                              （贴近 Handler，避免空转事务）
            //   Handler                                              （端点本体）
            //   Post.Before  : 事务提交 / 回滚                       （贴近 Handler，尽早释放连接）
            //   用户 Post                                            （业务声明的 PostProcessor）
            //   Post.After   : 响应日志                              （最末端，可观察到最终状态码）
            // 事务边界仅覆盖 Handler 本体，不卷入日志 / 审计 / 异步通知。
            c.Endpoints.Configurator = def =>
            {
                def.PreProcessor<RequestLoggingPreProcessor>(Order.Before);
                def.PreProcessor<AntiReplayPreProcessor>(Order.Before);
                def.PreProcessor<PermissionPreProcessor>(Order.Before);
                def.PreProcessor<IdempotencyPreProcessor>(Order.Before);
                def.PreProcessor<TransactionPreProcessor>(Order.After);

                def.PostProcessor<TransactionPostProcessor>(Order.Before);
                def.PostProcessor<ResponseLoggingPostProcessor>(Order.After);

                // 用户自定义扩展点最后执行，可覆盖 / 追加
                ep?.Invoke(def);
            };

            // 校验失败响应（HTTP 400）：走统一响应体 + 业务码 ValidationFailed
            c.Errors.ResponseBuilder = (failures, ctx, _) =>
            {
                var firstError = failures.FirstOrDefault();
                var body = ResponseResult.Fail(
                    ResultCodes.ValidationFailed,
                    firstError?.ErrorMessage ?? "One or more validation errors occurred.",
                    new ValidationErrorDetails
                    {
                        Status = ResultCodes.ToHttpStatus(ResultCodes.ValidationFailed),
                        Instance = ctx.Request.Path,
                        ErrorMessage = firstError?.ErrorMessage
                    });
                return body.Enrich(ctx);
            };
            c.Errors.ContentType = "application/problem+json";

            // 仅包装成功响应：不覆盖业务 Endpoint 设定的状态码 / ContentType，
            // 这样 SendNoContent / SendCreatedAt 等 204/201 响应仍能保留原语义。
            c.Serializer.ResponseSerializer = (rsp, dto, cType, _, ct) =>
            {
                if (rsp.StatusCode is >= 200 and < 300)
                {
                    rsp.ContentType = string.IsNullOrWhiteSpace(cType) ? "application/json" : cType;
                    var envelope = ResponseResult.Ok<object>(dto).Enrich(rsp.HttpContext);
                    return rsp.WriteAsJsonAsync(envelope, cancellationToken: ct);
                }

                // 非成功状态码保持原样（由 Errors.ResponseBuilder / 异常处理器输出）
                rsp.ContentType = string.IsNullOrWhiteSpace(cType) ? "application/problem+json" : cType;
                return rsp.WriteAsJsonAsync(dto, cancellationToken: ct);
            };
        });
        return app;
    }
}