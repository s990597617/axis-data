using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.CodeRule.Internal;

/// <summary>
/// 编码模板渲染与反向正则生成（无 IO）。
/// </summary>
internal static class CodeTemplateEngine
{
    private static readonly Regex PlaceholderRegex = new(@"\{([A-Za-z][A-Za-z0-9]*)\}", RegexOptions.Compiled);

    /// <summary>
    /// 按规则与流水渲染编码，并拆出各段。
    /// </summary>
    /// <param name="rule">规则快照。</param>
    /// <param name="serial">已占用或预览用的流水值。</param>
    /// <param name="now">取号时钟。</param>
    /// <param name="variables">调用方变量。</param>
    /// <returns>完整编码与拆段。</returns>
    public static (string Code, IReadOnlyList<CodeSegmentDto> Segments) Render(
        CodeRuleCacheItem rule,
        long serial,
        DateTime now,
        IReadOnlyDictionary<string, string> variables)
    {
        EnsureTemplateReady(rule);
        var values = BuildValues(rule, serial, now, variables);
        if (rule.Template.Contains("{" + CodePlaceholderNames.Check + "}", StringComparison.Ordinal)
            && rule.CheckAlgo == CodeCheckAlgoEnum.Mod10)
        {
            var body = ReplacePlaceholders(rule.Template, values, skipCheck: true);
            values[CodePlaceholderNames.Check] = ComputeMod10(body).ToString(CultureInfo.InvariantCulture);
        }

        var segments = new List<CodeSegmentDto>();
        var code = ReplacePlaceholders(rule.Template, values, skipCheck: false, segments);
        return (code, segments);
    }

