using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     组织机构种子（模拟完整企业组织架构）
/// </summary>
public sealed class SysOrgSeed : ISqlSugarEntitySeedData<SysOrg>
{
    public IEnumerable<SysOrg> HasData()
    {
        // 集团
        yield return Org(SysSeedIds.OrgRoot, 0, "云枢科技集团", "AXIS", OrgTypeEnum.Group, 1, "集团总部");

        // 公司
        yield return Org(SysSeedIds.OrgTechCompany, SysSeedIds.OrgRoot, "云枢信息技术有限公司", "AXIS-IT", OrgTypeEnum.Company, 1, "核心技术研发子公司");

        // 部门
        yield return Org(SysSeedIds.OrgDevDept, SysSeedIds.OrgTechCompany, "研发部", "DEV", OrgTypeEnum.Department, 1, "负责产品核心研发");
        yield return Org(SysSeedIds.OrgProdDept, SysSeedIds.OrgTechCompany, "产品部", "PROD", OrgTypeEnum.Department, 2, "负责产品规划与设计");
        yield return Org(SysSeedIds.OrgHrDept, SysSeedIds.OrgTechCompany, "人力资源部", "HR", OrgTypeEnum.Department, 3, "负责招聘、培训、薪酬");
        yield return Org(SysSeedIds.OrgFinDept, SysSeedIds.OrgTechCompany, "财务部", "FIN", OrgTypeEnum.Department, 4, "负责财务核算与预算管理");

        // 小组
        yield return Org(SysSeedIds.OrgBackendTeam, SysSeedIds.OrgDevDept, "后端开发组", "DEV-BE", OrgTypeEnum.Team, 1, "Java/.NET 后端服务开发");
        yield return Org(SysSeedIds.OrgFrontendTeam, SysSeedIds.OrgDevDept, "前端开发组", "DEV-FE", OrgTypeEnum.Team, 2, "Vue/React 前端应用开发");
        yield return Org(SysSeedIds.OrgQaTeam, SysSeedIds.OrgDevDept, "测试组", "DEV-QA", OrgTypeEnum.Team, 3, "质量保障与自动化测试");
    }

    private static SysOrg Org(long id, long parentId, string name, string code, OrgTypeEnum type, int order, string remark)
    {
        return new SysOrg
        {
            Id = id,
            TenantId = SysSeedIds.TenantDefault,
            ParentId = parentId,
            Name = name,
            Code = code,
            Type = type,
            OrderNo = order,
            Status = StatusEnum.Enable,
            Remark = remark,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}