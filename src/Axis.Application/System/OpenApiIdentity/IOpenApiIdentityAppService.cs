using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.OpenApiIdentity;

/// <summary>
/// 开放接口身份应用服务契约。
/// </summary>
public interface IOpenApiIdentityAppService
{
    /// <summary>
    /// 分页查询开放接口身份。
    /// </summary>
    Task<SqlSugarPagedList<OpenApiIdentityDto>> PageAsync(
        OpenApiIdentityPageQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// 创建身份；返回明文密钥（仅此一次）。
    /// </summary>
    Task<CreateOpenApiIdentityResult> CreateAsync(
        SaveOpenApiIdentityInput input,
        CancellationToken ct = default);

    /// <summary>
    /// 更新身份（不修改密钥）。
    /// </summary>
    Task UpdateAsync(long id, SaveOpenApiIdentityInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除身份。
    /// </summary>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 重置 AccessSecret，返回新明文密钥。
    /// </summary>
    Task<ResetOpenApiSecretResult> ResetSecretAsync(long id, CancellationToken ct = default);
}
