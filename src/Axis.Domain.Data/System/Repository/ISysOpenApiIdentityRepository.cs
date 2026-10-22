using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 开放接口身份仓储：按 AccessKey 查找（鉴权场景忽略租户过滤）。
/// </summary>
public interface ISysOpenApiIdentityRepository : IRepository<SysOpenApiIdentity>
{
    /// <summary>
    /// 按 AccessKey 查找启用中的身份；忽略租户过滤器。
    /// </summary>
    /// <param name="accessKey">身份标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>匹配记录；不存在或未启用时为 null。</returns>
    Task<SysOpenApiIdentity?> FindEnabledByAccessKeyAsync(string accessKey, CancellationToken ct = default);
}
