using System.Reflection;

namespace Axis.Infrastructure.Global;

/// <summary>
/// 程序集类型
/// </summary>
public static class GlobalType
{
    private const string CoreAssembly = "Axis.Core";
    private const string DomainAssembly = "Axis.Domain";
    private const string ApplicationAssembly = "Axis.Application";
    private const string InfrastructureAssembly = "Axis.Infrastructure";

    public static readonly List<Type> CoreTypes;
    public static readonly List<Type> DomainTypes;
    public static readonly List<Type> ApplicationTypes;
    public static readonly List<Type> InfrastructureTypes;
        

    static GlobalType()
    {
        CoreTypes =  LoadAssemblyTypes(CoreAssembly);
        DomainTypes = LoadAssemblyTypes(DomainAssembly);
        ApplicationTypes = LoadAssemblyTypes(ApplicationAssembly);
        InfrastructureTypes = LoadAssemblyTypes(InfrastructureAssembly);
    }

    /// <summary>
    /// 加载程序集
    /// </summary>
    /// <param name="dllName">程序集名称</param>
    /// <returns></returns>
    private static List<Type> LoadAssemblyTypes(string dllName)
    {
        var assembly = LoadAssembly(dllName + ".dll");
        return assembly.GetTypes().Where(u => u.IsPublic).ToList();
    }

    /// <summary>
    /// 加载程序集
    /// </summary>
    /// <param name="dllName">程序集名称</param>
    private static Assembly LoadAssembly(string dllName)
    {
        var basePath = AppContext.BaseDirectory;
        var dllFile = Path.Combine(basePath, dllName);
        return !File.Exists(dllFile) ? throw new Exception($"{dllName} 文件未生成, 编译项目成功后重试！") : Assembly.LoadFrom(dllFile);
    }
}
