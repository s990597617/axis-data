#nullable enable

using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Security;

/// <summary>
/// Web 端 Refresh Token Cookie 的统一读写入口。
/// </summary>
/// <remarks>
/// 仅负责 Cookie 读写，不负责令牌签发、轮换或校验；
/// 签发与轮换由认证用例（Author）完成，明文 Refresh Token 仅通过本服务写入浏览器。
/// Cookie 选项（名称、Path、Secure、SameSite）来自 <c>AxisAuthOptions.RefreshTokenCookie</c>。
/// </remarks>
public interface IRefreshTokenCookieService
{
    /// <summary>
    /// 从当前请求 Cookie 中读取 Refresh Token 明文。
    /// </summary>
    /// <param name="context">当前 HTTP 上下文；不可为 <c>null</c>。</param>
    /// <returns>存在且非空时返回令牌明文；未携带 Cookie 时返回 <c>null</c>。</returns>
    string? Read(HttpContext context);

    /// <summary>
    /// 将 Refresh Token 明文写入响应 Cookie（HttpOnly）。
    /// </summary>
    /// <param name="context">当前 HTTP 上下文；不可为 <c>null</c>。</param>
    /// <param name="refreshToken">服务端签发的 Refresh Token 明文；敏感，仅应出现在 HttpOnly Cookie 中。</param>
    /// <param name="expiresAt">令牌绝对过期时间；仅当 <paramref name="persistent"/> 为 <c>true</c> 时写入 Cookie <c>Expires</c>。</param>
    /// <param name="persistent">
    /// 是否持久化 Cookie：
    /// <c>true</c> 时设置过期时间（关闭浏览器后仍保留）；
    /// <c>false</c> 时为会话 Cookie（浏览器关闭即失效）。
    /// </param>
    void Write(HttpContext context, string refreshToken, DateTimeOffset expiresAt, bool persistent);

    /// <summary>
    /// 删除当前请求对应的 Refresh Token Cookie（登出或令牌失效时调用）。
    /// </summary>
    /// <param name="context">当前 HTTP 上下文；不可为 <c>null</c>。</param>
    /// <remarks>
    /// 删除时使用与写入相同的 Path / Secure / SameSite，确保浏览器能匹配到原 Cookie。
    /// </remarks>
    void Delete(HttpContext context);
}
