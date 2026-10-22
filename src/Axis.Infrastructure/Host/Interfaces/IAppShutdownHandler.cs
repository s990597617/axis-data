namespace Axis.Infrastructure.Host.Interfaces;

/// <summary>
/// 应用关闭处理接口
/// </summary>
public interface IAppShutdownHandler
{
    /// <summary>
    /// 处理关闭
    /// </summary>
    /// <returns></returns>
    Task ShutdownAsync();
}