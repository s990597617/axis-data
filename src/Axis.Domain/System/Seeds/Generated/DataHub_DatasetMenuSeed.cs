using Axis.Domain.Enums;
using Axis.Domain.DataHub;
using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：DataHub/Dataset（数据集）菜单与按钮挂载。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权在菜单导入中分配，不在此扩展 RoleMenu 挂载。
/// </remarks>
public sealed class DataHub_DatasetMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        yield return Item(10000006800004170, 0, MenuTypeEnum.Dir,
            "数据中台", "DataHub", "/datahub",
            icon: "ant-design:cloud-server-outlined", order: 50, isPlatform: false);

        yield return Item(10000006800221090, 10000006800004170, MenuTypeEnum.Menu,
            "数据集", "DhDataset", "/datahub/dataset", "modules/datahub/views/dataset/index",
            "datahub:dataset", "ant-design:table-outlined", 110, false);

        yield return Item(10000006800221091, 10000006800221090, MenuTypeEnum.Btn,
            "查询数据集", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Dataset.Query, order: 1, isPlatform: false);
        yield return Item(10000006800221092, 10000006800221090, MenuTypeEnum.Btn,
            "创建数据集", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Dataset.Create, order: 2, isPlatform: false);
        yield return Item(10000006800221093, 10000006800221090, MenuTypeEnum.Btn,
            "编辑数据集", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Dataset.Edit, order: 3, isPlatform: false);
        yield return Item(10000006800221094, 10000006800221090, MenuTypeEnum.Btn,
            "删除数据集", string.Empty, string.Empty,
            permission: DataHubPermissionCodes.Dataset.Delete, order: 4, isPlatform: false);
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
