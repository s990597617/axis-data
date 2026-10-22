using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     刷新令牌。
///     <para>数据库只保存令牌哈希；每次刷新时原子消费旧记录并签发子记录。</para>
/// </summary>
[SugarTable("sys_refresh_token", "刷新令牌")]
[SysTable]
[SugarIndex("index_{table}_user", nameof(UserId), OrderByType.Asc)]
[SugarIndex("index_{table}_token_hash", nameof(TokenHash), OrderByType.Asc, true)]
[SugarIndex("index_{table}_family", nameof(FamilyId), OrderByType.Asc)]
public class SysRefreshToken : BaseEntity
{
    /// <summary>
    ///     用户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    ///     刷新令牌 SHA-256 哈希
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "刷新令牌哈希", Length = 64, OldColumnName = "refresh_token")]
    public string TokenHash { get; set; } = null!;

    /// <summary>
    ///     配对 AccessToken 的 jti，强制下线时写入拒绝名单
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "AccessTokenJti", Length = 64, IsNullable = true)]
    public string? AccessTokenJti { get; set; }

    /// <summary>
    ///     配对 AccessToken 过期时间，用于拒绝名单 TTL
    /// </summary>
    [SugarColumn(ColumnDescription = "AccessToken过期时间", IsNullable = true)]
    public DateTime? AccessTokenExpiresAt { get; set; }

    /// <summary>
    ///     登录客户端 IP
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "客户端IP", Length = 64, IsNullable = true)]
    public string? ClientIp { get; set; }

    /// <summary>
    ///     登录客户端 User-Agent
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "User-Agent", Length = 512, IsNullable = true)]
    public string? UserAgent { get; set; }

    /// <summary>
    ///     令牌族 Id，登录签发的首个令牌以自身 Id 作为族 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "令牌族Id")]
    public long? FamilyId { get; set; }

    /// <summary>
    ///     上一个刷新令牌 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "父令牌Id", IsNullable = true)]
    public long? ParentTokenId { get; set; }

    /// <summary>
    ///     是否已使用（一次性令牌）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已使用")]
    public bool IsUsed { get; set; }

    /// <summary>
    ///     实际消费时间（服务器本地时间）
    /// </summary>
    [SugarColumn(ColumnDescription = "消费时间", IsNullable = true)]
    public DateTime? UsedTime { get; set; }

    /// <summary>
    ///     撤销时间（服务器本地时间）
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销时间", IsNullable = true)]
    public DateTime? RevokedTime { get; set; }

    /// <summary>
    ///     过期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间")]
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    ///     是否为跨浏览器会话的持久登录
    /// </summary>
    [SugarColumn(ColumnDescription = "记住登录")]
    public bool? RememberMe { get; set; }
}
