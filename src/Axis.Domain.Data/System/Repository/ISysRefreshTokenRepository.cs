using Axis.Domain.Data.System.Repository.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 刷新令牌仓储：负责令牌消费、吊销、在线会话投影与无效记录清理。
/// </summary>
/// <remarks>
/// 单表按 Id 查询 / 插入请直接使用基接口 <see cref="IRepository{T}"/> 的
/// <c>GetByIdAsync</c> / <c>InsertAsync</c>，勿在本接口再包装一层。
/// </remarks>
public interface ISysRefreshTokenRepository : IRepository<SysRefreshToken>
{
    /// <summary>
    /// 根据令牌哈希查找刷新令牌记录。
    /// </summary>
    /// <param name="tokenHash">刷新令牌的 SHA-256 十六进制哈希。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>匹配记录；不存在时返回 <c>null</c>。</returns>
    Task<SysRefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>
    /// 在仍未使用、未撤销且未过期时原子消费令牌，返回受影响行数。
    /// </summary>
    /// <param name="id">刷新令牌主键。</param>
    /// <param name="now">消费时间（服务器本地时间）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>成功消费时为 1，条件不满足时为 0。</returns>
    Task<int> TryConsumeAsync(long id, DateTime now, CancellationToken ct = default);

    /// <summary>
    /// 按哈希撤销令牌；记录不存在或已撤销时保持幂等。
    /// </summary>
    /// <param name="tokenHash">刷新令牌哈希。</param>
    /// <param name="now">撤销时间（服务器本地时间）。</param>
    /// <param name="ct">取消令牌。</param>
    Task RevokeByHashAsync(string tokenHash, DateTime now, CancellationToken ct = default);

    /// <summary>
    /// 按主键撤销仍有效的会话（未用尽、未吊销、未过期）。
    /// </summary>
    /// <param name="id">刷新令牌主键。</param>
    /// <param name="now">撤销时间（服务器本地时间）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>受影响行数；会话已失效时为 0。</returns>
    Task<int> RevokeByIdAsync(long id, DateTime now, CancellationToken ct = default);

    /// <summary>
    /// 撤销用户全部仍有效的会话，返回被撤销记录（含 jti）以便吊销 AccessToken。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="now">撤销时间（服务器本地时间）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>本次实际撤销的令牌列表；无有效会话时为空集合。</returns>
    Task<IReadOnlyList<SysRefreshToken>> RevokeByUserIdAsync(
        long userId,
        DateTime now,
        CancellationToken ct = default);

    /// <summary>
    /// 按令牌族吊销尚未标记撤销的全部成员，返回被触及的记录（含 jti）以便吊销 AccessToken。
    /// </summary>
    /// <remarks>
    /// 用于 Refresh Token 复用检测：已消费令牌再次出现时废掉同族后续会话。
    /// 含已消费但未撤销的历史节点，便于一并吊销其仍可能有效的配对 AT。
    /// </remarks>
    /// <param name="familyId">令牌族 Id。</param>
    /// <param name="now">撤销时间（服务器本地时间）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>本次写入撤销时间的令牌列表；族不存在或均已撤销时为空集合。</returns>
    Task<IReadOnlyList<SysRefreshToken>> RevokeByFamilyIdAsync(
        long familyId,
        DateTime now,
        CancellationToken ct = default);

    /// <summary>
    /// 分页查询仍有效的在线会话（未用尽、未吊销、未过期），并联表投影用户信息。
    /// </summary>
    /// <param name="keyword">关键字；匹配账号、姓名、客户端 IP；可为空表示不过滤。</param>
    /// <param name="page">页码，从 1 开始。</param>
    /// <param name="pageSize">每页条数。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>在线会话分页结果。</returns>
    Task<SqlSugarPagedList<ActiveSessionDto>> PageActiveSessionsAsync(
        string? keyword,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// 清理过期 / 已吊销 / 已消费且早于截止时间的刷新令牌，返回删除行数。
    /// </summary>
    /// <param name="cutoff">保留窗口截止时间；该时刻之前的无效记录将被删除。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>实际删除的行数。</returns>
    Task<int> DeleteObsoleteAsync(DateTime cutoff, CancellationToken ct = default);

    /// <summary>
    /// 清理切换哈希存储前遗留的明文刷新令牌记录（FamilyId 为空或 0）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task DeleteLegacyAsync(CancellationToken ct = default);
}
