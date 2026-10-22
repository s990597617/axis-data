namespace Axis.Domain.Enums;

/// <summary>
/// 编码来源类型。
/// </summary>
/// <remarks>
/// 本厂生成走取号占用流水；外部传入只校验不占号。
/// 与字典 <c>code_source_type</c> 双轨对齐。
/// </remarks>
public enum CodeSourceTypeEnum
{
    /// <summary>
    /// 未知或未指定；不得用于已启用规则。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 本厂生成；<c>NextAsync</c> 递增流水后渲染模板。
    /// </summary>
    SelfGenerated = 1,

    /// <summary>
    /// 外部传入（如 ERP）；只能 <c>ValidateAsync</c>，禁止取号。
    /// </summary>
    External = 2
}
