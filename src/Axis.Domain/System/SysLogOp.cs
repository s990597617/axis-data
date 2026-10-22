using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 操作日志
/// </summary>
[SugarTable("sys_log_op", "操作日志")]
[SysTable]
[SugarIndex("index_{table}_created_time", nameof(CreatedTime), OrderByType.Desc)]
public class SysLogOp : BaseEntityId
{
    /// <summary>
    /// 操作人 ID
    /// </summary>
    [SugarColumn(ColumnDescription = "操作人ID", IsNullable = true)]
    public long? UserId { get; set; }

    /// <summary>
    /// 操作人账号
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "操作人账号", Length = 64, IsNullable = true)]
    public string Account { get; set; }

    /// <summary>
    /// 端点名称/功能描述
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "功能描述", Length = 128, IsNullable = true)]
    public string DisplayTitle { get; set; }

    /// <summary>
    /// 请求方法全名
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "方法全名", Length = 256, IsNullable = true)]
    public string FullName { get; set; }

    /// <summary>
    /// 请求方法名称
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "方法名称", Length = 128, IsNullable = true)]
    public string ActionName { get; set; }

    /// <summary>
    /// 请求方式
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "请求方式", Length = 16, IsNullable = true)]
    public string HttpMethod { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "请求地址", Length = 512, IsNullable = true)]
    public string RequestUrl { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    [SugarColumn(ColumnDescription = "请求参数", ColumnDataType = "text", IsNullable = true)]
    public string RequestParam { get; set; }

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
    /// 操作用时（毫秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "操作用时")]
    public long Elapsed { get; set; }

    /// <summary>
    /// 状态码
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "状态码", Length = 16, IsNullable = true)]
    public string State { get; set; }

    /// <summary>
    /// 跟踪 ID
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "跟踪ID", Length = 64, IsNullable = true)]
    public string TraceId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedTime { get; set; }
}
