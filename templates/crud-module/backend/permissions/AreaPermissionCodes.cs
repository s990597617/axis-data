using Axis.Infrastructure.Permissions;

namespace Axis.Domain.__Area__;

/// <summary>
/// __Area__ 域权限码。
/// </summary>
/// <remarks>
/// 命名统一采用 <c>__PermArea__:模块:动作</c>；带特性的常量会进入 PermissionCatalog。
/// </remarks>
public static class __Area__PermissionCodes
{
    /// <summary>
    /// __CnName__管理权限。
    /// </summary>
    [PermissionModule("__route_key__", "__CnName__管理", Order = 10)]
    public static class __Module__
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "__PermPrefix__:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "__PermPrefix__:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "__PermPrefix__:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "__PermPrefix__:delete";
    }
}
