using Axis.Infrastructure.Options;

namespace Axis.Infrastructure.Constants;

/// <summary>
/// 缓存前缀归属方式。
/// </summary>
public enum CachePrefixKind
{
    /// <summary>
    /// 完整前缀，不再拼 <c>GlobalPrefix</c>（如登录失败计数、幂等锁）。
    /// </summary>
    Absolute = 0,

    /// <summary>
    /// 相对模块前缀，解析为 <c>{GlobalPrefix}{Separator}{KeyPrefix}</c>，与 <c>GenerateKey</c> 根一致。
    /// </summary>
    GlobalRooted = 1
}

/// <summary>
/// 一条缓存业务分组：键前缀 + 管理台展示信息（单一真相源）。
/// </summary>
/// <param name="Code">稳定编码（前端选中态）。</param>
/// <param name="Name">管理台展示名称。</param>
/// <param name="KeyPrefix">前缀本体；含义见 <see cref="Kind"/>。</param>
/// <param name="Description">简短说明。</param>
/// <param name="Kind">前缀解析方式。</param>
public sealed record CacheGroup(
    string Code,
    string Name,
    string KeyPrefix,
    string Description,
    CachePrefixKind Kind = CachePrefixKind.GlobalRooted);

/// <summary>
/// 框架缓存统一目录：业务键前缀、管理台分组、共用策略常量。
/// </summary>
/// <remarks>
/// 新增业务缓存时只改本文件：追加一个 <see cref="CacheGroup"/> 静态字段并加入 <see cref="Groups"/>。
/// 禁止再维护第二份前缀常量或在 AppService 内手写分组列表。
/// </remarks>
public static class CacheCatalog
{
    /// <summary>
    /// 登录锁定阈值（分钟），配合 <see cref="LoginError"/> 使用。
    /// </summary>
    public const int ErrorPasswordLockMinutes = 30;

    /// <summary>
    /// 登录允许的最大密码错误次数。
    /// </summary>
    public const int ErrorPasswordMaxCount = 5;

    /// <summary>
    /// 字典 / 配置等「近永久」元数据的兜底过期时间；以主动刷新为主，TTL 仅防脏留存。
    /// </summary>
    public static readonly TimeSpan MetaCacheTtl = TimeSpan.FromHours(24);

    /// <summary>
    /// 用户按钮权限码缓存 TTL；以角色/租户授权变更时主动失效为主，TTL 仅防脏留存。
    /// </summary>
    public static readonly TimeSpan UserPermissionCacheTtl = TimeSpan.FromHours(12);

    /// <summary>
    /// 无租户上下文时缓存键使用的全局作用域标记。
    /// </summary>
    public const string GlobalScope = "g";

    /// <summary>
    /// 字典类型启用项列表。键形如 <c>{GlobalPrefix}:sys:dict:type:{scope}:{code}</c>。
    /// </summary>
    public static readonly CacheGroup Dict = new(
        "dict",
        "字典缓存",
        "sys:dict:type",
        "字典类型启用项列表");

    /// <summary>
    /// 系统参数配置值。键形如 <c>{GlobalPrefix}:sys:config:item:{scope}:{code}</c>。
    /// </summary>
    public static readonly CacheGroup Config = new(
        "config",
        "参数配置",
        "sys:config:item",
        "系统参数配置值");

    /// <summary>
    /// 编码规则定义。键形如 <c>{GlobalPrefix}:sys:code-rule:item:{scope}:{code}</c>。
    /// </summary>
    public static readonly CacheGroup CodeRule = new(
        "code-rule",
        "编码规则",
        "sys:code-rule:item",
        "编码规则定义快照");

    /// <summary>
    /// 当前租户启用中的编码规则列表。键形如 <c>{GlobalPrefix}:sys:code-rule:list:{scope}</c>。
    /// </summary>
    public static readonly CacheGroup CodeRuleList = new(
        "code-rule-list",
        "编码规则列表",
        "sys:code-rule:list",
        "启用中的编码规则列表");

    /// <summary>
    /// 登录密码错误次数。键形如 <c>axis:login:error:{account}</c>（绝对前缀）。
    /// </summary>
    public static readonly CacheGroup LoginError = new(
        "login-error",
        "登录失败计数",
        "axis:login:error:",
        "密码错误次数与锁定",
        CachePrefixKind.Absolute);

    /// <summary>
    /// 接口幂等占位。键形如 <c>axis:idem:{endpoint}:{key}</c>（绝对前缀）。
    /// </summary>
    public static readonly CacheGroup Idempotency = new(
        "idempotency",
        "幂等锁",
        "axis:idem",
        "接口幂等请求占位",
        CachePrefixKind.Absolute);

    /// <summary>
    /// AccessToken 拒绝名单。键形如 <c>axis:auth:at:deny:{jti}</c>（绝对前缀）。
    /// </summary>
    public static readonly CacheGroup AccessTokenDeny = new(
        "at-deny",
        "AccessToken 拒绝名单",
        "axis:auth:at:deny:",
        "强制下线 / 登出吊销的 AT jti",
        CachePrefixKind.Absolute);

    /// <summary>
    /// 请求防重放 Nonce。键形如 <c>axis:auth:req:nonce:{userId}:{nonce}</c>（绝对前缀）。
    /// </summary>
    public static readonly CacheGroup RequestNonce = new(
        "req-nonce",
        "请求防重放 Nonce",
        "axis:auth:req:nonce:",
        "已消费的一次性请求 nonce",
        CachePrefixKind.Absolute);

    /// <summary>
    /// OpenAPI 签名 Nonce。键形如 <c>axis:auth:openapi:nonce:{accessKey}:{nonce}</c>（绝对前缀）。
    /// </summary>
    public static readonly CacheGroup OpenApiNonce = new(
        "openapi-nonce",
        "OpenAPI 签名 Nonce",
        "axis:auth:openapi:nonce:",
        "开放接口签名已消费 nonce",
        CachePrefixKind.Absolute);

    /// <summary>
    /// 用户按钮权限码。键形如 <c>{GlobalPrefix}:sys:user:perm:{userId}</c>。
    /// </summary>
    public static readonly CacheGroup UserPermission = new(
        "user-permission",
        "用户权限码",
        "sys:user:perm",
        "登录用户按钮权限码集合");

    /// <summary>
    /// 管理台已知业务分组（不含「全部」汇总项，「全部」由管理服务聚合）。
    /// </summary>
    public static IReadOnlyList<CacheGroup> Groups { get; } =
    [
        Dict,
        Config,
        CodeRule,
        CodeRuleList,
        LoginError,
        Idempotency,
        AccessTokenDeny,
        RequestNonce,
        OpenApiNonce,
        UserPermission
    ];

    /// <summary>
    /// 按当前缓存键配置解析实际扫描 / 清理 / 写入前缀。
    /// </summary>
    public static string ResolvePrefix(CacheGroup group, CacheKeyOptions keyOptions)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(keyOptions);

        if (group.Kind == CachePrefixKind.Absolute)
            return group.KeyPrefix;

        var global = keyOptions.GlobalPrefix ?? string.Empty;
        var sep = keyOptions.Separator ?? ":";
        return $"{global}{sep}{group.KeyPrefix}";
    }

    /// <summary>
    /// 解析全部已登记分组的实际前缀。
    /// </summary>
    public static IReadOnlyList<(CacheGroup Group, string Prefix)> ResolveAll(CacheKeyOptions keyOptions)
        => Groups.Select(g => (g, ResolvePrefix(g, keyOptions))).ToList();
}
