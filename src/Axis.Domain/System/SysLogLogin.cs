using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 登录日志
/// </summary>
[SugarTable("sys_log_login", "登录日志")]
[SysTable]
[SugarIndex("index_{table}_created_time", nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("index_{table}_user_id", nameof(UserId), OrderByType.Asc)]
public class SysLogLogin : BaseEntityId
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID")]
    public long UserId { get; set; }

    /// <summary>
    /// 用户账号
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "用户账号", Length = 64)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// IP 地址
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "IP地址", Length = 64, IsNullable = true)]
    public string RemoteIp { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "User-Agent", Length = 512, IsNullable = true)]
    public string UserAgent { get; set; }

    /// <summary>
    /// 登录结果（true=成功, false=失败）
    /// </summary>
    [SugarColumn(ColumnDescription = "登录结果")]
    public bool Success { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "失败原因", Length = 256, IsNullable = true)]
    public string FailReason { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedTime { get; set; }
}
