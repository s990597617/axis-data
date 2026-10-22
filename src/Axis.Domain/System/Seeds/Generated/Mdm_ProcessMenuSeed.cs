using Axis.Domain.Enums;
using Axis.Domain.Mdm;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 工序菜单与按钮种子。
/// </summary>
/// <remarks>
/// 挂在已有「主数据」目录下；仅在 EnableInitSeed=true 时导入。
/// </remarks>
public sealed class Mdm_ProcessMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800300200L, 10000006800004360L, MenuTypeEnum.Menu,
            "工序", "MdmProcess", "/mdm/process",
            "modules/mdm/views/process/index",
            "mdm:process", "ant-design:cluster-outlined", 110, false);

        yield return Item(10000006800300201L, 10000006800300200L, MenuTypeEnum.Btn,
            "查询工序", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Process.Query, order: 1, isPlatform: false);
        yield return Item(10000006800300202L, 10000006800300200L, MenuTypeEnum.Btn,
            "新增工序", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Process.Create, order: 2, isPlatform: false);
        yield return Item(10000006800300203L, 10000006800300200L, MenuTypeEnum.Btn,
            "编辑工序", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Process.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800300204L, 10000006800300200L, MenuTypeEnum.Btn,
            "删除工序", string.Empty, string.Empty,
            permission: MdmPermissionCodes.Process.Delete, order: 4, isPlatform: false);
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
