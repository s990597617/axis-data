using Axis.Application.System.Profile.Dto;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Profile;

/// <summary>
/// 个人中心应用服务契约。
/// </summary>
public interface IProfileAppService
{
    /// <summary>
    /// 获取当前登录用户的个人中心资料。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>个人中心资料。</returns>
    /// <exception cref="BusinessException">未登录或用户不存在时抛出。</exception>
    Task<ProfileDto> GetAsync(CancellationToken ct = default);

    /// <summary>
    /// 更新当前登录用户的基础资料。
    /// </summary>
    /// <param name="input">可编辑字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">未登录或用户不存在时抛出。</exception>
    Task UpdateAsync(UpdateProfileInput input, CancellationToken ct = default);

    /// <summary>
    /// 修改当前登录用户密码。
    /// </summary>
    /// <param name="input">旧密码与新密码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">未登录、用户不存在或旧密码错误时抛出。</exception>
    Task ChangePasswordAsync(ChangePasswordInput input, CancellationToken ct = default);
}
