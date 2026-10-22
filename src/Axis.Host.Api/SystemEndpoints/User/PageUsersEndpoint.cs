using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 用户分页列表
/// </summary>
public sealed class PageUsersEndpoint(IUserAppService userService)
    : AxisEndpoint<UserPageQuery, SqlSugarPagedList<UserDto>>
{
    public override void Configure()
    {
        Get("/sys/user/page");
        Summary(s =>
        {
            s.Summary = "用户分页列表";
            s.Description = "支持按机构、关键词、状态过滤";
        });
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Query));
    }

    public override async Task HandleAsync(UserPageQuery req, CancellationToken ct)
    {
        Response = await userService.PageAsync(req, ct);
    }
}
