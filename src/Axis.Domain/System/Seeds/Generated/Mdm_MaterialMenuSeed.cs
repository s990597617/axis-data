using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：Mdm/Material（物料）菜单与按钮种子。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权请在菜单管理中分配，或自行扩展 RoleMenu 种子。
/// </remarks>
public sealed class Mdm_MaterialMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800004360L, 0, MenuTypeEnum.Dir,
            "主数据", "Mdm", "/mdm",
            icon: "ant-design:database-outlined", order: 40, isPlatform: false);

        yield return Item(10000006800286560L, 10000006800004360, MenuTypeEnum.Menu,
            "物料档案", "MdmMaterial", "/mdm/material", "modules/mdm/views/material/index",
            "mdm:material", "ant-design:table-outlined", 100, false);

        yield return Item(10000006800286561L, 10000006800286560L, MenuTypeEnum.Btn,
            "查询物料档案", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Material.Query, order: 1, isPlatform: false);
        yield return Item(10000006800286562L, 10000006800286560L, MenuTypeEnum.Btn,
            "新增物料档案", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Material.Create, order: 2, isPlatform: false);
        yield return Item(10000006800286563L, 10000006800286560L, MenuTypeEnum.Btn,
            "编辑物料档案", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Material.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800286564L, 10000006800286560L, MenuTypeEnum.Btn,
            "删除物料档案", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Material.Delete, order: 4, isPlatform: false);
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
