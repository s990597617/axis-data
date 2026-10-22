namespace Axis.Domain.Mdm;

/// <summary>
/// 主数据域业务字典类型编码。
/// </summary>
/// <remarks>
/// 与 <c>SysDictTypeSeed</c> 对齐；前端与 AppService 必须引用本常量，禁止散落字符串。
/// </remarks>
public static class MdmDictCodes
{
    /// <summary>
    /// 物料计量单位。
    /// </summary>
    public const string MaterialUnit = "mdm_material_unit";

    /// <summary>
    /// 物料来源类型（外购 / 自制 / 委外）。
    /// </summary>
    /// <remarks>
    /// 与编码规则字典 <c>code_source_type</c> 无关，禁止混用。
    /// </remarks>
    public const string MaterialSource = "mdm_material_source";
}
