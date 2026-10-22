using Axis.Infrastructure.Components.FileStorage.Abstractions;

namespace Axis.Infrastructure.Components.FileStorage;

/// <summary>
/// 文件存储服务注册扩展
/// </summary>
public static class FileStorageServiceCollectionExtensions
{
    /// <summary>
    /// 注册文件存储服务。
    /// <para>默认使用本地磁盘存储，可通过泛型参数替换为 MinIO / OSS / S3 等实现。</para>
    /// </summary>
    /// <typeparam name="TImpl">文件存储实现类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="lifetime">生命周期，默认 Scoped</param>
    public static IServiceCollection AddFileStorage<TImpl>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TImpl : class, IFileStorageService
    {
        services.Add(new ServiceDescriptor(typeof(IFileStorageService), typeof(TImpl), lifetime));
        return services;
    }

    /// <summary>
    /// 注册本地磁盘文件存储（不入库，仅文件存取）
    /// </summary>
    public static IServiceCollection AddLocalFileStorage(this IServiceCollection services)
    {
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        return services;
    }

    /// <summary>
    /// 注册带持久化的本地磁盘文件存储（上传/删除自动同步数据库记录）。
    /// <para>
    /// 使用装饰器模式：实际文件操作由 <see cref="LocalFileStorageService"/> 完成，
    /// <see cref="PersistentFileStorageService"/> 负责在操作前后写入 / 更新数据库。
    /// </para>
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="providerName">存储提供者名称（记录到数据库以区分来源），默认 "local"</param>
    public static IServiceCollection AddLocalFileStorageWithPersistence(this IServiceCollection services,
        string providerName = "local")
    {
        // 注册底层实现（不对外暴露）
        services.AddScoped<LocalFileStorageService>();

        // 注册持久化装饰器作为 IFileStorageService
        services.AddScoped<IFileStorageService>(sp =>
        {
            var inner = sp.GetRequiredService<LocalFileStorageService>();
            var recordRepo = sp.GetRequiredService<IFileRecordRepository>();
            var logger = sp.GetRequiredService<ILogger<PersistentFileStorageService>>();
            return new PersistentFileStorageService(inner, recordRepo, logger, providerName);
        });

        return services;
    }

    /// <summary>
    /// 注册带持久化的自定义文件存储实现（泛型版，适用于 MinIO / OSS / S3 等）。
    /// </summary>
    /// <typeparam name="TImpl">文件存储实现类型</typeparam>
    /// <param name="services">服务集合</param>
    /// <param name="providerName">存储提供者名称</param>
    public static IServiceCollection AddFileStorageWithPersistence<TImpl>(this IServiceCollection services,
        string providerName = "custom")
        where TImpl : class, IFileStorageService
    {
        services.AddScoped<TImpl>();
        services.AddScoped<IFileStorageService>(sp =>
        {
            var inner = sp.GetRequiredService<TImpl>();
            var recordRepo = sp.GetRequiredService<IFileRecordRepository>();
            var logger = sp.GetRequiredService<ILogger<PersistentFileStorageService>>();
            return new PersistentFileStorageService(inner, recordRepo, logger, providerName);
        });

        return services;
    }
}
