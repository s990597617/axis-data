namespace Axis.Application.Author.Dto;

/// <summary>
/// Web 登录与刷新响应，只返回短期 Access Token。
/// </summary>
public sealed class WebTokenResponse
{
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Access Token 过期时间（Unix 毫秒时间戳）。
    /// </summary>
    public long AccessTokenExpires { get; set; }

    public static WebTokenResponse From(IssuedTokenPair pair) => new()
    {
        AccessToken = pair.AccessToken,
        AccessTokenExpires = pair.AccessTokenExpires
    };
}
