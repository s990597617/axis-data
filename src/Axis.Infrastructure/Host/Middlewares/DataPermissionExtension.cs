using Axis.Infrastructure.Components.Database.Permission;
using Axis.Infrastructure.Global;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Axis.Infrastructure.Host.Middlewares;

/// <summary>
/// 请求级数据权限：认证后按当前用户重挂各连接作用域上的查询过滤器。
/// </summary>
/// <remarks>
/// 启动建库阶段通常无登录用户，租户条件无法在此时确定；
/// 因此在每个已认证请求开始时，对全部 <c>GetConnectionScope</c> 重新挂载软删除 / 租户 / 机构过滤。
/// </remarks>
public static class DataPermissionExtension
{
    /// <summary>
    /// 在 UseAuthentication / UseAuthorization 之后启用数据权限过滤。
    /// </summary>
    public static IApplicationBuilder UseAxisDataPermission(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                var db = context.RequestServices.GetService<ISqlSugarClient>();
                var loader = context.RequestServices.GetService<IDataScopeLoader>();

                if (db is not null && context.User.Identity?.IsAuthenticated == true)
                {
                    if (loader is not null)
                        DataScopeContext.Current = await loader.LoadAsync(context.RequestAborted);

                    foreach (var provider in EnumerateProviders(db))
                    {
                        SqlSugarFilter.SetDeletedEntityFilter(provider);
                        SqlSugarFilter.SetTenantEntityFilter(provider);
                        SqlSugarFilter.SetOrgEntityFilter(provider);

                        if (loader is not null)
                        {
                            var contributors = context.RequestServices
                                .GetServices<IDataPermissionContributor>();
                            SqlSugarFilter.SetCustomEntityFilter(provider, contributors);
                        }
                    }
                }
                else
                {
                    DataScopeContext.Current = null;
                }

                await next();
            }
            finally
            {
                DataScopeContext.Current = null;
            }
        });
    }

    private static IEnumerable<SqlSugarScopeProvider> EnumerateProviders(ISqlSugarClient sugar)
    {
        var tenant = sugar.AsTenant();
        var configs = GlobalContext.BootstrapDb?.ConnectionConfigs;
        if (configs is null || configs.Count == 0)
            yield break;

        foreach (var config in configs)
        {
            var configId = config.ConfigId?.ToString();
            if (string.IsNullOrWhiteSpace(configId) || !tenant.IsAnyConnection(configId))
                continue;

            yield return tenant.GetConnectionScope(configId);
        }
    }
}
