using Axis.Application.__Area__.__Module__;
using Axis.Application.__Area__.__Module__.Dto;
using Axis.Domain.__Area__;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.__Area__Endpoints.__Module__;

/// <summary>
/// 删除__CnName__。
/// </summary>
public sealed class Delete__Module__Endpoint(I__Module__AppService service) : AxisEndpoint<__Module__IdRequest>
{
    public override void Configure()
    {
        Delete("__RoutePrefix__/{Id}");
        Summary(summary => summary.Summary = "删除__CnName__");
        Options(options => options.WithTags("__CnName__管理")
            .WithGroupName(nameof(ApiGroupEnum.__ApiGroup__))
            .RequirePermission(__PermissionCodesType__.__Module__.Delete));
    }

    public override async Task HandleAsync(__Module__IdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
