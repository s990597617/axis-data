namespace Axis.Infrastructure.Options;

/// <summary>
/// 系统级运行配置（雪花、跨域、密码策略、SM2 密钥等）。
/// </summary>
public class AxisSystemOptions
{
    /// <summary>
    /// 雪花 Id 生成基准 WorkerId。
    /// </summary>
    public int WorkId { get; set; }

    /// <summary>
    /// 允许跨域的地址，多个地址以 <c>|</c> 分隔。
    /// </summary>
    public string CorsUrls { get; set; }

    /// <summary>
    /// 是否开启密码强度验证。
    /// </summary>
    public bool StrongPassword { get; set; }

    /// <summary>
    /// 强密码验证规则（正则等）。
    /// </summary>
    public string PasswordStrengthValidation { get; set; }

    /// <summary>
    /// 强密码验证失败提示文案。
    /// </summary>
    public string PasswordStrengthValidationMsg { get; set; }

    /// <summary>
    /// SM2 公钥（HEX）。
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// SM2 私钥（HEX）。
    /// </summary>
    public string PrivateKey { get; set; }
}
