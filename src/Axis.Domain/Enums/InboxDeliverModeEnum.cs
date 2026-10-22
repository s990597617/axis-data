namespace Axis.Domain.Enums;

/// <summary>
/// 收件箱投递写入策略；决定同一次投递是覆盖来源还是仅追加。
/// </summary>
/// <remarks>
/// 由跨模块 <c>InboxDeliverEvent</c> 携带；落库时与
/// <c>UserId + SourceType + SourceId</c> 唯一索引语义配套使用。
/// </remarks>
public enum InboxDeliverModeEnum
{
    /// <summary>
    /// 未知或未指定；Handler 遇到此值应拒绝处理。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 按来源先删后插；同一 SourceType + SourceId 的收件人集合以本次为准。
    /// </summary>
    /// <remarks>
    /// 适用于公告发布/重发、改接收范围等「以最新受众覆盖」场景。
    /// </remarks>
    ReplaceBySource = 1,

    /// <summary>
    /// 仅追加写入，不按来源撤回。
    /// </summary>
    /// <remarks>
    /// 适用于每人独立 SourceId、或明确不想清除既有收件人的场景。
    /// </remarks>
    Append = 2
}
