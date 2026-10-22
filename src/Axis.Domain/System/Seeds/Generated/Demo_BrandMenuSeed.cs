using Axis.Domain.Enums;
using Axis.Domain.Demo;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：Demo/Brand（品牌）菜单与按钮种子。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权请在菜单管理中分配，或自行扩展 RoleMenu 种子。
/// </remarks>
public sealed class Demo_BrandMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800005550L, 0, MenuTypeEnum.Dir,
            "Demo", "Demo", "/demo",
            icon: "ant-design:appstore-outlined", order: 50, isPlatform: false);

        yield return Item(10000006800428850L, 10000006800005550L, MenuTypeEnum.Menu,
            "品牌", "SysBrand", "/demo/brand", "modules/demo/views/brand/index",
            "demo:brand", "ant-design:table-outlined", 100, false);

        yield return Item(10000006800428851L, 10000006800428850L, MenuTypeEnum.Btn,
            "查询品牌", string.Empty, string.Empty,
            permission: DemoPermissionCodes.Brand.Query, order: 1, isPlatform: false);
        yield return Item(10000006800428852L, 10000006800428850L, MenuTypeEnum.Btn,
            "新增品牌", string.Empty, string.Empty,
            permission: DemoPermissionCodes.Brand.Create, order: 2, isPlatform: false);
        yield return Item(10000006800428853L, 10000006800428850L, MenuTypeEnum.Btn,
            "编辑品牌", string.Empty, string.Empty,
            permission: DemoPermissionCodes.Brand.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800428854L, 10000006800428850L, MenuTypeEnum.Btn,
            "删除品牌", string.Empty, string.Empty,
            permission: DemoPermissionCodes.Brand.Delete, order: 4, isPlatform: false);
    }

    private static SysMenu Item(
        long id, long parentId, MenuTypeEnum type, string title, string name,
        string path, string component = null, string permission = null,
        string icon = null, int order = 100, bool isPlatform = false)
    {
        return new SysMenu
        {
            Id = id,
            ParentId = parentId,
            Type = type,
            Title = title,
            Name = name,
            Path = path,
            Component = component,
            Permission = permission,
            Icon = icon,
            OrderNo = order,
            IsPlatform = isPlatform,
            Status = StatusEnum.Enable,
            IsKeepAlive = true,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}
