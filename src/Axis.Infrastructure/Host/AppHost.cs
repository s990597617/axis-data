using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Axis.Infrastructure.Components.Caching;
using Axis.Infrastructure.Components.DataMasking;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.FastEndpoints;
using Axis.Infrastructure.Components.FileStorage;
using Axis.Infrastructure.Components.Mapster;
using Axis.Infrastructure.Components.Messaging;
using Axis.Infrastructure.Components.Swagger;
using Axis.Infrastructure.Dependency;
using Axis.Infrastructure.Host.JsonConverter;
using Axis.Infrastructure.Host.Middlewares;
using Axis.Infrastructure.Host.Tools;
using Axis.Infrastructure.Options;
using Axis.Infrastructure.Security;
using Axis.Infrastructure.Security.OpenApi;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

namespace Axis.Infrastructure.Host;

/// <summary>
/// Axis Host 构造器
/// </summary>
public static class AppHost
{
    /// <summary>
    /// 框架入口程序
    /// </summary>
    public static void Run(this WebApplicationBuilder builder, string[] args, Action<IServiceCollection> serviceAction = null,
        Action<EndpointDefinition> ep = null, Action<WebApplication> appAction = null)
    {
        // ===== 命令行工具 =====
        // 用法：dotnet run --project Axis.Host.Api -- gen-sm2
        if (HostToolExecutor.TryExecute(args))
            return;

        // ===== 宿主与配置 =====
        GlobalContext.InitBootstrap(builder.Configuration);

        builder.Host.UseSerilog((context, configuration) =>
            configuration.ReadFrom.Configuration(context.Configuration));

        // ===== 基础设施：上下文 / 序列化 / HttpClient =====
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
        });

        builder.Services.Configure<JsonOptions>(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            o.SerializerOptions.Converters.Add(new DatetimeJsonConverter());
            o.SerializerOptions.Converters.Add(new NullableDatetimeJsonConverter());
            o.SerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
            o.SerializerOptions.Converters.Add(new NullableDateTimeOffsetJsonConverter());
            o.SerializerOptions.Converters.Add(new LongJsonConverter());
            o.SerializerOptions.Converters.Add(new NullableLongJsonConverter());
            // 出参 DTO 上的 [DataMask] 在序列化时自动打码。
            o.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { DataMaskJsonTypeInfoModifier.Modify }
            };
            o.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
            o.SerializerOptions.AllowTrailingCommas = true;
            o.SerializerOptions.PropertyNameCaseInsensitive = false;
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddHttpClient();
        var authSection = builder.Configuration.GetSection(AxisAuthOptions.SectionName);
        var allowedOrigins = authSection.GetSection(nameof(AxisAuthOptions.AllowedOrigins))
            .GetChildren()
            .Select(item => item.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();
        builder.Services.Configure<AxisAuthOptions>(authSection);
        builder.Services.Configure<SignatureAuthenticationOptions>(
            SignatureAuthenticationDefaults.AuthenticationScheme,
            authSection.GetSection("OpenApiSignature"));
        builder.Services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()));

        // ===== 框架核心服务（顺序：Mapster → DB → Caching → Captcha → Swagger → DI 扫描） =====
        builder.Services.AddObjectMapper();
        builder.Services.AddSqlSugarSetup();
        builder.Services.AddAxisEasyCaching();
        builder.Services.AddCaptcha(builder.Configuration);
        builder.Services.Configure<AxisUploadOptions>(builder.Configuration.GetSection("Upload"));
        builder.Services.Configure<AxisCodeGenOptions>(builder.Configuration.GetSection(AxisCodeGenOptions.SectionName));
        builder.Services.AddLocalFileStorageWithPersistence();
        builder.Services.AddAxisMessaging(builder.Configuration);

        // ===== 业务扩展点：放在核心基础设施之后、扫描之前 =====
        serviceAction?.Invoke(builder.Services);

        // ===== DI 扫描（业务注册完再扫，允许业务方在 serviceAction 覆盖默认实现） =====
        builder.Services.AddDependencySetup();

        // ===== Web 门面：FastEndpoints + Swagger =====
        builder.Services.AddFastEndPointsSetup();
        builder.Services.AddSwaggerSetup();

        // ===== 构建宿主 =====
        var app = builder.Build();

        // ===== 中间件管道（顺序严格） =====
        // 1. 请求体可倒带：必须最早注册，让后续中间件 / 异常处理器都能读到 body
        app.Use(next => context =>
        {
            context.Request.EnableBuffering();
            return next(context);
        });

        // 2. 全局异常处理：统一映射 BusinessException / ValidationFailureException / 其它
        app.UseAxisExceptionHandler();

        // 3. 静态资源
        app.UseStaticFiles();

        // 3.1 上传文件目录对外提供静态访问
        var uploadPath = Path.IsPathRooted(GlobalContext.BootstrapUpload.Path)
            ? GlobalContext.BootstrapUpload.Path
            : Path.Combine(AppContext.BaseDirectory, GlobalContext.BootstrapUpload.Path);
        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadPath),
            RequestPath = $"/{GlobalContext.BootstrapUpload.Path}"
        });

        // 4. CORS
        app.UseCors();

        // 5. 认证 / 授权
        app.UseAuthentication();
        app.UseAuthorization();

        // 5.1 数据权限：认证后解析范围并写入 SqlSugar 请求级 QueryFilter
        app.UseAxisDataPermission();

        // 6. 审计日志（在 Auth 之后，这样日志里能拿到当前登录用户）
        app.UsePerformanceLog();

        // 7. FastEndpoints 终结点管道
        app.UseFastEndPointsApp(ep);

        // 8. Swagger 文档（放在 Endpoints 之后仍能工作，但需确保未被 CORS 策略拒绝）
        app.UseSwagger();

        // 9. 初始化全局运行时资源
        GlobalContext.InitRuntime(app.Services, builder.Services, app);

        // 10. 注册启动 Banner + 优雅停机钩子
        app.UseAxisLifetime();

        // 11. 宿主侧附加映射（SignalR Hub 等）
        appAction?.Invoke(app);

        // ===== 启动 =====
        app.Run();
    }
}
