using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 开放接口身份仓储实现。
/// </summary>
public sealed class SysOpenApiIdentityRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysOpenApiIdentity>(db, sp), ISysOpenApiIdentityRepository, IAutoInject
{
    /// <inheritdoc />
    public Task<SysOpenApiIdentity?> FindEnabledByAccessKeyAsync(string accessKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accessKey))
            return Task.FromResult<SysOpenApiIdentity?>(null);

        return Context.Queryable<SysOpenApiIdentity>()
            .ClearFilter()
            .Where(item => item.AccessKey == accessKey && item.Status == StatusEnum.Enable && !item.Deleted)
            .FirstAsync(ct)!;
    }
}
