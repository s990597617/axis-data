using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 按入参测试数据源连通性（可不依赖已保存实体）。
/// </summary>
public sealed class TestSourceConnectionEndpoint(ISourceAppService service)
    : AxisEndpoint<TestSourceConnectionInput, TestSourceConnectionResultDto>
{
    public override void Configure()
    {
        Post("/datahub/source/test-connection");
        Summary(summary => summary.Summary = "测试数据源连通性");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Edit));
    }

    public override async Task HandleAsync(TestSourceConnectionInput request, CancellationToken ct)
        => Response = await service.TestConnectionAsync(request, ct);
}