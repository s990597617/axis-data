using System.Text.RegularExpressions;
using Axis.Application.System.CodeRule.Dto;
using Axis.Application.System.CodeRule.Internal;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.CodeRule;

/// <summary>
/// 统一取号实现：读规则缓存、占流水、渲染模板。
/// </summary>
public sealed class CodeGenerator(
    ISysCodeRuleCache ruleCache,
    ISysCodeSerialRepository serialRepository,
    ICurrentAccount currentAccount,
    IUnitOfWork unitOfWork)
    : ICodeGenerator, IAutoInject
{
    private const string DefaultScopeKey = "_";

    /// <summary>
    /// 按规则编码占用流水并返回完整编码。
    /// </summary>
    /// <param name="ruleCode">规则编码。</param>
    /// <param name="context">取号上下文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>完整编码与拆段。</returns>
    public async Task<CodeGenerateResult> NextAsync(
        string ruleCode,
        CodeGenerateContext? context = null,
        CancellationToken ct = default)
    {
        if (context?.SerialOverride is not null)
            throw new BusinessException("取号不允许覆盖流水");

        var rule = await LoadEnabledRuleAsync(ruleCode, ct);
        if (rule.SourceType == CodeSourceTypeEnum.External)
            throw new BusinessException($"规则 '{ruleCode}' 为外部传入，不能取号");

        var now = context?.Now ?? DateTime.Now;
        var tenantId = context?.TenantId ?? currentAccount.TenantId;
        var variables = context?.Variables ?? new Dictionary<string, string>();
        CodeTemplateEngine.EnsureVariables(rule.Template, MergeBuiltinSkip(variables));

        var owned = EnsureTransaction();
        try
        {
            var serial = await serialRepository.IncrementAsync(
                tenantId, rule.Id, CodeTemplateEngine.PeriodKey(rule.ResetCycle, now), DefaultScopeKey, ct);
            var (code, segments) = CodeTemplateEngine.Render(rule, serial, now, variables);
            if (owned) await unitOfWork.CommitAsync(ct);
            return new CodeGenerateResult { Value = code, RuleId = rule.Id, RuleCode = rule.Code, Segments = segments };
        }
        catch
        {
            if (owned) await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// 按已保存规则预览下一号，不占用流水。
    /// </summary>
    /// <param name="ruleCode">规则编码。</param>
    /// <param name="context">预览上下文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>预览结果。</returns>
    public async Task<CodePreviewDto> PreviewAsync(
        string ruleCode,
        CodeGenerateContext? context = null,
        CancellationToken ct = default)
    {
        var rule = await LoadEnabledRuleAsync(ruleCode, ct);
        var now = context?.Now ?? DateTime.Now;
        var tenantId = context?.TenantId ?? currentAccount.TenantId;
        var serial = context?.SerialOverride
                     ?? await serialRepository.PeekNextAsync(
                         tenantId, rule.Id, CodeTemplateEngine.PeriodKey(rule.ResetCycle, now), DefaultScopeKey, ct);
        return BuildPreview(rule, serial, now, context);
    }

    /// <summary>
    /// 按未保存表单预览编码并生成反向正则。
    /// </summary>
    /// <param name="input">规则表单。</param>
    /// <param name="context">预览上下文。</param>
    /// <returns>预览结果。</returns>
    public CodePreviewDto PreviewTemplate(SaveCodeRuleInput input, CodeGenerateContext? context = null)
    {
        var rule = CodeTemplateEngine.ToSnapshot(input);
        var serial = context?.SerialOverride ?? 1;
        return BuildPreview(rule, serial, context?.Now ?? DateTime.Now, context);
    }

    /// <summary>
    /// 按优先级匹配反向正则。
    /// </summary>
    /// <param name="code">待解析编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>命中结果；无一匹配时为 null。</returns>
    public async Task<CodeParseResult?> ParseAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var rules = await ruleCache.ListEnabledAsync(ct);
        foreach (var rule in rules)
        {
            var pattern = string.IsNullOrWhiteSpace(rule.ReverseRegex)
                ? CodeTemplateEngine.BuildRegex(rule)
                : rule.ReverseRegex;
            if (!Regex.IsMatch(code, pattern)) continue;
            return new CodeParseResult { RuleCode = rule.Code, RuleId = rule.Id, Code = code };
        }

        return null;
    }

    /// <summary>
    /// 校验外来码是否符合指定规则。
    /// </summary>
    /// <param name="ruleCode">规则编码。</param>
    /// <param name="code">待校验编码。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task ValidateAsync(string ruleCode, string code, CancellationToken ct = default)
    {
        var rule = await LoadEnabledRuleAsync(ruleCode, ct);
        var pattern = string.IsNullOrWhiteSpace(rule.ReverseRegex)
            ? CodeTemplateEngine.BuildRegex(rule)
            : rule.ReverseRegex;
        if (!Regex.IsMatch(code ?? string.Empty, pattern))
            throw new BusinessException($"编码 '{code}' 不符合规则 '{ruleCode}'");
    }

    /// <summary>
    /// 加载启用中的规则；缺失则抛业务异常。
    /// </summary>
    private async Task<Axis.Domain.Data.System.Cache.CodeRuleCacheItem> LoadEnabledRuleAsync(
        string ruleCode, CancellationToken ct)
        => await ruleCache.GetEnabledAsync(ruleCode, ct)
           ?? throw new BusinessException($"编码规则 '{ruleCode}' 不存在或已禁用");

    /// <summary>
    /// 组装预览 DTO。
    /// </summary>
    private static CodePreviewDto BuildPreview(
        Axis.Domain.Data.System.Cache.CodeRuleCacheItem rule,
        long serial,
        DateTime now,
        CodeGenerateContext? context)
    {
        var variables = context?.Variables ?? SampleVariables();
        var (code, segments) = CodeTemplateEngine.Render(rule, serial, now, variables);
        return new CodePreviewDto
        {
            Sample = code,
            Segments = segments,
            Length = code.Length,
            ResetCycle = rule.ResetCycle,
            CheckAlgo = rule.CheckAlgo,
            SourceType = rule.SourceType,
            GeneratedRegex = CodeTemplateEngine.BuildRegex(rule)
        };
    }

    /// <summary>
    /// 管理端预览用的示例变量，避免未传物料/产线时无法渲染。
    /// </summary>
    private static Dictionary<string, string> SampleVariables()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["Material"] = "MAT01",
            ["Line"] = "L01",
            ["Shift"] = "A"
        };

    /// <summary>
    /// 仅用于变量齐套检查：内置占位符不要求调用方传入。
    /// </summary>
    private static IReadOnlyDictionary<string, string> MergeBuiltinSkip(
        IReadOnlyDictionary<string, string> variables)
        => variables;

    /// <summary>
    /// 无外层事务时自行开启，保证占号与业务写可同事务提交。
    /// </summary>
    private bool EnsureTransaction()
    {
        if (unitOfWork.HasActiveTransaction) return false;
        unitOfWork.Begin();
        return true;
    }
}
