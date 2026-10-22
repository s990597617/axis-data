using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Permission;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 权限前置处理器。
/// <para>
/// 仅对标注了 <see cref="PermissionAttribute"/> 的端点生效，
/// 根据 <see cref="PermissionPolicy"/> 调用 <see cref="IPermissionService"/> 进行判定。
/// 业务侧实现会从用户权限码缓存读取（登录预热 / 未命中回源），避免每次鉴权查库。
/// </para>
/// <para>
/// 行为矩阵：
/// </para>
/// <list type="bullet">
///   <item><description>匿名请求但端点需要权限 → 抛 <see cref="ResultCodes.Unauthorized"/>（由异常处理器映射为 401）。</description></item>
///   <item><description>登录但权限不足 → 抛 <see cref="ResultCodes.Forbidden"/>（403）。</description></item>
///   <item><description><see cref="ICurrentAccount.IsSuperAdmin"/> 为真 → 直接放行。</description></item>
///   <item><description><see cref="IPermissionService"/> 未注册 → 抛 <see cref="ResultCodes.Forbidden"/>（失败关闭，禁止裸放行）。</description></item>
/// </list>
/// </summary>
public sealed class PermissionPreProcessor : IGlobalPreProcessor
{
    /// <summary>
    /// 处理器进入点：仅在端点显式声明 <see cref="PermissionAttribute"/> 时才执行校验，
    /// 其它情况保持零开销直接返回。
    /// </summary>
    /// <param name="context">FastEndpoints 前置处理器上下文。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;

        // 同时兼容类级 [Permission(...)] 与 Options(x => x.WithMetadata(new PermissionAttribute(...))) 两种写法
        var attr = EndpointMetadataResolver.Resolve<PermissionAttribute>(http);
        if (attr is null || attr.Codes.Length == 0) return;

        var account = http.RequestServices.GetService<ICurrentAccount>();
        if (account is null || account.Id == 0)
            throw new BusinessException(ResultCodes.Unauthorized, "未登录或登录已失效");

        if (account.IsSuperAdmin) return;

        var service = http.RequestServices.GetService<IPermissionService>();
        if (service is null)
        {
            var logger = http.RequestServices.GetService<ILoggerFactory>()
                ?.CreateLogger("Axis.Kernel.FastEndpoints.Permission");

            logger?.LogError("端点 {Endpoint} 声明了 [Permission] 但未注册 IPermissionService，已拒绝。",
                EndpointMetadataResolver.ResolveEndpointType(http)?.FullName);

            // 失败关闭：有权限声明却无判定实现时不得放行。
            throw new BusinessException(ResultCodes.Forbidden, "权限服务未就绪");
        }

        var granted = attr.Policy == PermissionPolicy.AllOf
            ? await service.HasAllAsync(attr.Codes, ct)
            : await service.HasAnyAsync(attr.Codes, ct);

        if (!granted)
        {
            throw new BusinessException(ResultCodes.Forbidden, "权限不足",
                new { required = attr.Codes, policy = attr.Policy.ToString() });
        }
    }
}