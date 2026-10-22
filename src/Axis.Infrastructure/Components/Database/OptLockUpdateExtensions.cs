using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using SqlSugar;

namespace Axis.Infrastructure.Components.Database;

/// <summary>
/// SqlSugar 乐观锁更新扩展。
/// </summary>
/// <remarks>
/// 要求实体版本字段标注 <c>IsEnableUpdateVersionValidation</c>（见 <see cref="Domain.BaseEntity.Version"/>）。
/// 仅适用于实体对象更新；<c>SetColumns</c> / <c>UpdateColumns</c> 不走本扩展。
/// </remarks>
public static class OptLockUpdateExtensions
{
    /// <summary>
    /// 带乐观锁的异步更新；版本不匹配时抛出业务冲突异常。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="updateable">已配置的更新构建器（可含 IgnoreColumns）。</param>
    /// <param name="cancellationToken">取消令牌（SqlSugar OptLock API 当前不转发，保留以统一调用签名）。</param>
    /// <exception cref="BusinessException">并发冲突时码为 <see cref="ResultCodes.Conflict"/>。</exception>
    public static async Task ExecuteWithOptLockAsync<T>(this IUpdateable<T> updateable,
        CancellationToken cancellationToken = default)
        where T : class, new()
    {
        ArgumentNullException.ThrowIfNull(updateable);
        _ = cancellationToken;

        try
        {
            // true：版本不一致时抛 VersionExceptions，由下方映射为统一 Conflict。
            await updateable.ExecuteCommandWithOptLockAsync(true);
        }
        catch (VersionExceptions)
        {
            throw new BusinessException(ResultCodes.Conflict, "数据已被他人修改，请刷新后重试");
        }
    }
}