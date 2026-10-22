namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 密钥引用可选项（仅键名，不含明文）。
/// </summary>
public sealed class SecretRefOptionItemDto
{
    /// <summary>展示名。</summary>
    public string Label { get; set; } = null!;

    /// <summary>写入引用时的键（不含 config:/env: 前缀）。</summary>
    public string Value { get; set; } = null!;

    /// <summary>配置中是否已有非空值（不回传内容）。</summary>
    public bool Configured { get; set; }
}

/// <summary>
/// 数据源密钥引用选项集合。
/// </summary>
public sealed class SecretRefOptionsDto
{
    /// <summary>应用配置键（对应 config:）。</summary>
    public List<SecretRefOptionItemDto> ConfigKeys { get; set; } = [];

    /// <summary>环境变量名（对应 env:，仅 DH_SOURCE_ 前缀）。</summary>
    public List<SecretRefOptionItemDto> EnvKeys { get; set; } = [];
}
