using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 端点基类共享的便捷方法集合。
/// <para>
/// 抽出为独立静态类是为了让四个不同形参的 <c>AxisEndpoint</c>
/// 基类（有无请求 / 有无响应）复用同一套语义，而不必继承同一个父类，
/// 也避免 C# 单继承限制导致基类无法同时继承 FastEndpoints 的四种原生基类。
/// </para>
/// <para>
/// 所有方法均以抛出 <see cref="BusinessException"/> 为终止手段，
/// 由宿主层的统一异常处理器映射为 <see cref="ResponseResult"/> 响应体。
/// </para>
/// </summary>
internal static class EndpointHelpers
{
    /// <summary>
    /// 直接抛出业务异常。
    /// <para>
    /// 等价于 <c>throw new BusinessException(code, message, payload)</c>，
    /// 仅作为语义清晰的语法糖存在；业务码请使用 <see cref="ResultCodes"/> 中的常量。
    /// </para>
    /// </summary>
    /// <param name="code">业务码，参见 <see cref="ResultCodes"/>。</param>
    /// <param name="message">面向用户或调用方的可读消息。</param>
    /// <param name="payload">可选附加数据，会随错误响应的 <c>data</c> 字段返回。</param>
    internal static void ThrowBusiness(int code, string message, object payload = null)
        => throw new BusinessException(code, message, payload);

    /// <summary>
    /// 对可能为空的引用类型做非空断言，常用于把 <c>GetById</c> 等查询的 null 结果
    /// 直接转换为 <see cref="ResultCodes.NotFound"/> 业务码。
    /// </summary>
    /// <typeparam name="T">被断言的引用类型。</typeparam>
    /// <param name="value">待断言的值。</param>
    /// <param name="message">资源缺失时返回给调用方的消息。</param>
    /// <returns><paramref name="value"/> 自身，便于链式写法。</returns>
    internal static T EnsureFound<T>(T value, string message = "资源不存在") where T : class
    {
        if (value is null) throw new BusinessException(ResultCodes.NotFound, message);
        return value;
    }

    /// <summary>
    /// 条件断言：当 <paramref name="condition"/> 为 <c>false</c> 时抛出业务异常。
    /// <para>
    /// 用于替代大段 <c>if (!x) throw ...</c> 的样板代码，让业务方法聚焦主流程。
    /// </para>
    /// </summary>
    /// <param name="condition">需要满足的条件。</param>
    /// <param name="message">条件不满足时返回给调用方的消息。</param>
    /// <param name="code">业务码，默认 <see cref="ResultCodes.BusinessError"/>。</param>
    internal static void EnsureTrue(bool condition, string message, int code = ResultCodes.BusinessError)
    {
        if (!condition) throw new BusinessException(code, message);
    }
}
