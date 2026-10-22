using Axis.Domain.Enums;
using Axis.Domain.DataHub;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：DataHub/Source（数据源）菜单与按钮挂载。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权在菜单导入中分配，不在此扩展 RoleMenu 挂载。
/// </remarks>
public sealed class DataHub_SourceMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800004170, 0, MenuTypeEnum.Dir,
            "DataHub", "DataHub", "/datahub",
            icon: "ant-design:appstore-outlined", order: 50, isPlatform: false);

        yield return Item(10000006800241590, 10000006800004170, MenuTypeEnum.Menu,
            "数据源", "DhSource", "/datahub/source", "modules/datahub/views/source/index",
            "datahub:source", "ant-design:table-outlined", 100, false);

        yield return Item(10000006800241591, 10000006800241590, MenuTypeEnum.Btn,
            "查询数据源", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Source.Query, order: 1, isPlatform: false);
        yield return Item(10000006800241592, 10000006800241590, MenuTypeEnum.Btn,
            "创建数据源", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Source.Create, order: 2, isPlatform: false);
        yield return Item(10000006800241593, 10000006800241590, MenuTypeEnum.Btn,
            "编辑数据源", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Source.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800241594, 10000006800241590, MenuTypeEnum.Btn,
            "删除数据源", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Source.Delete, order: 4, isPlatform: false);
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
