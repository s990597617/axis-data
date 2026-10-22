namespace Axis.Application.System.OpenApiIdentity.Dto;

/// <summary>
/// 重置密钥结果。
/// </summary>
public sealed class ResetOpenApiSecretResult
{
    /// <summary>身份标识。</summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>新的明文 AccessSecret。</summary>
    public string AccessSecret { get; set; } = null!;
}
