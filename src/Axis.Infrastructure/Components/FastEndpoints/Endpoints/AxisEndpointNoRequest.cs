using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 端点基类 —— 无请求体、有响应体。
/// <para>
/// 常用于"只读的获取类"或参数全走 QueryString / Route 的 GET API。
/// 若确有零散查询参数，推荐改用 <see cref="AxisEndpoint{TRequest, TResponse}"/>
/// 并定义 Request DTO，以获得统一的校验 + OpenAPI 文档体验。
/// </para>
/// </summary>
/// <typeparam name="TResponse">响应 DTO 类型。</typeparam>
public abstract class AxisEndpointWithoutRequest<TResponse> : EndpointWithoutRequest<TResponse>
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
