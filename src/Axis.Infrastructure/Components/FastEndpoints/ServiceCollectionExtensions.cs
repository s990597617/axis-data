using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Components.FastEndpoints.Processors;
using Axis.Infrastructure.Security;
using Axis.Infrastructure.Security.OpenApi;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Infrastructure.Components.FastEndpoints;

/// <summary>
/// FastEndPoints 服务拓展
/// </summary>
public static class FastEndpointsServiceCollectionExtensions
{
    /// <summary>
    /// 增加 FastEndPoints 服务：JWT 认证 + 授权 + Endpoint 扫描 + 统一工作单元 + 全局处理器。
    /// Endpoint 程序集通过 <c>BootstrapAssembly.FastEndPoints</c> 声明，
    /// 统一走 <see cref="GlobalAssemblyLoader"/> 容错加载。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddFastEndPointsSetup(this IServiceCollection services)
    {
        // 全局前置 / 后置处理器注册为单例：在请求内部通过 HttpContext.RequestServices 解析 scoped 依赖
        services.AddSingleton<RequestLoggingPreProcessor>();
        services.AddSingleton<AntiReplayPreProcessor>();
        services.AddSingleton<PermissionPreProcessor>();
        services.AddSingleton<IdempotencyPreProcessor>();
        services.AddSingleton<TransactionPreProcessor>();
        services.AddSingleton<TransactionPostProcessor>();
        services.AddSingleton<ResponseLoggingPostProcessor>();

        services.AddAuthenticationJwtBearer(s =>
            {
                s.SigningKey = GlobalContext.BootstrapJwt.Token;
            }, b =>
            {
                b.TokenValidationParameters.ValidIssuer = GlobalContext.BootstrapJwt.Issuer;
                b.TokenValidationParameters.ValidAudience = GlobalContext.BootstrapJwt.Audience;
                b.Events ??= new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents();
                var previous = b.Events.OnTokenValidated;
                b.Events.OnTokenValidated = async context =>
                {
                    if (previous is not null)
                        await previous(context);

                    var jti = context.Principal?.FindFirst(ClaimConst.JwtId)?.Value
                              ?? context.Principal?.FindFirst("jti")?.Value;
                    if (string.IsNullOrWhiteSpace(jti)) return;

                    var deny = context.HttpContext.RequestServices.GetService<IAccessTokenDenyService>();
                    if (deny is null) return;

                    if (await deny.IsDeniedAsync(jti, context.HttpContext.RequestAborted))
                        context.Fail("令牌已失效，请重新登录");
                };
            });

        services.AddAuthentication()
            .AddScheme<SignatureAuthenticationOptions, SignatureAuthenticationHandler>(
                SignatureAuthenticationDefaults.AuthenticationScheme,
                _ => { });

        services.AddAuthorization()
            .AddFastEndpoints(x =>
            {
                var assemblies = GlobalAssemblyLoader.Load(GlobalContext.BootstrapAssembly?.FastEndPoints);
                if (assemblies.Length > 0)
                {
                    x.Assemblies = assemblies;
                    x.DisableAutoDiscovery = true;
                }
            });
        return services;
    }
}
