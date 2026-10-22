namespace Axis.Infrastructure.Options;

/// <summary>
/// 事务性 Outbox 选项。
/// </summary>
public sealed class AxisOutboxOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Outbox";

    /// <summary>
    /// 是否启用 Dispatcher；关闭后仍可入队，只是不自动投递。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 扫描间隔毫秒。
    /// </summary>
    public int PollIntervalMs { get; set; } = 2000;

    /// <summary>
    /// 每批拉取条数。
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// 失败转死信前的最大重试次数。
    /// </summary>
    public int MaxRetryCount { get; set; } = 8;

    /// <summary>
    /// 传输实现名，须与 <c>IOutboxTransport.Name</c> 一致。
    /// </summary>
    /// <remarks>
    /// 第一期仅实现 <c>InProcess</c>。多实例或需 Broker 时改为 <c>RabbitMq</c>
    /// 并注册对应 <c>IOutboxTransport</c>，不要改 Outbox 表结构或 <c>IOutbox.EnqueueAsync</c>。
    /// </remarks>
    public string Transport { get; set; } = OutboxTransportNames.InProcess;

    /// <summary>
    /// RabbitMQ 传输预留配置。仅当 <see cref="Transport"/> 为 <c>RabbitMq</c> 且已注册对应传输时读取。
    /// </summary>
    public AxisRabbitMqOutboxOptions RabbitMq { get; set; } = new();
}

/// <summary>
/// RabbitMQ Outbox 传输预留选项。本仓库不引用 RabbitMQ 客户端；接入时实现
/// <c>IOutboxTransport</c>（Name = <c>RabbitMq</c>）并从此节读连接参数。
/// </summary>
public sealed class AxisRabbitMqOutboxOptions
{
    /// <summary>
    /// 主机。
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// 端口。
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 虚拟主机。
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 交换机名。
    /// </summary>
    public string Exchange { get; set; } = "axis.integration";

    /// <summary>
    /// 交换机类型，建议 topic。
    /// </summary>
    public string ExchangeType { get; set; } = "topic";

    /// <summary>
    /// 消费队列名。
    /// </summary>
    public string Queue { get; set; } = "axis.integration";

    /// <summary>
    /// 路由键；出站投递与入站绑定共用。
    /// </summary>
    public string RoutingKey { get; set; } = "axis.integration.#";
}

/// <summary>
/// Outbox 传输名常量；新增 Broker 时在此追加，勿散落魔法字符串。
/// </summary>
public static class OutboxTransportNames
{
    /// <summary>
    /// 进程内 FastEndpoints Event Bus。
    /// </summary>
    public const string InProcess = "InProcess";

    /// <summary>
    /// RabbitMQ（预留；需自行注册 <c>IOutboxTransport</c> 实现）。
    /// </summary>
    public const string RabbitMq = "RabbitMq";
}
