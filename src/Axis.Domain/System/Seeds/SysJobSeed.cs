using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database;

namespace Axis.Domain.System.Seeds;

/// <summary>
/// 定时任务种子（内置刷新令牌清理）。
/// </summary>
/// <remarks>
/// 任务编码常量供 <c>IAxisJob.Code</c> 引用。行数据作为空库/CodeFirst 兜底；
/// 运行时以 <c>AxisJobAttribute</c> + <c>EnsureCatalogSyncedAsync</c> 为准做幂等补齐，
/// 不会覆盖运维已修改的 Cron / 状态 / 参数。
/// </remarks>
public sealed class SysJobSeed : ISqlSugarEntitySeedData<SysJob>
{

    /// <summary>
    /// 刷新令牌清理任务编码（与 IAxisJob 实现对齐）。
    /// </summary>
    public const string CleanupRefreshTokenJobCode = "sys:cleanup-refresh-token";

    public IEnumerable<SysJob> HasData()
    {

        yield return new SysJob
        {
            Id = SysSeedIds.JobCleanupRefreshToken,
            JobCode = CleanupRefreshTokenJobCode,
            Name = "清理刷新令牌",
            GroupName = "system",
            Cron = "0 0 3 * * ?",
            Concurrent = false,
            Params = """{"retainDays":7}""",
            IsBuiltin = true,
            OrderNo = 10,
            Status = StatusEnum.Enable,
            Remark = "每天 03:00 清理过期/已吊销/已消费超过保留天数的刷新令牌",
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}
