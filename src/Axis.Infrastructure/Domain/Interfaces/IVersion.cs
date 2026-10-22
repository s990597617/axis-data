namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 乐观锁实体接口。
/// </summary>
/// <remarks>
/// 版本字段须配合 SqlSugar <c>IsEnableUpdateVersionValidation</c>，
/// 更新走 <c>ExecuteWithOptLockAsync</c> / <c>ExecuteCommandWithOptLockAsync</c>。
/// </remarks>
public interface IVersion
{
    /// <summary>
    /// 版本号（乐观锁令牌）。
    /// </summary>
    long Version { get; set; }
}
