using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api;

/// <summary>
/// Ping 响应体
/// </summary>
public sealed record PingResponse(string Service, string Version, DateTimeOffset ServerTime);

/// <summary>
/// 健康检查 / 端到端管道验证端点
/// </summary>
public sealed class PingEndpoint : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/ping");
        AllowAnonymous();
        Options(x => x.WithTags("PingEndpoint").WithGroupName(nameof(ApiGroupEnum.AdminBaseModule)));
        Description(b => b.WithName("心跳检测").Produces<PingResponse>(200));
        Summary(s =>
        {
            s.Summary = "探活";
            s.Description = "返回服务名、版本、服务器时间，用于验证 Axis 框架管道是否正常。";
        });
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var response = new PingResponse(
            Service: "Axis.Admin.Api",
            Version: typeof(PingEndpoint).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            ServerTime: DateTimeOffset.Now);

        return Send.OkAsync(response, ct);
    }
}