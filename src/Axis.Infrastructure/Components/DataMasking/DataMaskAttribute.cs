namespace Axis.Infrastructure.Components.DataMasking;

/// <summary>
/// 出参字符串脱敏特性：标在 DTO / 导出行模型属性上，由 JSON / Excel 钩子自动处理。
/// </summary>
/// <remarks>
/// 仅用于对外输出模型，禁止标在会写回数据库的实体字段上，以免把掩码写入库。
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataMaskAttribute : Attribute
{
    /// <summary>
    /// 脱敏档位。
    /// </summary>
    public DataMaskProfile Profile { get; }

    /// <summary>
    /// 自定义脱敏起始下标（从 0 起）；仅 <see cref="DataMaskProfile.Custom"/> 生效。
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// 自定义脱敏长度；仅 <see cref="DataMaskProfile.Custom"/> 生效。
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 脱敏字符，默认 <c>*</c>。
    /// </summary>
    public char MaskChar { get; init; } = '*';

    /// <summary>
    /// 使用预设档位构造。
    /// </summary>
    /// <param name="profile">预设档位，不能为 <see cref="DataMaskProfile.Custom"/>。</param>
    public DataMaskAttribute(DataMaskProfile profile)
    {
        if (profile == DataMaskProfile.Custom)
            throw new ArgumentException("Custom 档位请使用 startIndex/length 构造函数。", nameof(profile));

        Profile = profile;
        StartIndex = 0;
        Length = 0;
    }

    /// <summary>
    /// 使用自定义起止位置构造。
    /// </summary>
    /// <param name="startIndex">起始下标（从 0 起）。</param>
    /// <param name="length">打码长度。</param>
    public DataMaskAttribute(int startIndex, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        Profile = DataMaskProfile.Custom;
        StartIndex = startIndex;
        Length = length;
    }

    /// <summary>
    /// 对明文执行脱敏；空串原样返回。
    /// </summary>
    /// <param name="input">原始字符串。</param>
    /// <returns>脱敏后的字符串。</returns>
    public string Mask(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input ?? string.Empty;

        return Profile switch
        {
            DataMaskProfile.Mobile => MaskFixedKeep(input, keepHead: 3, keepTail: 4),
            DataMaskProfile.IdCard => MaskFixedKeep(input, keepHead: 4, keepTail: 4),
            DataMaskProfile.Email => MaskEmail(input),
            DataMaskProfile.Name => MaskName(input),
            DataMaskProfile.BankCard => MaskFixedKeep(input, keepHead: 0, keepTail: 4),
            _ => MaskCustom(input)
        };
    }

    private string MaskCustom(string input)
    {
        if (input.Length <= StartIndex)
            return input;

        var maskedLength = Math.Min(Length, input.Length - StartIndex);
        var maskStr = new string(MaskChar, maskedLength);
        var tailStart = StartIndex + maskedLength;
        return input[..StartIndex] + maskStr + (tailStart < input.Length ? input[tailStart..] : string.Empty);
    }

    private string MaskFixedKeep(string input, int keepHead, int keepTail)
    {
        if (input.Length <= keepHead + keepTail)
            return new string(MaskChar, input.Length);

        var mid = input.Length - keepHead - keepTail;
        return input[..keepHead] + new string(MaskChar, mid) + input[^keepTail..];
    }

    private string MaskEmail(string input)
    {
        var at = input.IndexOf('@');
        if (at <= 0 || at == input.Length - 1)
            return MaskFixedKeep(input, 1, 0);

        var local = input[..at];
        var domain = input[(at + 1)..];
        var localMasked = local.Length == 1
            ? MaskChar.ToString()
            : local[0] + new string(MaskChar, Math.Min(3, local.Length - 1));
        return $"{localMasked}@{domain}";
    }

    private string MaskName(string input)
    {
        if (input.Length == 1)
            return MaskChar.ToString();
        return input[0] + new string(MaskChar, input.Length - 1);
    }
}
