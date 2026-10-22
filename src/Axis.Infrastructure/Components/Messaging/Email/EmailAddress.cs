namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 邮件地址。
/// </summary>
public sealed class EmailAddress
{
    /// <summary>
    /// 邮箱地址。
    /// </summary>
    public required string Address { get; init; }

    /// <summary>
    /// 显示名；可空。
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// 从纯地址构造。
    /// </summary>
    public static EmailAddress From(string address) => new() { Address = address };

    /// <summary>
    /// 从地址与显示名构造。
    /// </summary>
    public static EmailAddress From(string address, string? displayName)
        => new() { Address = address, DisplayName = displayName };
}
