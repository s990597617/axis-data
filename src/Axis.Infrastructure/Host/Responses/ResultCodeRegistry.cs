using System.Collections.Concurrent;
using System.Reflection;

namespace Axis.Infrastructure.Host.Responses;

/// <summary>
/// 业务码注册表。
/// <para>
/// 解决的问题：业务方在各模块里随手 <c>throw new BusinessException(40567, "xx")</c>，
/// 时间一长就会出现没人能讲清楚的"野码"。本注册表提供两条护栏：
/// </para>
/// <list type="number">
/// <item><description><b>格式校验</b>：必须是 5 位数，前 3 位须为合法 HTTP 状态码段（2xx/4xx/5xx）。</description></item>
/// <item><description><b>白名单登记</b>：框架内置码 + 业务方启动时显式 <see cref="Register"/> 的码构成白名单。未登记的码在 Dev/Staging 下会被记录 <c>Warning</c> 日志，不会阻塞业务。</description></item>
/// </list>
/// <para>
/// 生产环境默认只做格式校验，不强制白名单（避免因为漏登记导致线上故障）。
/// </para>
/// </summary>
public static class ResultCodeRegistry
{
    private static readonly ConcurrentDictionary<int, string> KnownCodes = new();

    static ResultCodeRegistry()
    {
        // 自注册所有 ResultCodes 公共常量
        foreach (var field in typeof(ResultCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(int) || !field.IsLiteral) continue;
            KnownCodes.TryAdd((int)field.GetRawConstantValue()!, $"ResultCodes.{field.Name}");
        }
    }

    /// <summary>
    /// 注册业务模块自定义码。推荐在应用启动时集中调用。
    /// </summary>
    /// <example>
    /// <code>
    /// ResultCodeRegistry.Register(40201, nameof(UserResultCodes.UserNotActivated));
    /// ResultCodeRegistry.Register(40202, nameof(UserResultCodes.UserLocked));
    /// </code>
    /// </example>
    public static void Register(int code, string name)
    {
        if (!IsValidFormat(code))
            throw new ArgumentException($"业务码 {code} 格式非法：必须为 5 位数且前 3 位是合法 HTTP 状态码段。", nameof(code));

        KnownCodes[code] = name;
    }

    /// <summary>
    /// 批量登记反射自某个常量类的所有 <c>public const int</c> 字段。
    /// </summary>
    public static void RegisterFromClass(Type constantsType)
    {
        foreach (var field in constantsType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType != typeof(int) || !field.IsLiteral) continue;
            var value = (int)field.GetRawConstantValue()!;
            Register(value, $"{constantsType.Name}.{field.Name}");
        }
    }

    /// <summary>
    /// 是否已登记。
    /// </summary>
    public static bool IsKnown(int code) => KnownCodes.ContainsKey(code);

    /// <summary>
    /// 校验业务码格式。
    /// 规则：5 位数字 + 前 3 位落在 [200,299] / [400,499] / [500,599]。
    /// </summary>
    public static bool IsValidFormat(int code)
    {
        if (code is < 10000 or > 99999) return false;
        var prefix = code / 100;
        return prefix is (>= 200 and < 300) or (>= 400 and < 600);
    }

    /// <summary>
    /// 校验业务码并按需日志告警（调用方通常是异常处理器）。
    /// </summary>
    /// <returns>true 表示格式合法（可继续使用），false 表示格式非法（建议回退到 <see cref="ResultCodes.ServerError"/>）</returns>
    public static bool Validate(int code, ILogger logger, bool strict = false)
    {
        if (!IsValidFormat(code))
        {
            logger?.LogError("检测到非法业务码 {Code}：不符合 5 位数 + HTTP 前缀规范，将回退到 ServerError。", code);
            return false;
        }

        if (!IsKnown(code))
        {
            if (strict)
                logger?.LogError("检测到未登记业务码 {Code}：未在 ResultCodeRegistry 中注册。", code);
            else
                logger?.LogWarning("检测到未登记业务码 {Code}：建议在启动时调用 ResultCodeRegistry.Register 登记。", code);
        }

        return true;
    }

    /// <summary>
    /// 返回当前所有已登记的码（只读快照，用于调试 / 暴露诊断端点）。
    /// </summary>
    public static IReadOnlyDictionary<int, string> GetAll()
        => KnownCodes.ToArray().ToDictionary(kv => kv.Key, kv => kv.Value);
}
