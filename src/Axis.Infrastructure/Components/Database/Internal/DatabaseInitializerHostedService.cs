namespace Axis.Infrastructure.Components.Database.Internal;

/// <summary>
/// 应用启动后执行 SqlSugar 的建库 / 建表 / 种子数据初始化。
/// 把原先在 AddSqlSugarSetup 里立即执行的副作用挪到 Host 构建完成之后，避免依赖未就绪。
/// </summary>
internal sealed class DatabaseInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DatabaseInitializerHostedService> _logger;

    public DatabaseInitializerHostedService(IServiceProvider services, ILogger<DatabaseInitializerHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            SqlSugarServiceCollectionExtensions.InitializeDatabase(_services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
