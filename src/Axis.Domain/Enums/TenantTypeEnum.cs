namespace Axis.Domain.Enums;

/// <summary>
/// 多租户隔离类型
/// </summary>
public enum TenantTypeEnum
{
    /// <summary>
    /// 字段隔离（共享库 + TenantId 过滤）
    /// </summary>
    Id = 1,

    /// <summary>
    /// Schema 隔离（已停用，保留枚举值兼容历史数据，不可新建/选择）
    /// </summary>
    [Obsolete("Schema 隔离已停用，请使用 Id 或 Db。")]
    Schema = 2,

    /// <summary>
    /// 独立数据库
    /// </summary>
    Db = 3,
}
