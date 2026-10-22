namespace Axis.Application.Author.Dto;

/// <summary>
/// 图形验证码响应
/// </summary>
public sealed class CaptchaResponse
{
    /// <summary>
    /// 验证码 Id（登录时原样回传作为键）
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// 图片 base64（data URI，前端可直接赋值给 img.src）
    /// </summary>
    public string Img { get; set; } = null!;
}
