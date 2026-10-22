using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     默认租户种子
/// </summary>
public sealed class SysTenantSeed : ISqlSugarEntitySeedData<SysTenant>
{
    public IEnumerable<SysTenant> HasData()
    {
        yield return new SysTenant
        {
            Id = SysSeedIds.TenantDefault,
            Name = "云枢科技集团",
            Code = "axis-tech",
            Type = TenantTypeEnum.Id,
            AdminUserId = SysSeedIds.UserAdmin,
            ProvisionStatus = TenantProvisionStatusEnum.Ready,
            OrderNo = 1,
            Status = StatusEnum.Enable,
            Remark = "平台默认租户，拥有完整功能权限",
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}