using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Domain.Interfaces;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     系统用户
/// </summary>
[SugarTable("sys_user", "系统用户")]
[SysTable]
[SugarIndex("uk_{table}_account", nameof(Account), OrderByType.Asc, true)]
[SugarIndex("index_{table}_phone", nameof(Phone), OrderByType.Asc)]
public class SysUser : BaseEntity, IUserDataPermission, ITenantId
{
    /// <summary>
    ///     租户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    ///     直属机构 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    ///     直属主管 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "直属主管Id")]
    public long? ManagerUserId { get; set; }

    /// <summary>
    ///     职位 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "职位Id")]
    public long PosId { get; set; }

    /// <summary>
    ///     账号
    /// </summary>
    [Required]
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "账号", Length = 32)]
    public string Account { get; set; } = null!;

    /// <summary>
    ///     密码（加密后的密文，仅服务端持久化，永不返回前端）
    /// </summary>
    [MaxLength(512)]
    [JsonIgnore]
    [SugarColumn(ColumnDescription = "密码", Length = 512)]
    public string Password { get; set; } = null!;

    /// <summary>
    ///     真实姓名
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "真实姓名", Length = 32)]
    public string RealName { get; set; } = null!;

    /// <summary>
    ///     昵称
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "昵称", Length = 32, IsNullable = true)]
    public string NickName { get; set; }

    /// <summary>
    ///     头像
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "头像", Length = 512, IsNullable = true)]
    public string Avatar { get; set; }

    /// <summary>
    ///     性别
    /// </summary>
    [SugarColumn(ColumnDescription = "性别")]
    public GenderEnum Sex { get; set; } = GenderEnum.Unknown;

    /// <summary>
    ///     出生日期
    /// </summary>
    [SugarColumn(ColumnDescription = "出生日期", IsNullable = true)]
    public DateTime Birthday { get; set; }

    /// <summary>
    ///     手机号
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "手机号", Length = 16, IsNullable = true)]
    public string Phone { get; set; }

    /// <summary>
    ///     邮箱
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "邮箱", Length = 64, IsNullable = true)]
    public string Email { get; set; }

    /// <summary>
    ///     身份证号
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "身份证号", Length = 32, IsNullable = true)]
    public string IdCardNum { get; set; }

    /// <summary>
    ///     工号
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "工号", Length = 32, IsNullable = true)]
    public string JobNum { get; set; }

    /// <summary>
    ///     入职日期
    /// </summary>
    [SugarColumn(ColumnDescription = "入职日期", IsNullable = true)]
    public DateTime? JoinDate { get; set; }

    /// <summary>
    ///     账号类型
    /// </summary>
    [SugarColumn(ColumnDescription = "账号类型")]
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.User;

    /// <summary>
    ///     排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    ///     状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    ///     备注
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }

    /// <summary>
    ///     最近登录 IP
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "最近登录IP", Length = 64, IsNullable = true)]
    public string LastLoginIp { get; set; }

    /// <summary>
    ///     最近登录时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近登录时间", IsNullable = true)]
    public DateTime LastLoginTime { get; set; }

    /// <summary>
    ///     最近登录设备
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "最近登录设备", Length = 128, IsNullable = true)]
    public string LastLoginDevice { get; set; }
    
    /// <summary>
    /// 导航属性，获取用户组织机构
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(OrgId))]
    public SysOrg SysOrg { get; set; }
}