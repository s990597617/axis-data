namespace Axis.Infrastructure.Security;

/// <summary>
/// AccessToken 拒绝名单：按 JWT <c>jti</c> 吊销尚未过期的访问令牌。
/// </summary>
/// <remarks>
/// 用于强制下线、踢人、改密后立刻失效已签发的 AccessToken。
/// 条目存活时间与令牌原过期时间对齐，过期后自然失效，无需额外清理。
/// </remarks>
public interface IAccessTokenDenyService
{
    /// <summary>
    /// 将指定 <paramref name="jti"/> 加入拒绝名单，存活至 AccessToken 原过期时间。
    /// </summary>
    /// <param name="jti">JWT Id（Claim <c>jti</c>）；空或空白时忽略。</param>
    /// <param name="accessTokenExpiresAt">令牌原过期时间；若已早于当前时间则不写入缓存。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示异步写入完成的任务。</returns>
    Task DenyAsync(string jti, DateTime accessTokenExpiresAt, CancellationToken ct = default);

    /// <summary>
    /// 批量将多个 <c>jti</c> 加入拒绝名单；忽略空 <c>jti</c> 与已过期项。
    /// </summary>
    /// <param name="items"><c>jti</c> 与对应过期时间的序列。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>表示批量写入完成的任务。</returns>
    Task DenyManyAsync(IEnumerable<(string Jti, DateTime ExpiresAt)> items, CancellationToken ct = default);

    /// <summary>
    /// 判断指定 <paramref name="jti"/> 是否已被吊销。
    /// </summary>
    /// <param name="jti">JWT Id；空或空白时返回 <c>false</c>。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>已在拒绝名单中为 <c>true</c>；否则为 <c>false</c>。</returns>
    Task<bool> IsDeniedAsync(string jti, CancellationToken ct = default);
}
