namespace Axis.Application.System.OpenApiIdentity.Dto;

/// <summary>
/// 创建身份后回传明文密钥（仅此一次）。
/// </summary>
public sealed class CreateOpenApiIdentityResult
{
    /// <summary>主键。</summary>
    public long Id { get; set; }

    /// <summary>身份标识。</summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>明文 AccessSecret；请妥善保存，列表不再返回。</summary>
    public string AccessSecret { get; set; } = null!;
}
