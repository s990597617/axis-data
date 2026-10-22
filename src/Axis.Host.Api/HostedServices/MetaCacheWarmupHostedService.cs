using Axis.Domain.Data.System.Cache;

namespace Axis.Host.Api.HostedServices;

/// <summary>
/// 启动后预热启用中的字典 / 参数配置缓存。
/// </summary>
/// <remarks>
/// 须在数据库初始化之后执行；失败只记日志，不阻断宿主启动。
/// </remarks>
public sealed class MetaCacheWarmupHostedService(
    IServiceProvider services,
    ILogger<MetaCacheWarmupHostedService> logger)
    : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var dictCache = scope.ServiceProvider.GetRequiredService<ISysDictCache>();
            var configCache = scope.ServiceProvider.GetRequiredService<ISysConfigCache>();

            var dictCount = await dictCache.WarmupAsync(cancellationToken);
            var configCount = await configCache.WarmupAsync(cancellationToken);

            logger.LogInformation(
                "元数据缓存预热完成：字典类型 {DictCount}，参数配置 {ConfigCount}",
                dictCount,
                configCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "元数据缓存预热失败（已忽略，不影响启动）");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
