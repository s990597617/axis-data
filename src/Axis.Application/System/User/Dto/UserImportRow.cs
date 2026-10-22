using MiniExcelLibs.Attributes;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 用户 Excel 导入行（中文列头与模板一致）。
/// </summary>
public sealed class UserImportRow
{
    /// <summary>
    /// 机构编码；必填，对应 <c>SysOrg.Code</c>。
    /// </summary>
    [ExcelColumnName("机构编码")]
    public string? OrgCode { get; set; }

    /// <summary>
    /// 登录账号；必填。
    /// </summary>
    [ExcelColumnName("账号")]
    public string? Account { get; set; }

    /// <summary>
    /// 真实姓名；必填。
    /// </summary>
    [ExcelColumnName("姓名")]
    public string? RealName { get; set; }

    /// <summary>
    /// 昵称；可空，空则回填姓名。
    /// </summary>
    [ExcelColumnName("昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 手机号；可空。
    /// </summary>
    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱；可空。
    /// </summary>
    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    /// <summary>
    /// 性别：未知/男/女 或 0/1/2；可空，默认未知。
    /// </summary>
    [ExcelColumnName("性别")]
    public string? Sex { get; set; }

    /// <summary>
    /// 工号；可空。
    /// </summary>
    [ExcelColumnName("工号")]
    public string? JobNum { get; set; }

    /// <summary>
    /// 入职日期；可空。
    /// </summary>
    [ExcelColumnName("入职日期")]
    public string? JoinDate { get; set; }

    /// <summary>
    /// 岗位编码；可空，对应 <c>SysPos.Code</c>。
    /// </summary>
    [ExcelColumnName("岗位编码")]
    public string? PosCode { get; set; }

    /// <summary>
    /// 状态：启用/禁用 或 1/0；可空，默认启用。
    /// </summary>
    [ExcelColumnName("状态")]
    public string? Status { get; set; }

    /// <summary>
    /// 排序号；可空，默认 100。
    /// </summary>
    [ExcelColumnName("排序")]
    public string? OrderNo { get; set; }

    /// <summary>
    /// 角色编码，多个用英文逗号分隔；可空。
    /// </summary>
    [ExcelColumnName("角色编码")]
    public string? RoleCodes { get; set; }

    /// <summary>
    /// 备注；可空。
    /// </summary>
    [ExcelColumnName("备注")]
    public string? Remark { get; set; }
}
