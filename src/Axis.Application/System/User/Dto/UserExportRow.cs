using MiniExcelLibs.Attributes;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 用户 Excel 导出行。
/// </summary>
public sealed class UserExportRow
{
    /// <summary>
    /// 机构编码。
    /// </summary>
    [ExcelColumnName("机构编码")]
    public string? OrgCode { get; set; }

    /// <summary>
    /// 机构名称。
    /// </summary>
    [ExcelColumnName("机构名称")]
    public string? OrgName { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    [ExcelColumnName("账号")]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名。
    /// </summary>
    [ExcelColumnName("姓名")]
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称。
    /// </summary>
    [ExcelColumnName("昵称")]
    public string? NickName { get; set; }

    /// <summary>
    /// 手机号。
    /// </summary>
    [ExcelColumnName("手机号")]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱。
    /// </summary>
    [ExcelColumnName("邮箱")]
    public string? Email { get; set; }

    /// <summary>
    /// 性别文案。
    /// </summary>
    [ExcelColumnName("性别")]
    public string Sex { get; set; } = null!;

    /// <summary>
    /// 工号。
    /// </summary>
    [ExcelColumnName("工号")]
    public string? JobNum { get; set; }

    /// <summary>
    /// 入职日期。
    /// </summary>
    [ExcelColumnName("入职日期")]
    public string? JoinDate { get; set; }

    /// <summary>
    /// 岗位编码。
    /// </summary>
    [ExcelColumnName("岗位编码")]
    public string? PosCode { get; set; }

    /// <summary>
    /// 状态文案。
    /// </summary>
    [ExcelColumnName("状态")]
    public string Status { get; set; } = null!;

    /// <summary>
    /// 排序号。
    /// </summary>
    [ExcelColumnName("排序")]
    public int OrderNo { get; set; }

    /// <summary>
    /// 角色编码，逗号分隔。
    /// </summary>
    [ExcelColumnName("角色编码")]
    public string? RoleCodes { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [ExcelColumnName("备注")]
    public string? Remark { get; set; }
}
