#nullable enable

using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Axis.Infrastructure.Security;

/// <summary>
/// 使用 HttpOnly Cookie 保存 Web 端 Refresh Token 的默认实现。
/// </summary>
/// <remarks>
/// Cookie 始终 <c>HttpOnly = true</c>、<c>IsEssential = true</c>；
/// <c>Secure</c> / <c>SameSite</c> / <c>Path</c> / 名称取自 <see cref="AxisAuthOptions.RefreshTokenCookie"/>。
/// </remarks>
/// <param name="options">认证相关配置；读取其中的 Refresh Token Cookie 节。</param>
public sealed class RefreshTokenCookieService(IOptions<AxisAuthOptions> options)
    : IRefreshTokenCookieService, IAutoInject
{
    private readonly AxisRefreshTokenCookieOptions _options = options.Value.RefreshTokenCookie;

    /// <inheritdoc />
    public string? Read(HttpContext context)
        => context.Request.Cookies.TryGetValue(_options.Name, out var value) ? value : null;

    /// <inheritdoc />
    public void Write(HttpContext context, string refreshToken, DateTimeOffset expiresAt, bool persistent)
    {
        var cookieOptions = BuildCookieOptions();
        cookieOptions.Expires = persistent ? expiresAt : null;
        context.Response.Cookies.Append(_options.Name, refreshToken, cookieOptions);
    }

    /// <inheritdoc />
    public void Delete(HttpContext context)
        => context.Response.Cookies.Delete(_options.Name, BuildCookieOptions());

    /// <summary>
    /// 按配置构造统一的 Cookie 选项（写入与删除必须一致，否则浏览器无法清除）。
    /// </summary>
    /// <returns>已填充 HttpOnly / Secure / SameSite / Path 的 <see cref="CookieOptions"/>。</returns>
    private CookieOptions BuildCookieOptions() => new()
    {
        HttpOnly = true,
        Secure = _options.Secure,
        SameSite = ResolveSameSite(_options.SameSite),
        Path = _options.Path,
        IsEssential = true
    };

    /// <summary>
    /// 将配置中的 SameSite 字符串解析为枚举；无法识别时回退为 <see cref="SameSiteMode.Lax"/>。
    /// </summary>
    /// <param name="value">配置值，如 <c>Lax</c>、<c>Strict</c>、<c>None</c>。</param>
    /// <returns>解析后的 SameSite 模式。</returns>
    private static SameSiteMode ResolveSameSite(string value)
        => Enum.TryParse<SameSiteMode>(value, true, out var mode) ? mode : SameSiteMode.Lax;
}
