using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 端点基类 —— 有请求体、无强类型响应。
/// <para>
/// 常用于"执行副作用但不返回数据"的命令型 API，如删除 / 状态变更等。
/// 响应由 FastEndpoints 的 <c>SendAsync</c> / <c>SendNoContent</c> 等方法输出，
/// 最终仍会被全局 <c>ResponseSerializer</c> 包裹成统一响应体。
/// </para>
/// </summary>
/// <typeparam name="TRequest">请求 DTO 类型，必须可无参构造。</typeparam>
public abstract class AxisEndpoint<TRequest> : Endpoint<TRequest>
    where TRequest : notnull, new()
{
    /// <summary>
    /// 当前登录用户上下文，详见 <see cref="AxisEndpoint{TRequest, TResponse}.CurrentAccount"/>。
    /// </summary>
    protected ICurrentAccount CurrentAccount
        => HttpContext.RequestServices.GetService<ICurrentAccount>();

    /// <summary>
    /// 当前请求的统一工作单元，详见 <see cref="AxisEndpoint{TRequest, TResponse}.UnitOfWork"/>。
    /// </summary>
    protected IUnitOfWork UnitOfWork
        => HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

    /// <summary>
    /// 抛出业务异常。详见 <see cref="EndpointHelpers.ThrowBusiness"/>。
    /// </summary>
    protected static void ThrowBusiness(int code, string message, object payload = null)
        => EndpointHelpers.ThrowBusiness(code, message, payload);

    /// <summary>
    /// 非空断言。详见 <see cref="EndpointHelpers.EnsureFound{T}"/>。
    /// </summary>
    protected static T EnsureFound<T>(T value, string message = "资源不存在") where T : class
        => EndpointHelpers.EnsureFound(value, message);

    /// <summary>
    /// 条件断言。详见 <see cref="EndpointHelpers.EnsureTrue"/>。
    /// </summary>
    protected static void EnsureTrue(bool condition, string message, int code = ResultCodes.BusinessError)
        => EndpointHelpers.EnsureTrue(condition, message, code);
}
