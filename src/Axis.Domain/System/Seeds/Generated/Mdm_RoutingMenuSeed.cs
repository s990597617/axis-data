using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 工艺路线菜单与按钮种子。
/// </summary>
/// <remarks>
/// 挂在已有「主数据」目录下；仅在 EnableInitSeed=true 时导入。
/// </remarks>
public sealed class Mdm_RoutingMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800300300L, 10000006800004360L, MenuTypeEnum.Menu,
            "工艺路线", "MdmRouting", "/mdm/routing",
            "modules/mdm/views/routing/index",
            "mdm:routing", "ant-design:node-index-outlined", 120, false);

        yield return Item(10000006800300301L, 10000006800300300L, MenuTypeEnum.Btn,
            "查询工艺路线", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Routing.Query, order: 1, isPlatform: false);
        yield return Item(10000006800300302L, 10000006800300300L, MenuTypeEnum.Btn,
            "新增工艺路线", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Routing.Create, order: 2, isPlatform: false);
        yield return Item(10000006800300303L, 10000006800300300L, MenuTypeEnum.Btn,
            "编辑工艺路线", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Routing.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800300304L, 10000006800300300L, MenuTypeEnum.Btn,
            "删除工艺路线", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Routing.Delete, order: 4, isPlatform: false);
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
