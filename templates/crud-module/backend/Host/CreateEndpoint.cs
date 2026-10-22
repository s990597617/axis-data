using Axis.Application.__Area__.__Module__;
using Axis.Application.__Area__.__Module__.Dto;
using Axis.Domain.__Area__;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.__Area__Endpoints.__Module__;

/// <summary>
/// 新增__CnName__。
/// </summary>
public sealed class Create__Module__Endpoint(I__Module__AppService service) : AxisEndpoint<Save__Module__Input>
{
    public override void Configure()
    {
        Post("__RoutePrefix__");
        Summary(summary => summary.Summary = "新增__CnName__");
        Options(options => options.WithTags("__CnName__管理")
            .WithGroupName(nameof(ApiGroupEnum.__ApiGroup__))
            .RequirePermission(__PermissionCodesType__.__Module__.Create));
    }

    public override async Task HandleAsync(Save__Module__Input request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
