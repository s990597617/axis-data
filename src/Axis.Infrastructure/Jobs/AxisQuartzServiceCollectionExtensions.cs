using Microsoft.Extensions.Configuration;
using Quartz;

namespace Axis.Infrastructure.Jobs;

/// <summary>
/// Quartz 调度注册。
/// </summary>
public static class AxisQuartzServiceCollectionExtensions
{
    /// <summary>
    /// 注册 Quartz 调度器（内存存储为主；可选持久化）。
    /// </summary>
    public static IServiceCollection AddAxisQuartz(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AxisQuartzOptions>(configuration.GetSection(AxisQuartzOptions.SectionName));
        var options = configuration.GetSection(AxisQuartzOptions.SectionName).Get<AxisQuartzOptions>()
                      ?? new AxisQuartzOptions();

        if (!options.Enabled)
            return services;

        services.AddQuartz(q =>
        {
            q.SchedulerId = "AUTO";
            q.SchedulerName = options.InstanceName;

            if (options.UsePersistentStore)
            {
                var connectionString = ResolveNpgsqlConnectionString(options);
                q.UsePersistentStore(store =>
                {
                    store.UseProperties = true;
                    store.UseClustering();
                    store.UsePostgres(postgres =>
                    {
                        postgres.ConnectionString = connectionString;
                        postgres.TablePrefix = options.TablePrefix;
                    });
                    store.UseSystemTextJsonSerializer();
                });
            }
            else
            {
                q.UseInMemoryStore();
            }
        });

        services.AddQuartzHostedService(host =>
        {
            host.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static string ResolveNpgsqlConnectionString(AxisQuartzOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            return options.ConnectionString;

        var raw = GlobalContext.BootstrapDb?.ConnectionConfigs?
            .FirstOrDefault()?.ConnectionString;
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("Quartz 持久化存储需要 ConnectionString 或 DbConnection 主库连接串。");

        return ToNpgsqlConnectionString(raw);
    }

    /// <summary>
    /// 将 SqlSugar 风格连接串尽量转换为 Npgsql 连接串。
    /// </summary>
    private static string ToNpgsqlConnectionString(string raw)
    {
        var map = raw.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0], parts => parts[1], StringComparer.OrdinalIgnoreCase);

        var host = Pick("Host", "Server");
        var port = Pick("Port");
        var database = Pick("Database");
        var username = Pick("Username", "UserName", "User Id", "UserId");
        var password = Pick("Password", "Pwd");

        return $"Host={host};Port={(string.IsNullOrWhiteSpace(port) ? "5432" : port)};Database={database};Username={username};Password={password}";

        string Pick(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (map.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return string.Empty;
        }
    }
}
