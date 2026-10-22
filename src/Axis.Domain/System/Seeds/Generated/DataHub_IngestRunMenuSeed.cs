using Axis.Domain.Enums;
using Axis.Domain.DataHub;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：DataHub/IngestRun（采集运行）菜单与按钮挂载。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权在菜单导入中分配，不在此扩展 RoleMenu 挂载。
/// </remarks>
public sealed class DataHub_IngestRunMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800004170, 0, MenuTypeEnum.Dir,
            "DataHub", "DataHub", "/datahub",
            icon: "ant-design:appstore-outlined", order: 50, isPlatform: false);

        yield return Item(10000006800234150, 10000006800004170, MenuTypeEnum.Menu,
            "采集运行", "DhIngestRun", "/datahub/ingest-run", "modules/datahub/views/ingest-run/index",
            "datahub:ingest-run", "ant-design:table-outlined", 130, false);

        yield return Item(10000006800234151, 10000006800234150, MenuTypeEnum.Btn,
            "查询采集运行", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.IngestRun.Query, order: 1, isPlatform: false);
        yield return Item(10000006800234152, 10000006800234150, MenuTypeEnum.Btn,
            "创建采集运行", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.IngestRun.Create, order: 2, isPlatform: false);
        yield return Item(10000006800234153, 10000006800234150, MenuTypeEnum.Btn,
            "编辑采集运行", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.IngestRun.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800234154, 10000006800234150, MenuTypeEnum.Btn,
            "删除采集运行", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.IngestRun.Delete, order: 4, isPlatform: false);
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
