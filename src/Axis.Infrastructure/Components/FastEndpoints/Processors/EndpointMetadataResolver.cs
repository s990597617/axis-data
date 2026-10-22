using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// Axis 处理器统一的端点元数据解析工具。
/// <para>
/// FastEndpoints 允许通过两种互补的方式给端点附加元数据：
/// </para>
/// <list type="number">
///   <item>
///     <description>
///     <b>类级特性</b>：直接把 <c>Attribute</c> 写在 Endpoint 类上，
///     如 <c>[Transactional]</c>、<c>[Permission("...")]</c>。
///     </description>
///   </item>
///   <item>
///     <description>
///     <b>运行时注入</b>：在 <c>Configure()</c> 中调用
///     <c>Options(x =&gt; x.WithMetadata(new XxxAttribute(...)))</c>，
///     将特性实例动态挂到 <see cref="Endpoint.Metadata"/> 集合。
///     </description>
///   </item>
/// </list>
/// <para>
/// 两种方式在底层存储位置不同，单独读取任一条都会遗漏另一侧的配置。
/// 本解析器先看运行时元数据集合（更高优先级，便于按路由动态覆盖），
/// 再回退到类级特性（更贴近业务声明风格），保证两种写法都能被前置处理器识别。
/// </para>
/// </summary>
internal static class EndpointMetadataResolver
{
    /// <summary>
    /// 在当前请求对应的端点上按"运行时元数据 → 类级特性"顺序解析指定特性。
    /// </summary>
    /// <typeparam name="TAttribute">待解析的特性类型。</typeparam>
    /// <param name="http">当前请求的 <see cref="HttpContext"/>。</param>
    /// <returns>命中则返回特性实例；未命中返回 <c>null</c>。</returns>
    public static TAttribute Resolve<TAttribute>(HttpContext http) where TAttribute : Attribute
    {
        var endpoint = http?.GetEndpoint();
        if (endpoint is null) return null;

        // 1) 运行时注入（Options(x => x.WithMetadata(...)))
        var fromMetadata = endpoint.Metadata.GetMetadata<TAttribute>();
        if (fromMetadata is not null) return fromMetadata;

        // 2) 类级特性（直接写在 Endpoint 类上）
        var endpointType = endpoint.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
        if (endpointType is null) return null;

        return (TAttribute)Attribute.GetCustomAttribute(endpointType, typeof(TAttribute), inherit: true);
    }

    /// <summary>
    /// 获取当前请求对应的 FastEndpoints 端点 CLR 类型，
    /// 主要用于日志打印和缓存键拼接等场景，没有匹配时返回 <c>null</c>。
    /// </summary>
    /// <param name="http">当前请求的 <see cref="HttpContext"/>。</param>
    public static Type ResolveEndpointType(HttpContext http)
        => http?.GetEndpoint()?.Metadata.GetMetadata<EndpointDefinition>()?.EndpointType;
}
