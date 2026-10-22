namespace Axis.Infrastructure.Components.FastEndpoints.Attributes;

/// <summary>
/// 声明端点跳过请求级时间戳 + Nonce 防重放校验。
/// </summary>
/// <remarks>
/// 用于对外 OpenAPI / Webhook / 第三方回调等无法携带 Axis 管理端防重放头的接口。
/// 标注后 <see cref="Processors.AntiReplayPreProcessor"/> 直接放行；权限与业务校验仍按原配置执行。
/// </remarks>
/// <example>
/// <code>
/// [SkipAntiReplay]
/// [SkipAntiReplay("对外开放创建订单")]
/// Options(x => x.SkipAntiReplay("Webhook 回调"));
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SkipAntiReplayAttribute : Attribute
{
    /// <summary>
    /// 跳过原因（仅文档 / 排障用，不影响运行时逻辑）。
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// 创建跳过防重放声明。
    /// </summary>
    public SkipAntiReplayAttribute()
    {
    }

    /// <summary>
    /// 创建跳过防重放声明并记录原因。
    /// </summary>
    /// <param name="reason">跳过原因说明。</param>
    public SkipAntiReplayAttribute(string reason)
    {
        Reason = reason;
    }
}
