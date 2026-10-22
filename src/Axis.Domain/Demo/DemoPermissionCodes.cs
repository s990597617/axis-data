using Axis.Infrastructure.Permissions;

namespace Axis.Domain.Demo;

/// <summary>
/// Demo 域权限码。
/// </summary>
/// <remarks>
/// 命名统一采用 <c>demo:模块:动作</c>；带特性的常量会进入 PermissionCatalog。
/// </remarks>
public static class DemoPermissionCodes
{
    /// <summary>
    /// 产品管理权限。
    /// </summary>
    [PermissionModule("product", "产品管理", Order = 10)]
    public static class Product
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "demo:product:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "demo:product:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "demo:product:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "demo:product:delete";
    }

    /// <summary>
    /// 岗位副本管理权限。
    /// </summary>
    [PermissionModule("pos-clone", "岗位副本管理", Order = 200)]
    public static class PosClone
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "demo:pos-clone:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "demo:pos-clone:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "demo:pos-clone:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "demo:pos-clone:delete";
    }

    /// <summary>
    /// SKU管理权限。
    /// </summary>
    [PermissionModule("sku", "SKU管理", Order = 200)]
    public static class Sku
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "demo:sku:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "demo:sku:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "demo:sku:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "demo:sku:delete";
    }

    /// <summary>
    /// 品牌管理权限.
    /// </summary>
    [PermissionModule("brand", "品牌管理", Order = 200)]
    public static class Brand
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "demo:brand:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "demo:brand:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "demo:brand:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "demo:brand:delete";
    }
}
