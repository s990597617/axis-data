using System.Reflection;

namespace Axis.Infrastructure.Global;

/// <summary>
/// 统一的程序集加载入口。
/// 用于解析 <c>BootstrapAssembly</c> 各节（| 分隔的程序集名称列表），
/// 对加载失败 / 类型反射失败做容错，避免一个坏程序集导致整个启动崩溃。
/// </summary>
public static class GlobalAssemblyLoader
{
    /// <summary>
    /// 解析管道分隔的程序集名称列表并加载。
    /// </summary>
    /// <param name="raw">管道分隔字符串，例如 <c>"A|B|C"</c></param>
    /// <param name="logger">可选日志记录，单个程序集加载失败写 Warning 并跳过</param>
    /// <returns>加载成功的程序集数组（去重）</returns>
    public static Assembly[] Load(string raw, ILogger logger = null)
    {
        if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<Assembly>();

        return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name =>
            {
                try { return Assembly.Load(name); }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "加载程序集失败: {Assembly}", name);
                    return null;
                }
            })
            .Where(a => a is not null)
            .ToArray();
    }

    /// <summary>
    /// 加载并展平所有类型，容忍 <see cref="ReflectionTypeLoadException"/>。
    /// </summary>
    public static Type[] LoadTypes(string raw, ILogger logger = null)
    {
        var assemblies = Load(raw, logger);
        return FlattenTypes(assemblies);
    }

    /// <summary>
    /// 扫描宿主输出目录下所有 <c>Axis.*.dll</c>，连同当前 AppDomain 已加载的 Axis 前缀程序集合并返回（按名称去重）。
    /// 用于替代显式的管道分隔配置列表，避免新增项目时忘记同步配置导致 DI 扫描遗漏。
    /// </summary>
    private static Assembly[] LoadAxisAssemblies(ILogger logger = null)
    {
        var dict = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && IsAxisAssembly(a.GetName().Name))
            .GroupBy(a => a.GetName().Name!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var baseDir = AppContext.BaseDirectory;
        if (!Directory.Exists(baseDir)) return dict.Values.ToArray();

        foreach (var dll in Directory.EnumerateFiles(baseDir, "Axis.*.dll", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileNameWithoutExtension(dll);
            if (!IsAxisAssembly(name) || dict.ContainsKey(name)) continue;
            try { dict[name] = Assembly.Load(new AssemblyName(name)); }
            catch (Exception ex) { logger?.LogWarning(ex, "加载程序集失败: {Assembly}", name); }
        }

        return dict.Values.ToArray();
    }

    /// <summary>
    /// 扫描所有 Axis 前缀程序集并展平类型，供 DI 自动注入等场景使用。
    /// </summary>
    public static Type[] LoadAxisTypes(ILogger logger = null)
        => FlattenTypes(LoadAxisAssemblies(logger));

    private static bool IsAxisAssembly(string name)
        => !string.IsNullOrEmpty(name)
           && name.StartsWith("Axis.", StringComparison.Ordinal);

    private static Type[] FlattenTypes(IEnumerable<Assembly> assemblies)
        => assemblies.SelectMany(a =>
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
        }).ToArray();
}
