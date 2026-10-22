using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 端点基类 —— 有请求体、有响应体。
/// <para>
/// 在 FastEndpoints 原生 <see cref="Endpoint{TRequest, TResponse}"/> 之上，
/// 按需暴露当前登录用户、工作单元等常用依赖，并提供一组不抛裸 <see cref="Exception"/> 的
/// 业务断言方法（统一收敛到 <see cref="ResponseResult"/>）。
/// </para>
/// <para>
/// 典型场景：查询 / 命令型 API，需要接收结构化 Request、返回结构化 Response，
/// 如 <c>CreateOrderEndpoint : AxisEndpoint&lt;CreateOrderReq, OrderDto&gt;</c>。
/// </para>
/// </summary>
/// <typeparam name="TRequest">请求 DTO 类型，必须可无参构造。</typeparam>
/// <typeparam name="TResponse">响应 DTO 类型。</typeparam>
public abstract class AxisEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse>
    where TRequest : notnull, new()
{
    /// <summary>
    /// 当前登录用户上下文。
    /// <para>
    /// 未登录（匿名请求）时返回 <c>null</c>，由业务方决定是否降级处理；
    /// 该服务为 Scoped，按请求解析自 <see cref="HttpContext.RequestServices"/>。
    /// </para>
    /// </summary>
    protected ICurrentAccount CurrentAccount
        => HttpContext.RequestServices.GetService<ICurrentAccount>();

    /// <summary>
    /// 当前请求的统一工作单元。
    /// <para>
    /// 与 <see cref="Attributes.TransactionalAttribute"/> 语义互补：
    /// 前者由处理器自动驱动，后者暴露给需要条件提交 / 手工控制事务边界的场景。
    /// </para>
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
