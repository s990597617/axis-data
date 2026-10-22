using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 物料分类菜单与按钮种子。
/// </summary>
/// <remarks>
/// 挂在已有「主数据」目录下；仅在 EnableInitSeed=true 时导入。
/// 角色授权请在菜单管理中分配。
/// </remarks>
public sealed class Mdm_MaterialCategoryMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800300100L, 10000006800004360L, MenuTypeEnum.Menu,
            "物料分类", "MdmMaterialCategory", "/mdm/material-category",
            "modules/mdm/views/material-category/index",
            "mdm:materialCategory", "ant-design:apartment-outlined", 90, false);

        yield return Item(10000006800300101L, 10000006800300100L, MenuTypeEnum.Btn,
            "查询物料分类", string.Empty, string.Empty,
            permission: MdmPermissionCodes.MaterialCategory.Query, order: 1, isPlatform: false);
        yield return Item(10000006800300102L, 10000006800300100L, MenuTypeEnum.Btn,
            "新增物料分类", string.Empty, string.Empty,
            permission: MdmPermissionCodes.MaterialCategory.Create, order: 2, isPlatform: false);
        yield return Item(10000006800300103L, 10000006800300100L, MenuTypeEnum.Btn,
            "编辑物料分类", string.Empty, string.Empty,
            permission: MdmPermissionCodes.MaterialCategory.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800300104L, 10000006800300100L, MenuTypeEnum.Btn,
            "删除物料分类", string.Empty, string.Empty,
            permission: MdmPermissionCodes.MaterialCategory.Delete, order: 4, isPlatform: false);
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