    /// <summary>
    /// 按模板生成反向正则。
    /// </summary>
    /// <param name="rule">规则快照。</param>
    /// <returns>锚定后的正则。</returns>
    public static string BuildRegex(CodeRuleCacheItem rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Template))
            throw new BusinessException("拼接模板不能为空");

        var pattern = new StringBuilder("^");
        var last = 0;
        foreach (Match match in PlaceholderRegex.Matches(rule.Template))
        {
            pattern.Append(Regex.Escape(rule.Template[last..match.Index]));
            pattern.Append(PlaceholderToRegex(rule, match.Groups[1].Value));
            last = match.Index + match.Length;
        }

        pattern.Append(Regex.Escape(rule.Template[last..]));
        pattern.Append('$');
        return pattern.ToString();
    }

    /// <summary>
    /// 校验模板所需变量是否齐套。
    /// </summary>
    /// <param name="template">拼接模板。</param>
    /// <param name="variables">调用方变量。</param>
    public static void EnsureVariables(string template, IReadOnlyDictionary<string, string> variables)
    {
        foreach (Match match in PlaceholderRegex.Matches(template ?? string.Empty))
        {
            var name = match.Groups[1].Value;
            if (IsBuiltin(name)) continue;
            if (variables.TryGetValue(name, out var value) && !string.IsNullOrWhiteSpace(value))
                continue;
            throw new BusinessException($"编码模板缺少变量 {{{name}}}");
        }
    }

    /// <summary>
    /// 将未保存表单转为可渲染的规则快照。
    /// </summary>
    /// <param name="input">管理端表单。</param>
    /// <returns>规则快照。</returns>
    public static CodeRuleCacheItem ToSnapshot(SaveCodeRuleInput input)
        => new()
        {
            Code = input.Code,
            Name = input.Name,
            Prefix = input.Prefix,
            DateFormat = input.DateFormat,
            SerialLength = input.SerialLength,
            ResetCycle = input.ResetCycle,
            Template = input.Template,
            CheckAlgo = input.CheckAlgo,
            SourceType = input.SourceType,
            ReverseRegex = input.ReverseRegex,
            Priority = input.Priority,
            Status = input.Status
        };

    /// <summary>
    /// 计算流水周期键。
    /// </summary>
    /// <param name="cycle">重置周期。</param>
    /// <param name="now">取号时钟。</param>
    /// <returns>周期键。</returns>
    public static string PeriodKey(CodeResetCycleEnum cycle, DateTime now)
        => cycle switch
        {
            CodeResetCycleEnum.Never => "never",
            CodeResetCycleEnum.Daily => now.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
            CodeResetCycleEnum.Monthly => now.ToString("yyyyMM", CultureInfo.InvariantCulture),
            CodeResetCycleEnum.Yearly => now.ToString("yyyy", CultureInfo.InvariantCulture),
            _ => throw new BusinessException("重置周期无效")
        };

    private static void EnsureTemplateReady(CodeRuleCacheItem rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Template))
            throw new BusinessException("拼接模板不能为空");
        if (rule.ResetCycle is CodeResetCycleEnum.Unknown)
            throw new BusinessException("重置周期无效");
        if (rule.CheckAlgo is CodeCheckAlgoEnum.Unknown)
            throw new BusinessException("校验位算法无效");
        if (rule.Template.Contains("{" + CodePlaceholderNames.Date + "}", StringComparison.Ordinal)
            && string.IsNullOrWhiteSpace(rule.DateFormat))
            throw new BusinessException("模板包含 {Date} 时必须填写日期格式");
        if (rule.Template.Contains("{" + CodePlaceholderNames.Check + "}", StringComparison.Ordinal)
            && rule.CheckAlgo == CodeCheckAlgoEnum.None)
            throw new BusinessException("模板包含 {Check} 时必须选择校验位算法");
        if (rule.SerialLength is < 1 or > 16)
            throw new BusinessException("流水长度须在 1～16 之间");
    }

    private static Dictionary<string, string> BuildValues(
        CodeRuleCacheItem rule,
        long serial,
        DateTime now,
        IReadOnlyDictionary<string, string> variables)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [CodePlaceholderNames.Prefix] = rule.Prefix ?? string.Empty,
            [CodePlaceholderNames.Date] = FormatDate(rule.DateFormat, now),
            [CodePlaceholderNames.Serial] = serial.ToString(CultureInfo.InvariantCulture).PadLeft(rule.SerialLength, '0')
        };

        foreach (var pair in variables)
        {
            if (!string.IsNullOrWhiteSpace(pair.Value))
                values[pair.Key] = pair.Value;
        }

        EnsureVariables(rule.Template, values);
        return values;
    }

    private static string FormatDate(string? format, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(format)) return string.Empty;
        try
        {
            return now.ToString(format, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            throw new BusinessException($"日期格式 '{format}' 无效");
        }
    }

    private static string ReplacePlaceholders(
        string template,
        IReadOnlyDictionary<string, string> values,
        bool skipCheck,
        List<CodeSegmentDto>? segments = null)
    {
        var last = 0;
        var builder = new StringBuilder();
        foreach (Match match in PlaceholderRegex.Matches(template))
        {
            AppendLiteral(template[last..match.Index], builder, segments);
            var name = match.Groups[1].Value;
            var value = skipCheck && name == CodePlaceholderNames.Check
                ? string.Empty
                : values.GetValueOrDefault(name, string.Empty);
            builder.Append(value);
            segments?.Add(new CodeSegmentDto { Name = name, Value = value });
            last = match.Index + match.Length;
        }

        AppendLiteral(template[last..], builder, segments);
        return builder.ToString();
    }

    private static void AppendLiteral(string literal, StringBuilder builder, List<CodeSegmentDto>? segments)
    {
        if (literal.Length == 0) return;
        builder.Append(literal);
        segments?.Add(new CodeSegmentDto { Name = "Literal", Value = literal });
    }

    private static string PlaceholderToRegex(CodeRuleCacheItem rule, string name)
        => name switch
        {
            CodePlaceholderNames.Prefix => string.IsNullOrEmpty(rule.Prefix)
                ? @"[\w-]*"
                : Regex.Escape(rule.Prefix),
            CodePlaceholderNames.Date => DateFormatToRegex(rule.DateFormat),
            CodePlaceholderNames.Serial => $@"\d{{{Math.Clamp(rule.SerialLength, 1, 16)}}}",
            CodePlaceholderNames.Check => @"\d",
            _ => @"[A-Za-z0-9_-]+"
        };

    private static string DateFormatToRegex(string? format)
    {
        if (string.IsNullOrWhiteSpace(format)) return @"\d+";
        var pattern = new StringBuilder();
        var count = 0;
        char? current = null;
        foreach (var ch in format)
        {
            if (ch is 'y' or 'M' or 'd' or 'H' or 'm' or 's')
            {
                if (current == ch) { count++; continue; }
                if (current is not null) pattern.Append(@"\d{").Append(count).Append('}');
                current = ch;
                count = 1;
                continue;
            }

            if (current is not null)
            {
                pattern.Append(@"\d{").Append(count).Append('}');
                current = null;
                count = 0;
            }

            pattern.Append(Regex.Escape(ch.ToString()));
        }

        if (current is not null) pattern.Append(@"\d{").Append(count).Append('}');
        return pattern.Length == 0 ? @"\d+" : pattern.ToString();
    }

    private static int ComputeMod10(string body)
    {
        var sum = 0;
        var alt = false;
        for (var i = body.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(body[i])) continue;
            var n = body[i] - '0';
            if (alt)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }

            sum += n;
            alt = !alt;
        }

        return (10 - sum % 10) % 10;
    }

    private static bool IsBuiltin(string name)
        => name is CodePlaceholderNames.Prefix
            or CodePlaceholderNames.Date
            or CodePlaceholderNames.Serial
            or CodePlaceholderNames.Check;
}
