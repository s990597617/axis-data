using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 列出可用的密钥引用选项（仅键名，不含明文）。
/// </summary>
public sealed class ListSecretRefOptionsEndpoint(IConfiguration configuration)
    : AxisEndpointWithoutRequest<SecretRefOptionsDto>
{
    public override void Configure()
    {
        Get("/datahub/source/secret-ref-options");
        Summary(summary => summary.Summary = "数据源密钥引用选项");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Query));
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var dto = new SecretRefOptionsDto
        {
            ConfigKeys = CollectConfigKeys(),
            EnvKeys = CollectEnvKeys()
        };
        Response = dto;
        return Task.CompletedTask;
    }

    private List<SecretRefOptionItemDto> CollectConfigKeys()
    {
        var map = new Dictionary<string, SecretRefOptionItemDto>(StringComparer.OrdinalIgnoreCase);

        void AddSection(string sectionPath, string keyPrefix)
        {
            foreach (var child in configuration.GetSection(sectionPath).GetChildren())
            {
                if (string.IsNullOrWhiteSpace(child.Key)) continue;
                // 跳过嵌套对象：只收叶子或连接串值
                var value = child.Value;
                var hasChildren = child.GetChildren().Any();
                if (hasChildren && value is null) continue;

                var key = $"{keyPrefix}{child.Key}";
                var configured = !string.IsNullOrWhiteSpace(value);
                var label = configured ? $"{key}（已配置）" : $"{key}（未配置）";
                map[key] = new SecretRefOptionItemDto
                {
                    Label = label,
                    Value = key,
                    Configured = configured
                };
            }
        }

        AddSection("ConnectionStrings", "ConnectionStrings:");
        AddSection("DataHub:Sources", "DataHub:Sources:");

        return map.Values.OrderByDescending(x => x.Configured).ThenBy(x => x.Value, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static List<SecretRefOptionItemDto> CollectEnvKeys()
    {
        const string prefix = "DH_SOURCE_";
        var list = new List<SecretRefOptionItemDto>();
        foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            var name = entry.Key?.ToString();
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            var configured = !string.IsNullOrWhiteSpace(entry.Value?.ToString());
            list.Add(new SecretRefOptionItemDto
            {
                Label = configured ? $"{name}（已配置）" : $"{name}（未配置）",
                Value = name,
                Configured = configured
            });
        }

        return list.OrderByDescending(x => x.Configured).ThenBy(x => x.Value, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
