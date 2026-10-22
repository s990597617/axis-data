using System.Reflection;
using Axis.Infrastructure.Components.Swagger.Attributes;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Components.Swagger.Internal;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag;

namespace Axis.Infrastructure.Components.Swagger;

/// <summary>
/// Swagger 服务拓展
/// </summary>
public static class SwaggerServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Swagger 服务。
    /// 按 <see cref="ApiGroupEnum"/> 每个带 <see cref="ApiGroupInfoAttribute"/> 的值生成一个文档分组，
    /// 未贴特性的枚举值会被安全跳过（而不是 NRE）。
    /// </summary>
    public static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        if (!GlobalContext.BootstrapSwagger.EnableSwagger) return services;

        var groupFields = typeof(ApiGroupEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => new
            {
                Field = f,
                Info = f.GetCustomAttribute<ApiGroupInfoAttribute>()
            })
            .Where(x => x.Info is not null)
            .ToList();

        foreach (var item in groupFields)
        {
            var name = item.Field.Name;
            var info = item.Info!;

            services.SwaggerDocument(c =>
            {
                c.ShortSchemaNames = true;
                c.AutoTagPathSegmentIndex = 0;
                c.RemoveEmptyRequestSchema = true;

                // 交由手动 AddAuth 接管；避免与下方 DocumentSettings 里的 AddAuth 重复
                c.EnableJWTBearerAuth = false;

                c.DocumentSettings = s =>
                {
                    s.DocumentName = name;
                    s.Title = info.Title;
                    s.Version = info.Version;
                    s.Description = info.Description;
                    s.ApiGroupNames = [name];
                    s.AddAuth("Authorization", new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.Http,
                        Scheme = JwtBearerDefaults.AuthenticationScheme,
                        BearerFormat = "JWT",
                    });

                    // 把所有 Endpoint 的响应 schema 自动包装为 ResponseResult 信封，
                    // 并为常见错误码（400/401/403/500）补齐默认响应示例。
                    s.OperationProcessors.Add(new ResponseEnvelopeOperationProcessor());
                };
            });
        }

        return services;
    }
}