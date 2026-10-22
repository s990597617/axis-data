namespace Axis.Infrastructure.Host.Tools;

/// <summary>
/// Axis 宿主命令行工具接口。
/// </summary>
public interface IHostTool
{
    /// <summary>
    /// 执行工具命令。
    /// </summary>
    /// <param name="args">命令参数，不包含工具名称本身。</param>
    void Execute(string[] args);
}