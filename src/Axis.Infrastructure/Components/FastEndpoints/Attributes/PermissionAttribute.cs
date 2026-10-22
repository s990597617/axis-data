namespace Axis.Infrastructure.Components.FastEndpoints.Attributes;

/// <summary>
/// 权限码组合策略。
/// <para>
/// 用于 <see cref="PermissionAttribute"/> 指定多权限码之间的判定语义：
/// "全部具备才允许"或"任一具备即允许"。
/// </para>
/// </summary>
public enum PermissionPolicy
{
    /// <summary>
    /// 必须同时具备所有权限码才通过（AND 语义）。
    /// <para>
    /// 适合"查看订单且导出订单"等需要多个细粒度权限叠加的场景。
    /// </para>
    /// </summary>
    AllOf = 0,

    /// <summary>
    /// 任意一个权限码命中即通过（OR 语义）。
    /// <para>
    /// 适合"订单管理员或客服主管均可查看"等多角色并集场景。
    /// </para>
    /// </summary>
    AnyOf = 1
}

/// <summary>
/// 声明一个 FastEndpoints 端点所需的权限码。
/// <para>
/// 校验由 <see cref="Processors.PermissionPreProcessor"/> 统一执行：
/// 它会从 DI 解析 <see cref="Permission.IPermissionService"/> 并调用对应策略方法，
/// 未通过时以 <see cref="Host.Responses.ResultCodes.Forbidden"/> 抛出业务异常。
/// 业务实现从用户权限码缓存判定（登录预热，未命中回源）。
/// </para>
/// <para>
/// 若 <see cref="Permission.IPermissionService"/> 未注册，处理器失败关闭并拒绝请求，禁止裸放行。
/// </para>
/// <example>
/// <code>
/// [Permission("order:view")]
/// public class GetOrderEndpoint : AxisEndpoint&lt;GetOrderReq, OrderDto&gt; { ... }
///
/// [Permission(PermissionPolicy.AnyOf, "order:admin", "cs:supervisor")]
/// public class ListOrderEndpoint : AxisEndpoint&lt;ListOrderReq, ListOrderRes&gt; { ... }
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class PermissionAttribute : Attribute
{
    /// <summary>
    /// 端点所需的权限码列表。至少一项；为空将被视为"配置错误"并放行。
    /// </summary>
    public string[] Codes { get; }

    /// <summary>
    /// 多权限码之间的判定策略，默认为 <see cref="PermissionPolicy.AllOf"/>。
    /// </summary>
    public PermissionPolicy Policy { get; }

    /// <summary>
    /// 使用默认 <see cref="PermissionPolicy.AllOf"/> 策略声明所需权限码。
    /// </summary>
    /// <param name="codes">权限码列表。</param>
    public PermissionAttribute(params string[] codes)
        : this(PermissionPolicy.AllOf, codes) { }

    /// <summary>
    /// 使用指定策略声明所需权限码。
    /// </summary>
    /// <param name="policy">权限码组合策略。</param>
    /// <param name="codes">权限码列表。</param>
    public PermissionAttribute(PermissionPolicy policy, params string[] codes)
    {
        Policy = policy;
        Codes = codes ?? Array.Empty<string>();
    }
}
