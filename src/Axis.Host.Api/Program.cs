using Axis.Host.Api.Health;
using Axis.Host.Api.HostedServices;
using Axis.Infrastructure.Host;

var builder = WebApplication.CreateBuilder(args);

// 额外加载 Config 文件夹下的所有 json 配置文件（按文件名排序，后加载的会覆盖前面的键）
var configFolder = Path.Combine(builder.Environment.ContentRootPath, "Config");
if (Directory.Exists(configFolder))
{
    foreach (var file in Directory.GetFiles(configFolder, "*.json").OrderBy(f => f))
    {
        builder.Configuration.AddJsonFile(file, optional: true, reloadOnChange: true);
    }
}

builder.Run(
    args,
    services =>
    {
        // SignalR 须在 DI 扫描前注册，覆盖 NullUserNotifyPusher
        services.AddAxisSignalR(builder.Configuration);
        // 在数据库初始化 HostedService 之后注册，确保启动时已建表/种子
        services.AddAxisHostedServices(builder.Configuration);
        services.AddAxisHealthChecks(builder.Configuration);
    },
    _ => { },
    app =>
    {
        app.MapAxisSignalRHubs(builder.Configuration);
        app.MapAxisHealthChecks();
    });
