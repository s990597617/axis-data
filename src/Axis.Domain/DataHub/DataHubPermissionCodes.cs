using Axis.Infrastructure.Permissions;

namespace Axis.Domain.DataHub;

/// <summary>
/// DataHub 鉴权码。
/// </summary>
/// <remarks>
/// 命名统一为 <c>datahub:模块:动作</c>，带特性的常量会进入 PermissionCatalog。
/// </remarks>
public static class DataHubPermissionCodes
{
    /// <summary>
    /// 数据源管理权限。
    /// </summary>
    [PermissionModule("source", "数据源管理", Order = 10)]
    public static class Source
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "datahub:source:query";

        [PermissionAction("创建", Order = 2)]
        public const string Create = "datahub:source:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "datahub:source:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "datahub:source:delete";
    }

    /// <summary>
    /// 数据集管理权限。
    /// </summary>
    [PermissionModule("dataset", "数据集管理", Order = 20)]
    public static class Dataset
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "datahub:dataset:query";

        [PermissionAction("创建", Order = 2)]
        public const string Create = "datahub:dataset:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "datahub:dataset:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "datahub:dataset:delete";
    }

    /// <summary>
    /// 采集任务管理权限。
    /// </summary>
    [PermissionModule("ingest-job", "采集任务管理", Order = 30)]
    public static class IngestJob
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "datahub:ingest-job:query";

        [PermissionAction("创建", Order = 2)]
        public const string Create = "datahub:ingest-job:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "datahub:ingest-job:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "datahub:ingest-job:delete";
    }

    /// <summary>
    /// 采集运行管理权限。
    /// </summary>
    [PermissionModule("ingest-run", "采集运行管理", Order = 40)]
    public static class IngestRun
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "datahub:ingest-run:query";

        [PermissionAction("创建", Order = 2)]
        public const string Create = "datahub:ingest-run:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "datahub:ingest-run:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "datahub:ingest-run:delete";
    }
}
