using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
/// 内置编码规则种子：工单 / 报工 / 检验。
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
