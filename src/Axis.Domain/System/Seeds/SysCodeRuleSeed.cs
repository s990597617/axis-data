using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
/// 内置编码规则种子：物料 / 工序 / 工艺路线 / 工单 / 报工 / 检验。
/// </summary>
/// <remarks>
/// 编码与 <see cref="CodeRuleCodes"/> 对齐；模板可在管理端调整，编码不可改。
/// </remarks>
public sealed class SysCodeRuleSeed : ISqlSugarEntitySeedData<SysCodeRule>
{
    /// <summary>
    /// 返回默认租户下的内置规则。
    /// </summary>
    /// <returns>内置编码规则集合。</returns>
    public IEnumerable<SysCodeRule> HasData()
    {
        yield return Rule(
            SysSeedIds.CodeRuleMat,
            CodeRuleCodes.Material,
            "物料编码",
            "M",
            null,
            5,
            CodeResetCycleEnum.Never,
            "{Prefix}{Serial}",
            @"^M\d{5}$",
            10);

        yield return Rule(
            SysSeedIds.CodeRulePrc,
            CodeRuleCodes.Process,
            "工序编码",
            "P",
            null,
            5,
            CodeResetCycleEnum.Never,
            "{Prefix}{Serial}",
            @"^P\d{5}$",
            15);

        yield return Rule(
            SysSeedIds.CodeRuleRtg,
            CodeRuleCodes.Routing,
            "工艺路线编码",
            "R",
            null,
            5,
            CodeResetCycleEnum.Never,
            "{Prefix}{Serial}",
            @"^R\d{5}$",
            16);

        yield return Rule(
            SysSeedIds.CodeRuleWo,
            CodeRuleCodes.WorkOrder,
            "生产工单号",
            "WO",
            "yyyyMMdd",
            5,
            CodeResetCycleEnum.Daily,
            "{Prefix}-{Date}-{Serial}",
            @"^WO-\d{8}-\d{5}$",
            20);

        yield return Rule(
            SysSeedIds.CodeRuleRpt,
            CodeRuleCodes.Report,
            "报工单号",
            "RPT",
            "yyyyMMdd",
            5,
            CodeResetCycleEnum.Daily,
            "{Prefix}-{Date}-{Serial}",
            @"^RPT-\d{8}-\d{5}$",
            30);

        yield return Rule(
            SysSeedIds.CodeRuleIqc,
            CodeRuleCodes.Inspection,
            "检验单号",
            "IQC",
            "yyyyMMdd",
            5,
            CodeResetCycleEnum.Daily,
            "{Prefix}-{Date}-{Serial}",
            @"^IQC-\d{8}-\d{5}$",
            40);
    }

    private static SysCodeRule Rule(
        long id,
        string code,
        string name,
        string prefix,
        string? dateFormat,
        int serialLength,
        CodeResetCycleEnum resetCycle,
        string template,
        string reverseRegex,
        int priority)
    {
        return new SysCodeRule
        {
            Id = id,
            TenantId = SysSeedIds.TenantDefault,
            Code = code,
            Name = name,
            Prefix = prefix,
            DateFormat = dateFormat,
            SerialLength = serialLength,
            ResetCycle = resetCycle,
            Template = template,
            CheckAlgo = CodeCheckAlgoEnum.None,
            SourceType = CodeSourceTypeEnum.SelfGenerated,
            ReverseRegex = reverseRegex,
            Priority = priority,
            IsBuiltin = true,
            Status = StatusEnum.Enable,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}
