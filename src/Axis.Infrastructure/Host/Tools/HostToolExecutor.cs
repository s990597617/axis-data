using System.Reflection;

namespace Axis.Infrastructure.Host.Tools;

/// <summary>
/// Axis 宿主命令行工具执行器。
/// </summary>
public static class HostToolExecutor
{
    /// <summary>
    /// 工具缓存，避免重复反射扫描。
    /// </summary>
    private static readonly Lazy<IReadOnlyList<HostToolDescriptor>> ToolDescriptors = new(LoadTools);

    /// <summary>
    /// 尝试执行命令行工具。
    /// </summary>
    /// <param name="args">命令行参数。</param>
    /// <returns>如果成功识别并执行了工具命令，则返回 true；否则返回 false。</returns>
    public static bool TryExecute(string[] args)
    {
        if (args.Length == 0)
            return false;

        var command = args[0];

        if (IsHelpCommand(command))
        {
            PrintHelp();
            return true;
        }

        var descriptor = ToolDescriptors.Value.FirstOrDefault(x =>
            string.Equals(x.Name, command, StringComparison.OrdinalIgnoreCase));

        if (descriptor == null)
            return false;

        var tool = CreateTool(descriptor.ToolType);
        tool.Execute(args.Skip(1).ToArray());

        return true;
    }

    /// <summary>
    /// 加载当前程序集中的宿主工具。
    /// </summary>
    private static IReadOnlyList<HostToolDescriptor> LoadTools()
    {
        var assembly = typeof(HostToolExecutor).Assembly;

        var descriptors = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                && typeof(IHostTool).IsAssignableFrom(type))
            .Select(type => new
            {
                Type = type,
                Attribute = type.GetCustomAttribute<HostToolAttribute>()
            })
            .Where(x => x.Attribute != null)
            .Select(x => new HostToolDescriptor(
                x.Attribute!.Name,
                x.Attribute.Description,
                x.Type))
            .ToList();

        ValidateTools(descriptors);

        return descriptors;
    }

    /// <summary>
    /// 创建工具实例。
    /// </summary>
    private static IHostTool CreateTool(Type toolType)
    {
        if (Activator.CreateInstance(toolType) is not IHostTool tool)
        {
            throw new InvalidOperationException($"无法创建宿主工具实例：{toolType.FullName}");
        }

        return tool;
    }

    /// <summary>
    /// 校验工具配置。
    /// </summary>
    private static void ValidateTools(IReadOnlyList<HostToolDescriptor> descriptors)
    {
        var duplicateGroups = descriptors
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .ToList();

        if (duplicateGroups.Count == 0)
            return;

        var duplicateNames = string.Join(", ", duplicateGroups.Select(x => x.Key));

        throw new InvalidOperationException($"存在重复的宿主工具命令名称：{duplicateNames}");
    }

    /// <summary>
    /// 判断是否为帮助命令。
    /// </summary>
    private static bool IsHelpCommand(string command)
    {
        return string.Equals(command, "help", StringComparison.OrdinalIgnoreCase)
               || string.Equals(command, "--help", StringComparison.OrdinalIgnoreCase)
               || string.Equals(command, "-h", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 打印帮助信息。
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine("===== Axis Host Tools =====");
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project Axis.Host.Api -- <command>");
        Console.WriteLine();

        Console.WriteLine("Commands:");

        foreach (var tool in ToolDescriptors.Value.OrderBy(x => x.Name))
        {
            Console.WriteLine($"  {tool.Name,-20} {tool.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  dotnet run --project Axis.Host.Api -- gen-sm2");
        Console.WriteLine("===========================");
    }

    /// <summary>
    /// 宿主工具描述信息。
    /// </summary>
    private sealed record HostToolDescriptor(
        string Name,
        string Description,
        Type ToolType);
}