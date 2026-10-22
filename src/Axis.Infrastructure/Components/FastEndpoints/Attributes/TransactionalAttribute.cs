using System.Data;

namespace Axis.Infrastructure.Components.FastEndpoints.Attributes;

/// <summary>
/// 标记一个 FastEndpoints 端点需要在统一数据库事务中执行。
/// <para>
/// 事务由 <see cref="Processors.TransactionPreProcessor"/> 开启，
/// 由 <see cref="Processors.TransactionPostProcessor"/> 提交或回滚：
/// </para>
/// <list type="bullet">
///   <item><description>端点执行成功（未抛异常）→ Commit</description></item>
///   <item><description>端点抛出任意异常 → Rollback（异常继续向外抛给 <c>UseAxisExceptionHandler</c>）</description></item>
/// </list>
/// <para>
/// 事务边界覆盖 <b>端点 Handler 本体</b>，不会覆盖后置日志/审计处理器，
/// 避免审计写入回滚或拖慢主事务。
/// </para>
/// <example>
/// <code>
/// [Transactional]
/// public class CreateOrderEndpoint : AxisEndpoint&lt;CreateOrderReq, OrderDto&gt; { ... }
///
/// [Transactional(IsolationLevel = IsolationLevel.Serializable)]
/// public class TransferEndpoint : AxisEndpoint&lt;TransferReq&gt; { ... }
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TransactionalAttribute : Attribute
{
    /// <summary>
    /// 事务隔离级别，默认 <see cref="IsolationLevel.ReadCommitted"/>。
    /// </summary>
    public IsolationLevel IsolationLevel { get; init; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// 指定使用的数据库连接 ConfigId；留空表示使用 <c>ISqlSugarClient</c> 默认连接。
    /// 多库场景下可指定特定连接参与事务。
    /// </summary>
    public string ConnectionId { get; init; }
}
