namespace Axis.Infrastructure.Components.Database.Enums;

/// <summary>
/// 数据权限枚举
/// </summary>
public enum DataPermissionEnum
{
    /// <summary>
    /// 无权限
    /// </summary>
    None = 0,

    /// <summary>
    /// 全部数据权限
    /// </summary>
    All = 1,

    /// <summary>
    /// 自定数据权限
    /// </summary>
    Custom = 2,

    /// <summary>
    /// 部门数据权限
    /// </summary>
    Dept = 3,

    /// <summary>
    /// 部门及以下数据权限
    /// </summary>
    DeptChild = 4,

    /// <summary>
    /// 仅本人数据权限
    /// </summary>
    Self = 5,
}