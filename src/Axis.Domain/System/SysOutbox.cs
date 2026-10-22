using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 事务性 Outbox 行。
/// </summary>
/// <remarks>
/// 与业务写同事务插入；Dispatcher 提交后再投递。本表不实现 <c>ITenantId</c>，
/// 避免后台扫描被租户过滤器挡住。不走软删除。
/// </remarks>
[SugarTable("axis_outbox", "事务性Outbox")]
[SysTable]
[SugarIndex("uk_{table}_event", nameof(EventId), OrderByType.Asc, true)]
[SugarIndex("ix_{table}_due", nameof(Status), OrderByType.Asc, nameof(NextRetryTime), OrderByType.Asc)]
public class SysOutbox : BaseEntityId
{
    /// <summary>
    /// 业务事件 Id；全表唯一，用于幂等。
    /// </summary>
    [SugarColumn(ColumnDescription = "事件Id")]
    public long EventId { get; set; }

    /// <summary>
    /// 事件 CLR 全名，用于反序列化。
    /// </summary>
    [SugarColumn(ColumnDescription = "事件类型", Length = 256)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// 事件 JSON 载荷。
    /// </summary>
    [SugarColumn(ColumnDescription = "载荷", ColumnDataType = "text")]
    public string Payload { get; set; } = null!;

    /// <summary>
    /// 投递时恢复的租户；平台事件可空。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id", IsNullable = true)]
    public long? TenantId { get; set; }

    /// <summary>
    /// 追踪号。
    /// </summary>
    [SugarColumn(ColumnDescription = "追踪号", Length = 64, IsNullable = true)]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// 投递状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public OutboxStatusEnum Status { get; set; } = OutboxStatusEnum.Pending;

    /// <summary>
    /// 已重试次数。
    /// </summary>
    [SugarColumn(ColumnDescription = "重试次数")]
    public int RetryCount { get; set; }

    /// <summary>
    /// 下次投递时间；空表示立即。
    /// </summary>
    [SugarColumn(ColumnDescription = "下次投递时间", IsNullable = true)]
    public DateTime? NextRetryTime { get; set; }

    /// <summary>
    /// 最近一次失败信息（截断）。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近错误", Length = 2000, IsNullable = true)]
    public string? LastError { get; set; }

    /// <summary>
    /// 入队时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 成功投递时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "投递时间", IsNullable = true)]
    public DateTime? PublishedTime { get; set; }
}
