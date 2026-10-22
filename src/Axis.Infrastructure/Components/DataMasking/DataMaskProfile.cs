namespace Axis.Infrastructure.Components.DataMasking;

/// <summary>
/// 常用脱敏档位；自定义起止位置请用 <see cref="DataMaskAttribute"/> 的 start/length 构造。
/// </summary>
public enum DataMaskProfile
{
    /// <summary>
    /// 自定义（由 startIndex / length 决定）。
    /// </summary>
    Custom = 0,

    /// <summary>
    /// 手机号：保留前 3 后 4，中间打码。
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// 身份证：保留前 4 后 4。
    /// </summary>
    IdCard = 2,

    /// <summary>
    /// 邮箱：本地名首字符 + *** + @域名。
    /// </summary>
    Email = 3,

    /// <summary>
    /// 中文姓名：保留首字，其余打码。
    /// </summary>
    Name = 4,

    /// <summary>
    /// 银行卡：仅保留后 4 位。
    /// </summary>
    BankCard = 5
}
