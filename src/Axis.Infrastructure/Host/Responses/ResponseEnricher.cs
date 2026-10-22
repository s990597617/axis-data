using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Host.Responses;

/// <summary>
/// 为 <see cref="ResponseResult{T}"/> 补齐与 <see cref="HttpContext"/> 相关的字段（TraceId / Path）。
/// 统一到本工具，避免异常处理器、FastEndpoints serializer、错误 builder 三处各自写一遍。
/// </summary>
public static class ResponseEnricher
{
    /// <summary>
    /// 写入 TraceId（优先 W3C <see cref="Activity.Current"/>.TraceId，回退到 <see cref="HttpContext.TraceIdentifier"/>）
    /// 与 Path；不覆盖调用方已经显式赋值的字段。
    /// </summary>
    public static ResponseResult<T> Enrich<T>(this ResponseResult<T> response, HttpContext ctx)
    {
        if (response is null || ctx is null) return response;

        response.TraceId ??= Activity.Current?.TraceId.ToString() ?? ctx.TraceIdentifier;
        response.Path ??= ctx.Request?.Path.Value;
        return response;
    }
}
