#nullable enable
using System.Collections.ObjectModel;
using System.Reflection;

namespace Axis.Infrastructure.Permissions;

/// <summary>
/// 权限码目录：扫描已加载程序集中带元数据特性的权限常量。
/// </summary>
/// <remarks>
/// 不直接引用 Domain 类型，避免 Infrastructure ↔ Domain 循环依赖。
/// 约定：静态权限常量根类型的嵌套模块类标注 <see cref="PermissionModuleAttribute"/>，
/// 字符串常量标注 <see cref="PermissionActionAttribute"/>。
/// 权限码本身仍以编译期常量为唯一真相；本目录仅供管理端选择与校验。
/// </remarks>
public static class PermissionCatalog
{
    private static readonly Lazy<IReadOnlyList<PermissionDescriptor>> LazyAll = new(Scan);

    /// <summary>
    /// 全部已登记权限码（按模块、动作排序）。
    /// </summary>
    public static IReadOnlyList<PermissionDescriptor> All => LazyAll.Value;

    /// <summary>
    /// 判断权限码是否在目录中。
    /// </summary>
    /// <param name="code">待校验权限码。</param>
    /// <returns>已登记为 true。</returns>
    public static bool Contains(string? code)
        => !string.IsNullOrWhiteSpace(code)
           && All.Any(item => string.Equals(item.Code, code, StringComparison.Ordinal));

    /// <summary>
    /// 按权限码查找描述；未登记时返回 null。
    /// </summary>
    /// <param name="code">权限码。</param>
    /// <returns>描述或 null。</returns>
    public static PermissionDescriptor? Find(string? code)
        => string.IsNullOrWhiteSpace(code)
            ? null
            : All.FirstOrDefault(item => string.Equals(item.Code, code, StringComparison.Ordinal));

    private static IReadOnlyList<PermissionDescriptor> Scan()
    {
        var list = new List<PermissionDescriptor>();

        foreach (var root in EnumeratePermissionRoots())
        {
            foreach (var moduleType in root.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
            {
                var moduleAttr = moduleType.GetCustomAttribute<PermissionModuleAttribute>();
                if (moduleAttr is null) continue;

                foreach (var field in moduleType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                {
                    if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                        continue;

                    var actionAttr = field.GetCustomAttribute<PermissionActionAttribute>();
                    if (actionAttr is null) continue;

                    var code = field.GetRawConstantValue() as string;
                    if (string.IsNullOrWhiteSpace(code)) continue;

                    list.Add(new PermissionDescriptor(
                        code,
                        moduleAttr.Key,
                        moduleAttr.Name,
                        actionAttr.Name,
                        moduleAttr.Order,
                        actionAttr.Order));
                }
            }
        }

        var ordered = list
            .OrderBy(item => item.ModuleOrder)
            .ThenBy(item => item.Module)
            .ThenBy(item => item.ActionOrder)
            .ThenBy(item => item.Code)
            .ToList();

        return new ReadOnlyCollection<PermissionDescriptor>(ordered);
    }

    /// <summary>
    /// 查找「含 PermissionModuleAttribute 嵌套类」的静态根类型（如 SystemPermissionCodes）。
    /// </summary>
    private static IEnumerable<Type> EnumeratePermissionRoots()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic) continue;

            Type[] types;
            try
            {
                types = assembly.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(item => item is not null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                // C# static class → abstract + sealed
                if (!type.IsClass || !type.IsAbstract || !type.IsSealed) continue;

                var hasModule = type.GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
                    .Any(nested => nested.GetCustomAttribute<PermissionModuleAttribute>() is not null);
                if (!hasModule) continue;

                yield return type;
            }
        }
    }
}
