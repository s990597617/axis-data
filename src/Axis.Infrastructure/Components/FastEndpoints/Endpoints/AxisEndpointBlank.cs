using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 端点基类 —— 无请求体、无强类型响应。
/// <para>
/// 适用于心跳、触发任务、回调通知等"只返回状态码"的端点；
/// 响应状态码通过 <c>SendAsync</c> / <c>SendNoContent</c> 等方法显式指定。
/// </para>
/// </summary>
public abstract class AxisEndpointWithoutRequest : EndpointWithoutRequest
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
