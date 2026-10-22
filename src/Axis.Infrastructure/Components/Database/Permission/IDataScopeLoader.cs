namespace Axis.Infrastructure.Components.Database.Permission;

/// <summary>
/// 加载当前登录用户的数据权限范围。
/// </summary>
/// <remarks>
/// 过滤器挂载由 <see cref="SqlSugarFilter"/> 完成；本接口只负责解析范围快照。
/// </remarks>
public interface IDataScopeLoader
{
    /// <summary>
    /// 解析当前用户数据范围；未登录时返回无限制快照。
    /// </summary>
    Task<DataScopeState> LoadAsync(CancellationToken ct = default);
}
