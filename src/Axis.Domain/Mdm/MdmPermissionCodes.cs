using Axis.Infrastructure.Permissions;

namespace Axis.Domain.Mdm;

/// <summary>
/// Mdm 域权限码。
/// </summary>
/// <remarks>
/// 命名统一采用 <c>mdm:模块:动作</c>；带特性的常量会进入 PermissionCatalog。
/// </remarks>
public static class MdmPermissionCodes
{
    /// <summary>
    /// 物料档案权限。
    /// </summary>
    [PermissionModule("material", "物料档案", Order = 10)]
    public static class Material
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "mdm:material:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "mdm:material:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "mdm:material:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "mdm:material:delete";
    }

    /// <summary>
    /// 物料分类权限。
    /// </summary>
    [PermissionModule("materialCategory", "物料分类", Order = 20)]
    public static class MaterialCategory
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "mdm:materialCategory:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "mdm:materialCategory:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "mdm:materialCategory:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "mdm:materialCategory:delete";
    }

    /// <summary>
    /// 工序权限。
    /// </summary>
    [PermissionModule("process", "工序", Order = 30)]
    public static class Process
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "mdm:process:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "mdm:process:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "mdm:process:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "mdm:process:delete";
    }

    /// <summary>
    /// 工艺路线权限。
    /// </summary>
    [PermissionModule("routing", "工艺路线", Order = 40)]
    public static class Routing
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "mdm:routing:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "mdm:routing:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "mdm:routing:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "mdm:routing:delete";
    }
}
