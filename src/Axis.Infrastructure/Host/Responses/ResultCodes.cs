namespace Axis.Infrastructure.Host.Responses;

/// <summary>
/// Axis 框架统一业务码。
///
/// <para>设计原则</para>
/// <list type="bullet">
/// <item><description>5 位数字，<b>前 3 位等于 HTTP 状态码</b>，后 2 位为业务细分（00 为默认）。</description></item>
/// <item><description>前端可用 <c>Math.floor(code / 100)</c> 快速回推 HTTP 状态，避免升级新码破坏老逻辑。</description></item>
/// <item><description>成功固定为 <see cref="Success"/> = 10000，便于区别 HTTP 200 的 2xx 场景。</description></item>
/// <item><description>业务模块可在 <c>40200-40299</c> / <c>42200-42299</c> 等预留段自由扩展，无需改本文件。</description></item>
/// </list>
///
/// <para>为何用 <c>static class + const int</c> 而不是 <c>enum</c>：
/// 便于业务下游不改本程序集就扩展码值；同时 JSON 序列化时是整数而非字符串，前后端对齐更简单。</para>
/// </summary>
public static class ResultCodes
{
    // ===== 2xx 成功 =====

    /// <summary>操作成功（HTTP 200）。</summary>
    public const int Success = 10000;

    // ===== 4xx 客户端错误 =====

    /// <summary>参数错误 / 请求不合法（HTTP 400）。</summary>
    public const int BadRequest = 40000;

    /// <summary>校验失败（HTTP 400）。</summary>
    public const int ValidationFailed = 40001;

    /// <summary>业务规则拒绝（HTTP 400，来自 <see cref="Exceptions.BusinessException"/> 默认码）。</summary>
    public const int BusinessError = 42200;

    /// <summary>未认证（HTTP 401）。</summary>
    public const int Unauthorized = 40100;

    /// <summary>Token 已过期（HTTP 401）。</summary>
    public const int TokenExpired = 40101;

    /// <summary>Token 无效 / 篡改（HTTP 401）。</summary>
    public const int TokenInvalid = 40102;

    /// <summary>无权限（HTTP 403）。</summary>
    public const int Forbidden = 40300;

    /// <summary>资源不存在（HTTP 404）。</summary>
    public const int NotFound = 40400;

    /// <summary>方法不允许（HTTP 405）。</summary>
    public const int MethodNotAllowed = 40500;

    /// <summary>请求冲突（HTTP 409，如唯一键冲突 / 版本冲突）。</summary>
    public const int Conflict = 40900;

    /// <summary>请求频率过高（HTTP 429）。</summary>
    public const int TooManyRequests = 42900;

    // ===== 5xx 服务端错误 =====

    /// <summary>服务器内部错误（HTTP 500）。</summary>
    public const int ServerError = 50000;

    /// <summary>外部依赖不可用（HTTP 503）。</summary>
    public const int DependencyUnavailable = 50300;

    /// <summary>请求超时（HTTP 504）。</summary>
    public const int Timeout = 50400;

    /// <summary>
    /// 根据业务码反推出 HTTP 状态码。
    /// <list type="bullet">
    /// <item><description><see cref="Success"/> → 200</description></item>
    /// <item><description>40000-49999 → 400-499（取前 3 位）</description></item>
    /// <item><description>50000-59999 → 500-599（取前 3 位）</description></item>
    /// <item><description>其它未知码 → 500</description></item>
    /// </list>
    /// </summary>
    public static int ToHttpStatus(int code) => code switch
    {
        Success => 200,
        >= 20000 and < 30000 => code / 100, // 预留 2xx 扩展
        >= 40000 and < 60000 => code / 100,
        _ => 500
    };

    /// <summary>
    /// 判断业务码是否代表成功（仅 <see cref="Success"/> 为真；2xx 预留段按需调整）。
    /// </summary>
    public static bool IsSuccess(int code) => code == Success;
}