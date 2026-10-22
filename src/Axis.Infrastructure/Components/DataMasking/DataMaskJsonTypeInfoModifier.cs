using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;

namespace Axis.Infrastructure.Components.DataMasking;

/// <summary>
/// System.Text.Json 类型信息修饰：序列化时对带 <see cref="DataMaskAttribute"/> 的 string 属性输出脱敏值。
/// </summary>
public static class DataMaskJsonTypeInfoModifier
{
    /// <summary>
    /// 注册到 <see cref="DefaultJsonTypeInfoResolver.Modifiers"/>。
    /// </summary>
    /// <param name="typeInfo">当前类型的 JSON 元数据。</param>
    public static void Modify(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        foreach (var property in typeInfo.Properties)
        {
            if (property.PropertyType != typeof(string))
                continue;

            var attr = ResolveAttribute(property);
            if (attr is null)
                continue;

            var originalGet = property.Get;
            if (originalGet is null)
                continue;

            // 仅改写出路径：读入仍是明文绑定，避免影响入参反序列化语义。
            property.Get = obj =>
            {
                var value = originalGet(obj) as string;
                return attr.Mask(value);
            };
        }
    }

    private static DataMaskAttribute? ResolveAttribute(JsonPropertyInfo property)
    {
        if (property.AttributeProvider is null)
            return null;

        foreach (var item in property.AttributeProvider.GetCustomAttributes(typeof(DataMaskAttribute), inherit: true))
            return (DataMaskAttribute)item;

        return null;
    }
}

/// <summary>
/// 导出行脱敏：浅拷贝对象后对带特性的 string 属性打码，避免改写业务内存对象。
/// </summary>
public static class DataMaskExportHelper
{
    private static readonly ConcurrentDictionary<Type, PropertyMaskPlan[]> PlanCache = new();

    /// <summary>
    /// 若类型含脱敏属性则返回打码后的新列表；否则原样返回（不复制）。
    /// </summary>
    /// <typeparam name="T">行模型。</typeparam>
    /// <param name="rows">原始行。</param>
    public static IList<T> ApplyForExport<T>(IList<T> rows) where T : class, new()
    {
        var plan = PlanCache.GetOrAdd(typeof(T), BuildPlan);
        if (plan.Length == 0)
            return rows;

        var result = new List<T>(rows.Count);
        foreach (var row in rows)
            result.Add(CloneAndMask(row, plan));
        return result;
    }

    private static PropertyMaskPlan[] BuildPlan(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p is { CanRead: true, CanWrite: true, PropertyType: not null }
                        && p.PropertyType == typeof(string)
                        && p.GetIndexParameters().Length == 0)
            .Select(p =>
            {
                var attr = p.GetCustomAttribute<DataMaskAttribute>(inherit: true);
                return attr is null ? null : new PropertyMaskPlan(p, attr);
            })
            .Where(p => p is not null)
            .Cast<PropertyMaskPlan>()
            .ToArray();
    }

    private static T CloneAndMask<T>(T source, PropertyMaskPlan[] maskPlans) where T : class, new()
    {
        var dest = new T();
        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            if (!prop.CanRead || !prop.CanWrite || prop.GetIndexParameters().Length > 0)
                continue;

            var value = prop.GetValue(source);
            dest.GetType().GetProperty(prop.Name)!.SetValue(dest, value);
        }

        foreach (var plan in maskPlans)
        {
            var raw = plan.Property.GetValue(source) as string;
            plan.Property.SetValue(dest, plan.Attribute.Mask(raw));
        }

        return dest;
    }

    private sealed record PropertyMaskPlan(PropertyInfo Property, DataMaskAttribute Attribute);
}
