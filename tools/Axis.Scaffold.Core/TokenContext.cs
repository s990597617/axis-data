using System.Text;
using System.Text.RegularExpressions;

namespace Axis.Scaffold;

/// <summary>
/// 脚手架令牌上下文：由 CLI 参数计算，供路径与内容替换。
/// </summary>
public sealed class TokenContext
{
    private static readonly Regex IdentifierRegex = new("^[A-Z][A-Za-z0-9]*$", RegexOptions.Compiled);
    private static readonly Regex RouteKeyRegex = new("^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public required string Module { get; init; }
    public required string ModuleCamel { get; init; }
    public required string ModuleKebab { get; init; }
    public required string CnName { get; init; }
    public required string Area { get; init; }
    public required string AreaLower { get; init; }
    public required string RouteKey { get; init; }
    public required string Entity { get; init; }
    public required string Table { get; init; }
    public required string RoutePrefix { get; init; }
    public required string PermPrefix { get; init; }
    public required string PermArea { get; init; }
    public required string ApiGroup { get; init; }
    public required string PermissionCodesType { get; init; }
    public required string PermissionCodesVar { get; init; }
    public bool IsSystemArea => string.Equals(Area, "System", StringComparison.Ordinal);

    public static TokenContext Create(
        string module,
        string cnName,
        string area,
        string? routeKey,
        string? entity,
        string? table)
    {
        if (!IdentifierRegex.IsMatch(module))
            throw new ArgumentException("Module 必须是 PascalCase 标识符，如 Product。", nameof(module));
        if (!IdentifierRegex.IsMatch(area))
            throw new ArgumentException("Area 必须是 PascalCase 标识符，如 Demo。", nameof(area));
        if (string.IsNullOrWhiteSpace(cnName))
            throw new ArgumentException("中文名 --cn 不能为空。", nameof(cnName));

        var kebab = ToKebab(module);
        var resolvedRouteKey = string.IsNullOrWhiteSpace(routeKey) ? kebab : routeKey.Trim();
        if (!RouteKeyRegex.IsMatch(resolvedRouteKey))
            throw new ArgumentException("route-key 须为小写字母开头的 kebab，如 product。", nameof(routeKey));

        var areaLower = area.ToLowerInvariant();
        var permArea = string.Equals(area, "System", StringComparison.Ordinal) ? "sys" : areaLower;
        var resolvedEntity = string.IsNullOrWhiteSpace(entity) ? $"Sys{module}" : entity.Trim();
        if (!IdentifierRegex.IsMatch(resolvedEntity))
            throw new ArgumentException("Entity 必须是 PascalCase 标识符。", nameof(entity));

        var resolvedTable = string.IsNullOrWhiteSpace(table)
            ? $"{permArea}_{resolvedRouteKey.Replace('-', '_')}"
            : table.Trim();

        return new TokenContext
        {
            Module = module,
            ModuleCamel = ToCamel(module),
            ModuleKebab = kebab,
            CnName = cnName.Trim(),
            Area = area,
            AreaLower = areaLower,
            RouteKey = resolvedRouteKey,
            Entity = resolvedEntity,
            Table = resolvedTable,
            RoutePrefix = $"/{permArea}/{resolvedRouteKey}",
            PermPrefix = $"{permArea}:{resolvedRouteKey}",
            PermArea = permArea,
            ApiGroup = ResolveApiGroup(area),
            PermissionCodesType = string.Equals(area, "System", StringComparison.Ordinal)
                ? "SystemPermissionCodes"
                : $"{area}PermissionCodes",
            PermissionCodesVar = string.Equals(area, "System", StringComparison.Ordinal)
                ? "systemPermissionCodes"
                : $"{ToCamel(area)}PermissionCodes"
        };
    }

    public string Apply(string input)
    {
        // 长令牌优先，避免短令牌误伤。
        var pairs = new (string Token, string Value)[]
        {
            ("__PermissionCodesType__", PermissionCodesType),
            ("__permissionCodesVar__", PermissionCodesVar),
            ("__module_kebab__", ModuleKebab),
            ("__RoutePrefix__", RoutePrefix),
            ("__PermPrefix__", PermPrefix),
            ("__ApiGroup__", ApiGroup),
            ("__PermArea__", PermArea),
            ("__route_key__", RouteKey),
            ("__CnName__", CnName),
            ("__Entity__", Entity),
            ("__Table__", Table),
            ("__Module__", Module),
            ("__module__", ModuleCamel),
            ("__Area__", Area),
            ("__area__", AreaLower)
        };

        var result = input;
        foreach (var (token, value) in pairs)
            result = result.Replace(token, value, StringComparison.Ordinal);
        return result;
    }

    private static readonly HashSet<string> MesAreas = new(StringComparer.Ordinal)
    {
        "Mdm", "Factory", "Plan", "Exec", "Quality", "Wip", "Inv", "Andon", "Equip"
    };

    private static string ResolveApiGroup(string area)
    {
        if (string.Equals(area, "System", StringComparison.Ordinal))
            return "AdminSystemModule";
        if (MesAreas.Contains(area))
            return "MesModule";
        return "AdminBaseModule";
    }

    private static string ToCamel(string pascal)
        => string.IsNullOrEmpty(pascal)
            ? pascal
            : char.ToLowerInvariant(pascal[0]) + pascal[1..];

    private static string ToKebab(string pascal)
    {
        var sb = new StringBuilder(pascal.Length + 4);
        for (var i = 0; i < pascal.Length; i++)
        {
            var c = pascal[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}
