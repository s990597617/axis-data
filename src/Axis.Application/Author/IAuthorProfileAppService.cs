using Axis.Application.Author.Dto;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;

namespace Axis.Application.Author;

/// <summary>
/// 登录后用户资料与菜单装配服务契约。
/// </summary>
/// <remarks>
/// 与 <see cref="IAuthorAppService"/> 分离：后者只管会话令牌与验证码，本接口负责当前用户资料下发。
/// </remarks>
public interface IAuthorProfileAppService
{
    /// <summary>
    /// 装配当前登录用户的完整资料（机构、角色、按钮权限、菜单树）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>供前端初始化路由与权限的用户信息。</returns>
    /// <exception cref="BusinessException">未登录或用户不存在时抛出未授权错误。</exception>
    Task<LoginUserInfo> GetUserInfoAsync(CancellationToken ct = default);
}
