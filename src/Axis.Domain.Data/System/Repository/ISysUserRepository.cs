using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 用户仓储
/// </summary>
public interface ISysUserRepository : IRepository<SysUser>
{
    /// <summary>
    /// 按条件分页查询用户并携带组织机构导航（供列表投影机构名称）。
    /// </summary>
    /// <param name="page">页码，从 1 开始。</param>
    /// <param name="pageSize">每页条数。</param>
    /// <param name="orgId">按机构过滤；为空表示不过滤。</param>
    /// <param name="keyword">关键字；匹配账号、姓名、昵称、手机号；可为空。</param>
    /// <param name="status">按状态过滤；为空表示不过滤。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>含 <see cref="SysUser.SysOrg"/> 导航的用户实体分页结果。</returns>
    /// <remarks>数据权限由 SqlSugar 全局 QueryFilter 自动附加，无需在此传参。</remarks>
    Task<SqlSugarPagedList<SysUser>> PageWithOrgAsync(
        int page,
        int pageSize,
        long? orgId,
        string? keyword,
        StatusEnum? status,
        CancellationToken ct = default);

    /// <summary>
    /// 根据账号查找（可用状态过滤）
    /// </summary>
    Task<SysUser> FindByAccountAsync(string account, CancellationToken ct = default);

    /// <summary>
    /// 根据账号查找并携带组织机构信息（用于登录签发 Claim）。
    /// </summary>
    Task<SysUser?> FindByAccountWithOrgAsync(string account, CancellationToken ct = default);

    /// <summary>
    /// 根据用户 Id 查找并携带组织机构信息（忽略租户过滤，用于刷新令牌等跨租户场景）。
    /// </summary>
    Task<SysUser?> FindByIdWithOrgAsync(long userId, bool ignoreFilter = false, CancellationToken ct = default);

    /// <summary>
    /// 获取用户所属角色 Id 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetRoleIdsAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 更新最后登录信息（登录成功后调用）
    /// </summary>
    Task UpdateLastLoginAsync(long userId, string? ip, string? device, CancellationToken ct = default);
}
