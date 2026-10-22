using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Security.OpenApi;

namespace Axis.Host.Api.SystemEndpoints.OpenApi;

/// <summary>
/// OpenAPI 签名联调示例：仅 Signature 认证，跳过管理端防重放头。
/// </summary>
public sealed class OpenApiWhoAmIEndpoint(ICurrentAccount currentAccount)
    : AxisEndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get("/open-api/whoami");
        Summary(s => s.Summary = "OpenAPI 签名认证联调（返回当前绑定用户）");
        AuthSchemes(SignatureAuthenticationDefaults.AuthenticationScheme);
        Options(x => x.WithTags("OpenAPI")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .SkipAntiReplay("OpenAPI 签名自带 nonce"));
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        Response = new
        {
            currentAccount.Id,
            Account = currentAccount.AccountName,
            currentAccount.RealName,
            currentAccount.TenantId,
            currentAccount.OrgId,
            currentAccount.OrgName,
            currentAccount.AccountType
        };
        return Task.CompletedTask;
    }
}
