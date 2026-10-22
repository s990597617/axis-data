using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Axis.Infrastructure.Components.FastEndpoints;

/// <summary>
/// Axis 全局异常处理。
/// 把所有未捕获异常映射为统一的 <see cref="ResponseResult"/>，HTTP 状态码由业务码 <see cref="ResultCodes.ToHttpStatus(int)"/> 反推：
/// <list type="bullet">
///   <item><description><see cref="BusinessException"/> → 使用其 <see cref="BusinessException.Code"/>（默认 42200 → HTTP 400）</description></item>
///   <item><description><see cref="ValidationFailureException"/> → 40001（校验失败 → HTTP 400）</description></item>
///   <item><description><see cref="VersionExceptions"/> → 40900（乐观锁冲突 → HTTP 409）</description></item>
///   <item><description>其它异常 → 50000（服务器错误 → HTTP 500）</description></item>
/// </list>
/// </summary>
public static class ExceptionHandlerExtensions
{
    /// <summary>
    /// 注册全局异常处理程序（规范入口）。
    /// </summary>
    /// <param name="app">应用管道</param>
    /// <param name="useGenericReason">true 则对外隐藏真实异常原因（适合生产），默认 false</param>
    public static IApplicationBuilder UseAxisExceptionHandler(this IApplicationBuilder app,
        bool useGenericReason = false)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var feature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (feature is null) return;

                var logger = ctx.RequestServices.GetService<ILoggerFactory>()
                    ?.CreateLogger("Axis.Kernel.ExceptionHandler");

                var reason = feature.Error.Message;
                var displayMessage = useGenericReason ? "An unexpected error has occurred." : reason;

                int code;
                object data = null;
                switch (feature.Error)
                {
                    case BusinessException be:
                        code = ResultCodeRegistry.Validate(be.Code, logger) ? be.Code : ResultCodes.ServerError;
                        data = be.Payload;
                        logger?.LogInformation(be, "业务异常：Code={Code} Message={Message}", be.Code, reason);
                        break;
                    case ValidationFailureException vfe:
                        code = ResultCodes.ValidationFailed;
                        logger?.LogInformation(vfe, "校验异常：{Message}", reason);
                        break;
                    case VersionExceptions ve:
                        code = ResultCodes.Conflict;
                        displayMessage = useGenericReason ? "An unexpected error has occurred." : "数据已被他人修改，请刷新后重试";
                        logger?.LogInformation(ve, "乐观锁冲突：{Path}", ctx.Request.Path);
                        break;
                    default:
                        code = ResultCodes.ServerError;
                        logger?.LogError(feature.Error, "未处理异常：{Path}", ctx.Request.Path);
                        break;
                }

                ctx.Response.StatusCode = ResultCodes.ToHttpStatus(code);
                ctx.Response.ContentType = "application/problem+json";

                var body = ResponseResult.Fail(code, displayMessage, data).Enrich(ctx);
                await ctx.Response.WriteAsJsonAsync(body);
            });
        });
        return app;
    }
}