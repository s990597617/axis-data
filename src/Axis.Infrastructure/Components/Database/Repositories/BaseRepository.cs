using System.Linq.Expressions;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Components.Database.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace Axis.Infrastructure.Components.Database.Repositories;

/// <summary>
/// 基础仓储：按实体特性与当前租户解析 SqlSugar 连接作用域。
/// </summary>
/// <typeparam name="T">实体</typeparam>
/// <remarks>
/// 路由优先级：
/// <list type="number">
/// <item><see cref="TenantAttribute"/> → 特性指定库</item>
/// <item><see cref="LogTableAttribute"/> → 日志库（若已配置）</item>
/// <item><see cref="SysTableAttribute"/> → 主库</item>
/// <item>其余 → 当前登录租户的独立库 ConfigId（无则主库）</item>
/// </list>
/// 租户来自登录上下文，不读取请求头中的 TenantId。
/// </remarks>
public class BaseRepository<T> : SimpleClient<T>, IRepository<T> where T : class, new()
{
    /// <summary>
    /// 构造注入并绑定连接作用域。
    /// </summary>
    public BaseRepository(ISqlSugarClient db, IServiceProvider serviceProvider)
    {
        var iTenant = db.AsTenant();
        base.Context = iTenant.GetConnectionScope(SqlSugarConst.MainConfigId);

        if (typeof(T).IsDefined(typeof(TenantAttribute), false))
        {
            base.Context = iTenant.GetConnectionScopeWithAttr<T>();
            return;
        }

        if (typeof(T).IsDefined(typeof(LogTableAttribute), false))
        {
            if (iTenant.IsAnyConnection(SqlSugarConst.LogConfigId))
                base.Context = iTenant.GetConnectionScope(SqlSugarConst.LogConfigId);
            return;
        }

        // 系统表固定主库
        if (typeof(T).IsDefined(typeof(SysTableAttribute), false))
            return;

        var configAccessor = serviceProvider.GetService<ITenantDbConfigAccessor>()
            ?? serviceProvider.GetService<IHttpContextAccessor>()
                ?.HttpContext?.RequestServices.GetService<ITenantDbConfigAccessor>();

        var configId = configAccessor?.GetConfigId();
        if (string.IsNullOrWhiteSpace(configId) || configId == SqlSugarConst.MainConfigId)
            return;

        if (!iTenant.IsAnyConnection(configId))
            return;

        base.Context = iTenant.GetConnectionScope(configId);
    }

    #region 分表操作

    public async Task<bool> SplitTableInsertAsync(T input)
        => await base.AsInsertable(input).SplitTable().ExecuteCommandAsync() > 0;

    public async Task<bool> SplitTableInsertAsync(List<T> input)
        => await base.AsInsertable(input).SplitTable().ExecuteCommandAsync() > 0;

    public async Task<bool> SplitTableUpdateAsync(T input)
        => await base.AsUpdateable(input).SplitTable().ExecuteCommandAsync() > 0;

    public async Task<bool> SplitTableUpdateAsync(List<T> input)
        => await base.AsUpdateable(input).SplitTable().ExecuteCommandAsync() > 0;

    public async Task<bool> SplitTableDeleteableAsync(T input)
        => await base.Context.Deleteable(input).SplitTable().ExecuteCommandAsync() > 0;

    public async Task<bool> SplitTableDeleteableAsync(List<T> input)
        => await base.Context.Deleteable(input).SplitTable().ExecuteCommandAsync() > 0;

    public Task<T> SplitTableGetFirstAsync(Expression<Func<T, bool>> whereExpression)
        => base.AsQueryable().SplitTable().FirstAsync(whereExpression);

    public Task<bool> SplitTableIsAnyAsync(Expression<Func<T, bool>> whereExpression)
        => base.Context.Queryable<T>().Where(whereExpression).SplitTable().AnyAsync();

    public Task<List<T>> SplitTableGetListAsync()
        => Context.Queryable<T>().SplitTable().ToListAsync();

    public Task<List<T>> SplitTableGetListAsync(Expression<Func<T, bool>> whereExpression)
        => Context.Queryable<T>().Where(whereExpression).SplitTable().ToListAsync();

    public Task<List<T>> SplitTableGetListAsync(Expression<Func<T, bool>> whereExpression, string[] tableNames)
        => Context.Queryable<T>().Where(whereExpression).SplitTable(t => t.InTableNames(tableNames)).ToListAsync();

    #endregion
}
