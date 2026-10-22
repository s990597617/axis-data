using Mapster;
using SqlSugar;
using System.Reflection;
using Yitter.IdGenerator;
using System.Collections;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Http;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Components.Database.Permission;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Domain;

namespace Axis.Infrastructure.Components.Database;

/// <summary>
/// SqlSugar 服务拓展
/// </summary>
public static class SqlSugarServiceCollectionExtensions
{
    /// <summary>
    /// 注册 SqlSugar 相关服务。
    /// </summary>
    public static IServiceCollection AddSqlSugarSetup(this IServiceCollection services)
    {
        // 配置雪花 id 生成规则
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = (ushort)GlobalContext.BootstrapSystem.WorkId
        });
        StaticConfig.CustomSnowFlakeFunc = YitIdHelper.NextId;

        // Dynamic Linq 解析器
        StaticConfig.DynamicExpressionParserType = typeof(DynamicExpressionParser);
        StaticConfig.DynamicExpressionParsingConfig = new ParsingConfig
        {
            CustomTypeProvider = new SqlSugarTypeProvider()
        };

        // 读取配置并初始化每个连接（设置命名策略、可空性、默认行为）
        var dbOptions = GlobalContext.BootstrapDb;
        dbOptions.ConnectionConfigs.ForEach(SetDbConfig);

        // 注册 ISqlSugarClient 为单例，AOP 内延迟解析 scoped 服务以匹配请求生命周期
        services.AddSingleton<ISqlSugarClient>(sp =>
        {
            var accessor = sp.GetService<IHttpContextAccessor>();
            var logger = sp.GetService<ILoggerFactory>()?
                             .CreateLogger("Axis.Infrastructure.Components.Database.SqlSugar")
                         ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            var lifetime = sp.GetRequiredService<IHostApplicationLifetime>();

            SqlSugarFilter.Configure(accessor);

            return new SqlSugarScope(dbOptions.ConnectionConfigs.Adapt<List<ConnectionConfig>>(), db =>
            {
                foreach (var dbProvider in dbOptions.ConnectionConfigs.Select(config => db.GetConnectionScope(config.ConfigId)))
                {
                    SetDbAop(dbProvider, accessor, logger, lifetime);
                }
            });
        });

        services.AddScoped(typeof(BaseRepository<>)); // 仓储注册
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>)); // 开放泛型仓储：允许直接注入 IRepository&lt;TEntity&gt;
        services.AddScoped<IUnitOfWork, SqlSugarUnitOfWork>(); // 事务与工作单元注册

        // 建库 / 建表 / 种子数据走 IHostedService，在应用构建完成后自动执行
        services.AddHostedService<DatabaseInitializerHostedService>();
        return services;
    }

    /// <summary>
    /// 执行数据库建库 / 建表 / 种子数据初始化。
    /// </summary>
    public static void InitializeDatabase(IServiceProvider rootServices)
    {
        var tenant = (ITenant)rootServices.GetRequiredService<ISqlSugarClient>();
        var logger = rootServices.GetService<ILoggerFactory>()?.CreateLogger("Axis.Infrastructure.Database");

        // 程序集只扫描一次并缓存
        var sqlSugarTypes = LoadSqlSugarEntityTypes(logger);

        foreach (var config in GlobalContext.BootstrapDb.ConnectionConfigs)
        {
            InitDatabase(tenant, config, sqlSugarTypes, logger);
        }
    }

    /// <summary>
    /// 配置连接属性
    /// </summary>
    private static void SetDbConfig(AxisDbConnectionConfig config)
    {
        var nullabilityContext = new NullabilityInfoContext();
        var configureExternalServices = new ConfigureExternalServices
        {
            EntityNameService = (type, entity) =>
            {
                entity.IsDisabledDelete = true;

                if (!type.GetCustomAttributes<SugarTable>().Any())
                    return;
                if (config.DbSettings.EnableUnderLine && !entity.DbTableName.Contains('_'))
                    entity.DbTableName = UtilMethods.ToUnderLine(entity.DbTableName);
            },
            EntityService = (property, column) =>
            {
                if (!property.DeclaringType!.GetCustomAttributes<SugarTable>().Any())
                    return;
                // NullabilityInfoContext 线程不安全，外层已单连接复用
                if (nullabilityContext.Create(property).WriteState is NullabilityState.Nullable)
                    column.IsNullable = true;
                if (config.DbSettings.EnableUnderLine && !column.IsIgnore && !column.DbColumnName.Contains('_'))
                    column.DbColumnName = UtilMethods.ToUnderLine(column.DbColumnName);
            }
        };
        config.ConfigureExternalServices = configureExternalServices;
        config.InitKeyType = InitKeyType.Attribute;
        config.IsAutoCloseConnection = true;
        config.MoreSettings = new ConnMoreSettings
        {
            IsAutoRemoveDataCache = true,
            IsAutoDeleteQueryFilter = true,
            IsAutoUpdateQueryFilter = true,
            SqlServerCodeFirstNvarchar = true
        };

        // 若库类型是人大金仓则默认设置PG模式
        if (config.DbType == DbType.Kdbndp)
            config.MoreSettings.DatabaseModel = DbType.PostgreSQL; // 配置PG模式主要是兼容系统表差异

        // 若库类型是Oracle则默认主键名字和参数名字最大长度
        if (config.DbType == DbType.Oracle)
            config.MoreSettings.MaxParameterNameLength = 30;
    }

    /// <summary>
    /// 配置 SqlSugar AOP：SQL 日志、超时告警、数据权限过滤器，以及 Insert/Update 主键与审计字段自动赋值。
    /// </summary>
    private static void SetDbAop( SqlSugarScopeProvider db, IHttpContextAccessor accessor, ILogger logger, IHostApplicationLifetime lifetime)
    {
        var stopping = false;
        lifetime.ApplicationStopping.Register(() => { stopping = true; });

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
            if (stopping)
                return;

            if (!logger.IsEnabled(LogLevel.Debug))
                return;

            logger.LogDebug("【执行SQL】{Sql}", UtilMethods.GetNativeSql(sql, pars));
        };

        db.Aop.OnError = ex =>
        {
            if (stopping)
                return;

            var nativeSql = ex.Parametres is SugarParameter[] pars
                ? UtilMethods.GetNativeSql(ex.Sql, pars)
                : ex.Sql;

            logger.LogError(ex, "【错误SQL】{Sql}", nativeSql);
        };

        db.Aop.OnLogExecuted = (sql, pars) =>
        {
            if (stopping)
                return;

            if (db.Ado.SqlExecutionTime.TotalSeconds <= 5)
                return;

            var trace = db.Ado.SqlStackTrace;

            logger.LogWarning(
                "【超时SQL】文件={File} 行={Line} 方法={Method} 耗时={Elapsed}s SQL={Sql}",
                trace?.FirstFileName,
                trace?.FirstLine,
                trace?.FirstMethodName,
                db.Ado.SqlExecutionTime.TotalSeconds,
                UtilMethods.GetNativeSql(sql, pars));
        };

        // Insert/Update 主键与审计字段：仅对象写路径（InsertByObject / UpdateByObject）生效
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            if (stopping)
                return;

            var currentAccount = ResolveCurrentAccount();
            var hasAccount = currentAccount is { Id: not 0 };

            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                // 雪花主键：未赋值时生成
                if (entityInfo.EntityColumnInfo.IsPrimarykey
                    && entityInfo.EntityColumnInfo.PropertyInfo.PropertyType == typeof(long))
                {
                    var id = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                    if (id is null or 0L)
                        entityInfo.SetValue(YitIdHelper.NextId());
                }

                // 创建审计：依赖当前登录用户；无账户则跳过（Id 仍可能已生成）
                if (!hasAccount)
                    return;

                if (entityInfo.PropertyName == nameof(BaseEntity.CreatedTime))
                {
                    var createTime = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                    if (createTime is null || createTime.Equals(DateTime.MinValue))
                        entityInfo.SetValue(DateTime.Now);
                }
                else if (entityInfo.PropertyName == nameof(BaseEntity.CreatedBy))
                {
                    var currentValue = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue);
                    if (currentValue is null or 0L)
                        entityInfo.SetValue(currentAccount!.Id);
                }
                else if (entityInfo.PropertyName == nameof(BaseEntity.Creator))
                {
                    var currentValue = entityInfo.EntityColumnInfo.PropertyInfo.GetValue(entityInfo.EntityValue) as string;
                    if (string.IsNullOrEmpty(currentValue))
                        entityInfo.SetValue(currentAccount!.RealName);
                }
            }
            else if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                // 修改审计：ModifiedTime 始终刷新；操作人仅登录态写入
                if (entityInfo.PropertyName == nameof(BaseEntity.ModifiedTime))
                {
                    entityInfo.SetValue(DateTime.Now);
                }
                else if (hasAccount && entityInfo.PropertyName == nameof(BaseEntity.ModifiedBy))
                {
                    entityInfo.SetValue(currentAccount!.Id);
                }
                else if (hasAccount && entityInfo.PropertyName == nameof(BaseEntity.Modifier))
                {
                    entityInfo.SetValue(currentAccount!.RealName);
                }
            }
        };

        // 连接级查询过滤器：软删除始终挂上；租户在启动期常无登录用户（跳过），由中间件按请求再挂。
        SqlSugarFilter.SetDeletedEntityFilter(db);
        SqlSugarFilter.SetTenantEntityFilter(db);
        SqlSugarFilter.SetOrgEntityFilter(db);

        ICurrentAccount ResolveCurrentAccount()
        {
            return stopping ? null : accessor?.HttpContext?.RequestServices.GetService<ICurrentAccount>();
        }
    }

    /// <summary>
    /// 扫描并缓存 SqlSugar 相关的实体与种子数据类型。
    /// </summary>
    private static Type[] LoadSqlSugarEntityTypes(ILogger logger)
        => GlobalAssemblyLoader.LoadTypes(GlobalContext.BootstrapAssembly?.SqlSugar, logger);

    /// <summary>
    /// 初始化数据库
    /// </summary>
    private static void InitDatabase(ITenant tenant, AxisDbConnectionConfig config, Type[] allTypes, ILogger logger)
    {
        var dbProvider = tenant.GetConnectionScope(config.ConfigId);

        // 初始化 / 创建数据库
        if (config.DbSettings.EnableInitDb && config.DbType != DbType.Oracle)
        {
            dbProvider.DbMaintenance.CreateDatabase();
        }

        var isMainDb = config.ConfigId.ToString() == SqlSugarConst.MainConfigId;

        // 初始化表结构
        if (config.DbSettings.EnableInitTable)
        {
            var entityTypes = allTypes
                .Where(u => u is { IsInterface: false, IsAbstract: false, IsClass: true } && u.IsDefined(typeof(SugarTable), false))
                .Where(u => isMainDb
                    ? u.GetCustomAttributes<SysTableAttribute>().Any() || !u.GetCustomAttributes<TenantAttribute>().Any()
                    : u.GetCustomAttribute<TenantAttribute>()?.configId.ToString() == config.ConfigId.ToString())
                .ToList();

            foreach (var entityType in entityTypes)
            {
                try
                {
                    if (entityType.GetCustomAttribute<SplitTableAttribute>() == null)
                        dbProvider.CodeFirst.InitTables(entityType);
                    else
                        dbProvider.CodeFirst.SplitTables().InitTables(entityType);
                }
                catch (Exception ex)
                {
                    logger?.LogError(ex, "初始化表失败: {Entity}", entityType.FullName);
                }
            }
        }

        // 初始化种子数据
        if (!config.DbSettings.EnableInitSeed) return;

        var seedInterface = typeof(ISqlSugarEntitySeedData<>);
        var seedDataTypes = allTypes
            .Where(u => u is { IsInterface: false, IsAbstract: false, IsClass: true })
            .Select(u => new
            {
                Type = u,
                SeedInterface = u.GetInterfaces().FirstOrDefault(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == seedInterface)
            })
            .Where(x => x.SeedInterface is not null)
            .ToList();

        foreach (var item in seedDataTypes)
        {
            var entityType = item.SeedInterface!.GetGenericArguments()[0];

            // 过滤与当前数据库不匹配的种子
            if (isMainDb)
            {
                if (entityType.GetCustomAttribute<SysTableAttribute>() == null
                    && entityType.GetCustomAttribute<TenantAttribute>() != null) continue;
            }
            else
            {
                var att = entityType.GetCustomAttribute<TenantAttribute>();
                if (att == null || att.configId.ToString() != config.ConfigId.ToString()) continue;
            }

            try
            {
                var instance = Activator.CreateInstance(item.Type);
                var hasDataMethod = item.Type.GetMethod("HasData");
                if (hasDataMethod?.Invoke(instance, null) is not IEnumerable rawData) continue;
                var seedData = rawData.Cast<object>().ToList();
                if (seedData.Count == 0) continue;

                var entityInfo = dbProvider.EntityMaintenance.GetEntityInfo(entityType);
                if (entityInfo.Columns.Any(u => u.IsPrimarykey))
                {
                    var storage = dbProvider.StorageableByObject(seedData).ToStorage();
                    storage.AsInsertable.ExecuteCommand();
                    storage.AsUpdateable.ExecuteCommand();
                }
                else if (!dbProvider.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any())
                {
                    dbProvider.InsertableByObject(seedData).ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "导入种子数据失败: {Seed}", item.Type.FullName);
            }
        }
    }
}