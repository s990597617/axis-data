using System.Security.Cryptography;
using System.Text;

namespace Axis.Infrastructure.Security;

/// <summary>
/// 管理端防重放用的 HMAC-SHA256 请求签名（密钥为当前 AccessToken）。
/// </summary>
/// <remarks>
/// 规范化串：<c>METHOD&amp;path&amp;timestamp&amp;nonce</c>（与后续 OpenAPI 的 accessKey 方案字段不同，勿混用）。
/// OpenAPI AccessKey/Secret 签名另立认证方案，不在此处理。
/// </remarks>
public static class RequestHmacSigner
{
    /// <summary>
    /// 组装 OpenAPI 规范化待签字符串：<c>METHOD&amp;path&amp;accessKey&amp;timestamp&amp;nonce</c>。
    /// </summary>
    public static string BuildOpenApiPayload(
        string method,
        string path,
        string accessKey,
        string timestamp,
        string nonce)
        => $"{method.Trim().ToUpperInvariant()}&{NormalizePath(path)}&{accessKey}&{timestamp}&{nonce}";

    /// <summary>
    /// 组装管理端防重放规范化待签字符串：<c>METHOD&amp;path&amp;timestamp&amp;nonce</c>。
    /// </summary>
    /// <param name="method">HTTP 方法（将规范为大写）。</param>
    /// <param name="path">以 / 开头的路径，不含协议、域名与查询串。</param>
    /// <param name="timestamp">时间戳原文（与请求头一致）。</param>
    /// <param name="nonce">一次性随机串。</param>
    /// <returns>待 HMAC 的明文。</returns>
    public static string BuildPayload(string method, string path, string timestamp, string nonce)
        => $"{method.Trim().ToUpperInvariant()}&{NormalizePath(path)}&{timestamp}&{nonce}";

    /// <summary>
    /// 使用密钥对明文做 HMAC-SHA256，返回 Base64。
    /// </summary>
    /// <param name="secret">对称密钥（管理端为 AccessToken 原文）。</param>
    /// <param name="payload">规范化待签字符串。</param>
    /// <returns>Base64 签名。</returns>
    public static string SignBase64(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(payload);
        var hash = HMACSHA256.HashData(key, data);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 固定时间比较签名，避免短路计时。
    /// </summary>
    public static bool FixedEquals(string? left, string? right)
    {
        if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return false;
        var a = Encoding.UTF8.GetBytes(left);
        var b = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(a, b);
    }

    /// <summary>
    /// 规范化路径：去查询串，保证以 / 开头。
    /// </summary>
    public static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "/";

        var value = path.Trim();
        var query = value.IndexOf('?', StringComparison.Ordinal);
        if (query >= 0) value = value[..query];

        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                value = uri.AbsolutePath;
        }

        if (!value.StartsWith('/'))
            value = "/" + value;

        return value;
    }
}
