using Axis.Application.Author.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Author;

/// <summary>
/// 登录会话应用服务契约：验证码、登录、刷新令牌、退出。
/// </summary>
/// <remarks>
/// 不负责当前用户资料与菜单装配；见 <see cref="IAuthorProfileAppService"/>。
/// Cookie 读写由 Endpoint / <see cref="Infrastructure.Security.IRefreshTokenCookieService"/> 处理。
/// </remarks>
public interface IAuthorAppService
{
    /// <summary>
    /// Web 端登录：校验锁定与验证码，解密并校验密码后签发令牌对。
    /// </summary>
    /// <param name="req">账号、SM2 密文密码、验证码与记住我选项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>AccessToken 与 RefreshToken 配对；RefreshToken 明文仅此返回一次。</returns>
    /// <exception cref="BusinessException">验证码错误、账号锁定、密码错误或账号冻结时抛出。</exception>
    Task<IssuedTokenPair> LoginAsync(LoginRequest req, CancellationToken ct = default);

    /// <summary>
    /// 使用刷新令牌哈希查找未吊销会话，条件消费成功后签发新的令牌对。
    /// </summary>
    /// <param name="refreshToken">客户端持有的明文刷新令牌；服务端仅存储其哈希。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>新的 AccessToken 与 RefreshToken 配对。</returns>
    /// <remarks>
    /// 在工作单元内执行；并发下同一刷新令牌仅允许一个请求消费成功。
    /// </remarks>
    /// <exception cref="BusinessException">令牌无效、已使用、已过期或账号冻结时抛出。</exception>
    Task<IssuedTokenPair> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// 按哈希吊销刷新令牌；空令牌视为幂等成功。
    /// </summary>
    /// <param name="refreshToken">客户端持有的明文刷新令牌。</param>
    /// <param name="ct">取消令牌。</param>
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    /// <summary>
    /// 生成图形验证码及其 Id。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>验证码 Id 与 Base64 图片。</returns>
    Task<CaptchaResponse> GetCaptchaAsync(CancellationToken ct = default);
}
