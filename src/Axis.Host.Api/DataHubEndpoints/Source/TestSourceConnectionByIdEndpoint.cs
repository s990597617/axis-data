using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 按已保存数据源测试连通性并回写最近测试结果。
/// </summary>
public sealed class TestSourceConnectionByIdEndpoint(ISourceAppService service)
    : AxisEndpoint<SourceIdRequest, TestSourceConnectionResultDto>
{
    public override void Configure()
    {
        Post("/datahub/source/{Id}/test-connection");
        Summary(summary => summary.Summary = "测试已保存数据源连通性");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Edit));
    }

    public override async Task HandleAsync(SourceIdRequest request, CancellationToken ct)
        => Response = await service.TestConnectionByIdAsync(request.Id, ct);
}