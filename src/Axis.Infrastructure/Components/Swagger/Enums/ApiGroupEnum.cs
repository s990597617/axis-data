using Axis.Infrastructure.Components.Swagger.Attributes;

namespace Axis.Infrastructure.Components.Swagger.Enums;

/// <summary>
/// api 分组枚举
/// </summary>
[Flags]
public enum ApiGroupEnum
{
    [ApiGroupInfo(Title = "登录授权模块", Version = "v1", Description = "系统登录模块接口集合")]
    AdminAuthorModule = 1,
    [ApiGroupInfo(Title = "后台系统模块", Version = "v1", Description = "系统后台系统模块集合")]
    AdminSystemModule = 2,
    [ApiGroupInfo(Title = "基础业务模块", Version = "v1", Description = "系统基础业务模块集合")]
    AdminBaseModule = 3,
    [ApiGroupInfo(Title = "MES模块", Version = "v1", Description = "制造执行模块接口集合")]
    MesModule = 4
}