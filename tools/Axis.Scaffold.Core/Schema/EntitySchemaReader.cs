using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Axis.Scaffold.Schema;

/// <summary>
/// 用 MetadataLoadContext 只读反射实体，避免加载 Domain→Infrastructure 时触发模块初始化器。
/// </summary>
public static class EntitySchemaReader
{
    public static (IReadOnlyList<ScaffoldField> Fields, string? TableName) Read(string repoRoot, string entityName)
    {
        var domainDll = FindDll(repoRoot, "Axis.Domain");
        var infraDll = FindDll(repoRoot, "Axis.Infrastructure");
        var domainDir = Path.GetDirectoryName(domainDll)!;
        var infraDir = Path.GetDirectoryName(infraDll)!;

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in new[] { domainDir, infraDir, RuntimeEnvironment.GetRuntimeDirectory() })
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
                paths.Add(dll);
        }

        // Infrastructure 依赖包可能只在 NuGet 缓存；补扫常见依赖目录（同级 deps）。
        TryAddNuGetFallback(paths, "SqlSugarCoreNoDrive", "5.1.4.216");

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);

        var domain = mlc.LoadFromAssemblyPath(domainDll);
        var type = domain.GetTypes()
                       .FirstOrDefault(t => t.Name == entityName)
                   ?? throw new InvalidOperationException(
                       $"在 Axis.Domain 中未找到实体类型 {entityName}。请确认已编译：dotnet build src/Axis.Domain");

        string? tableName = null;
        foreach (var attr in type.GetCustomAttributesData())
        {
            if (!IsSugarTableAttribute(attr.AttributeType.Name)) continue;
            if (attr.ConstructorArguments.Count > 0)
                tableName = attr.ConstructorArguments[0].Value?.ToString();
            break;
        }

        var fields = new List<ScaffoldField>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            if (FieldCatalog.SkipNames.Contains(prop.Name)) continue;
            if (prop.GetIndexParameters().Length > 0) continue;

            var (cs, ts, isStatus, nullable, maxLen, label) = InspectProperty(prop);
            fields.Add(new ScaffoldField
            {
                Name = prop.Name,
                CamelName = FieldCatalog.ToCamel(prop.Name),
                CsType = cs,
                TsType = ts,
                Label = label,
                MaxLength = maxLen,
                IsNullable = nullable,
                IsRequired = !nullable && (cs == "string" || isStatus),
                DefaultCsLiteral = GuessDefault(prop.Name, cs, isStatus, nullable),
                IsEnumStatus = isStatus
            });
        }

        if (fields.Count == 0)
            throw new InvalidOperationException($"实体 {entityName} 没有可生成的业务属性。");

        return (fields, tableName);
    }

    private static (string Cs, string Ts, bool IsStatus, bool Nullable, int? MaxLen, string Label)
        InspectProperty(PropertyInfo prop)
    {
        var propType = prop.PropertyType;
        var nullable = false;

        if (propType.IsGenericType && propType.GetGenericTypeDefinition().FullName == typeof(Nullable<>).FullName)
        {
            nullable = true;
            propType = propType.GetGenericArguments()[0];
        }

        var typeName = propType.Name;
        var fullName = propType.FullName ?? typeName;

        if (typeName == "String" || fullName == "System.String")
        {
            // MetadataLoadContext 下 NullabilityInfoContext 不可用，按引用类型 + 无 Required 近似。
            var required = prop.GetCustomAttributesData()
                .Any(a => a.AttributeType.Name is "RequiredAttribute");
            nullable = !required;
            typeName = "string";
        }

        var isStatus = typeName == "StatusEnum"
                       || (string.Equals(prop.Name, "Status", StringComparison.OrdinalIgnoreCase)
                           && typeName is "Int32" or "Int16" or "Int64");

        string cs;
        string ts;
        if (isStatus)
        {
            cs = "StatusEnum";
            ts = "number";
        }
        else
        {
            (cs, ts) = typeName switch
            {
                "String" or "string" => ("string", "string"),
                "Int16" => ("short", "number"),
                "Int32" => ("int", "number"),
                "Int64" => ("long", "string"),
                "Single" => ("float", "number"),
                "Double" => ("double", "number"),
                "Decimal" => ("decimal", "number"),
                "Boolean" => ("bool", "boolean"),
                "Guid" => ("Guid", "string"),
                "DateTime" => ("DateTime", "string"),
                "DateOnly" => ("DateOnly", "string"),
                "TimeSpan" => ("TimeSpan", "string"),
                "StatusEnum" => ("StatusEnum", "number"),
                _ when propType.IsEnum => (typeName, "number"),
                _ => ("string", "string")
            };
        }

        int? maxLen = null;
        string? label = null;
        foreach (var attr in prop.GetCustomAttributesData())
        {
            var name = attr.AttributeType.Name;
            if (name is "MaxLengthAttribute" or "StringLengthAttribute")
            {
                if (attr.ConstructorArguments.Count > 0
                    && attr.ConstructorArguments[0].Value is int len
                    && len > 0)
                    maxLen = len;
            }

            if (name == "SugarColumnAttribute")
            {
                foreach (var named in attr.NamedArguments)
                {
                    if (named.MemberName == "ColumnDescription")
                        label = named.TypedValue.Value?.ToString();
                    if (named.MemberName == "IsNullable" && named.TypedValue.Value is true)
                        nullable = true;
                    if (named.MemberName == "Length" && named.TypedValue.Value is int l && l > 0)
                        maxLen ??= l;
                }
            }

            if (name == "RequiredAttribute")
                nullable = false;
        }

        label = string.IsNullOrWhiteSpace(label) ? prop.Name : label!;
        return (cs, ts, isStatus, nullable, maxLen, label);
    }

    private static string? GuessDefault(string name, string cs, bool isStatus, bool nullable)
    {
        if (nullable) return null;
        if (isStatus) return "StatusEnum.Enable";
        if (string.Equals(name, "OrderNo", StringComparison.OrdinalIgnoreCase)) return "100";
        return null;
    }

    /// <summary>
    /// 列出 Domain 程序集中带 SugarTable 的实体类型名（只读反射）。
    /// </summary>
    public static IReadOnlyList<string> ListEntityNames(string repoRoot)
    {
        var domainDll = FindDll(repoRoot, "Axis.Domain");
        var infraDll = FindDll(repoRoot, "Axis.Infrastructure");
        var domainDir = Path.GetDirectoryName(domainDll)!;
        var infraDir = Path.GetDirectoryName(infraDll)!;

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in new[] { domainDir, infraDir, RuntimeEnvironment.GetRuntimeDirectory() })
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var dll in Directory.EnumerateFiles(dir, "*.dll"))
                paths.Add(dll);
        }

        TryAddNuGetFallback(paths, "SqlSugarCoreNoDrive", "5.1.4.216");

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);
        var domain = mlc.LoadFromAssemblyPath(domainDll);

        return SafeGetTypes(domain)
            .Where(t => t is { IsClass: true, IsAbstract: false, IsPublic: true })
            .Where(t => t.GetCustomAttributesData().Any(a => IsSugarTableAttribute(a.AttributeType.Name)))
            .Select(t => t.Name)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>
    /// SqlSugar 特性类名为 SugarTable（无 Attribute 后缀）；兼容 SugarTableAttribute。
    /// </summary>
    private static bool IsSugarTableAttribute(string? typeName)
        => typeName is "SugarTable" or "SugarTableAttribute";

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }

    private static string FindDll(string repoRoot, string projectName)
    {
        var candidates = new[]
        {
            Path.Combine(repoRoot, "src", projectName, "bin", "Debug", "net10.0", $"{projectName}.dll"),
            Path.Combine(repoRoot, "src", projectName, "bin", "Release", "net10.0", $"{projectName}.dll")
        };
        return candidates.FirstOrDefault(File.Exists)
               ?? throw new FileNotFoundException(
                   $"未找到 {projectName}.dll，请先执行：dotnet build src/{projectName}");
    }

    private static void TryAddNuGetFallback(HashSet<string> paths, string packageId, string version)
    {
        var nuget = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nuget", "packages", packageId.ToLowerInvariant(), version, "lib");
        if (!Directory.Exists(nuget)) return;
        foreach (var dll in Directory.EnumerateFiles(nuget, "*.dll", SearchOption.AllDirectories))
            paths.Add(dll);
    }
}
