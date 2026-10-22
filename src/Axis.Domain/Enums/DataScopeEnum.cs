namespace Axis.Domain.Enums;

/// <summary>
/// 数据范围（角色数据权限）
/// </summary>
public enum DataScopeEnum
{
    /// <summary>
    /// 全部数据
    /// </summary>
    All = 1,

    /// <summary>
    /// 自定义数据（按勾选机构）
    /// </summary>
    Custom = 2,

    /// <summary>
    /// 本部门及以下数据
    /// </summary>
    DeptWithChildren = 3,

    /// <summary>
    /// 本部门数据
    /// </summary>
    Dept = 4,

    /// <summary>
    /// 仅本人数据
    /// </summary>
    Self = 5,
}
