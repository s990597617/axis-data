namespace Axis.Application.Author.Dto;

/// <summary>
/// 应用层内部签发结果。Refresh Token 只允许由 Web Endpoint 写入 HttpOnly Cookie。
/// </summary>
public sealed record IssuedTokenPair(
    string AccessToken,
    long AccessTokenExpires,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpires,
    bool RememberMe);
