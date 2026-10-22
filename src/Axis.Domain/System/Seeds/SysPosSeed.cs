using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     岗位种子（模拟常见企业职位体系）
/// </summary>
public sealed class SysPosSeed : ISqlSugarEntitySeedData<SysPos>
{
    public IEnumerable<SysPos> HasData()
    {
        yield return Pos(SysSeedIds.PosCeo, "总经理", "CEO", 1);
        yield return Pos(SysSeedIds.PosCto, "技术总监", "CTO", 2);
        yield return Pos(SysSeedIds.PosDevManager, "研发经理", "DEV-MGR", 3);
        yield return Pos(SysSeedIds.PosDeveloper, "开发工程师", "DEV-ENG", 4);
        yield return Pos(SysSeedIds.PosDesigner, "产品设计师", "PD", 5);
        yield return Pos(SysSeedIds.PosHr, "人事专员", "HR-SPEC", 6);
        yield return Pos(SysSeedIds.PosFinance, "财务专员", "FIN-SPEC", 7);
    }

    private static SysPos Pos(long id, string name, string code, int order)
    {
        return new SysPos
        {
            Id = id,
            TenantId = SysSeedIds.TenantDefault,
            Name = name,
            Code = code,
            OrderNo = order,
            Status = StatusEnum.Enable,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}