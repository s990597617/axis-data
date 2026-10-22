namespace Axis.Domain.System;

/// <summary>
/// 用户账号相关默认值。
/// </summary>
/// <remarks>
/// 新建用户不由前端传密码，统一使用 <see cref="InitialPassword"/>；
/// 与种子用户初始密码保持一致，首次登录后应通过「重置密码」修改。
/// </remarks>
public static class UserAccountDefaults
{
    /// <summary>
    /// 新建用户与种子账号的初始明文密码。
    /// </summary>
    public const string InitialPassword = "admin123";
}
