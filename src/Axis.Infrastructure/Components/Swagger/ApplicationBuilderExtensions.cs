using System.Reflection;
using Axis.Infrastructure.Components.Swagger.Attributes;
using Axis.Infrastructure.Components.Swagger.Enums;
using FastEndpoints;
using FastEndpoints.Swagger;
using IGeekFan.AspNetCore.Knife4jUI;
using Microsoft.AspNetCore.Builder;

namespace Axis.Infrastructure.Components.Swagger;

/// <summary>
/// swagger 运行程序拓展
/// </summary>
public static class SwaggerApplicationBuilderExtensions
{
    /// <summary>
    /// 启用 Swagger / Knife4UI。
    /// Knife4UI 挂在 <c>/doc</c> 路径下，而不是根路径，避免线上误开 Swagger 导致 API 列表裸奔在域名根。
    /// </summary>
    public static WebApplication UseSwagger(this WebApplication app, Action<EndpointDefinition> ep = null)
    {
        if (!GlobalContext.BootstrapSwagger.EnableSwagger) return app;

        app.UseOpenApi();
        app.UseSwaggerUi(s => s.ConfigureDefaults());

        app.UseKnife4UI(c =>
        {
            c.RoutePrefix = "";

            var groups = typeof(ApiGroupEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(f => new { Name = f.Name, Info = f.GetCustomAttribute<ApiGroupInfoAttribute>() })
                .Where(x => x.Info is not null);

            foreach (var g in groups)
            {
                c.SwaggerEndpoint($"/swagger/{g.Name}/swagger.json", g.Info!.Title);
            }
        });

        return app;
    }
}