namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 按前缀查询缓存键。
/// </summary>
public sealed class CacheKeyQuery
{
    /// <summary>
    /// 键前缀；必填。传 <c>*</c> 表示扫描全部已知根前缀。
    /// </summary>
    public string Prefix { get; set; } = "*";

    /// <summary>
    /// 返回上限，默认 200，最大 500。
    /// </summary>
    public int Take { get; set; } = 200;
}
