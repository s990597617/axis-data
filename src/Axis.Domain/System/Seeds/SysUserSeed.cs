using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     用户种子（模拟完整企业人员结构）。
///     <para>
///         所有账号默认密码：<see cref="UserAccountDefaults.InitialPassword"/>（BCrypt 哈希值相同）。
///         <b>首次登录请立即修改密码。</b>
///     </para>
///     <para>
///         说明：生产环境请在首次运行后关闭 <c>AxisDbSettings.EnableInitSeed</c>，
///         避免每次启动重置密码。
///     </para>
/// </summary>
public sealed class SysUserSeed : ISqlSugarEntitySeedData<SysUser>
{
    /// <summary>
    /// admin123 的 BCrypt 哈希值
    /// </summary>
    private const string DefaultPasswordHash = "$2a$11$Av3nO90I0Xzd0JbeTpAf7.l0gHXr0XNAGWaDI33SgLIZqBP.KWj26";

    public IEnumerable<SysUser> HasData()
    {
        // 超级管理员
        yield return User(SysSeedIds.UserAdmin, SysSeedIds.OrgRoot, SysSeedIds.PosCeo, null,
            "admin", "系统管理员", "Admin", GenderEnum.Male, AccountTypeEnum.SuperAdmin,
            "13800000001", "admin@axis-tech.cn", "A001", 1,
            "平台超级管理员，拥有所有权限");

        // 张三 - 技术总监
        yield return User(SysSeedIds.UserZhangSan, SysSeedIds.OrgDevDept, SysSeedIds.PosCto, SysSeedIds.UserAdmin,
            "zhangsan", "张三", "三哥", GenderEnum.Male, AccountTypeEnum.SysAdmin,
            "13800000002", "zhangsan@axis-tech.cn", "D001", 2,
            "研发部技术总监，负责技术架构决策");

        // 李四 - 后端开发
        yield return User(SysSeedIds.UserLiSi, SysSeedIds.OrgBackendTeam, SysSeedIds.PosDeveloper, SysSeedIds.UserZhangSan,
            "lisi", "李四", "老李", GenderEnum.Male, AccountTypeEnum.User,
            "13800000003", "lisi@axis-tech.cn", "D002", 3,
            "后端开发组高级工程师");

        // 王五 - 前端开发
        yield return User(SysSeedIds.UserWangWu, SysSeedIds.OrgFrontendTeam, SysSeedIds.PosDeveloper, SysSeedIds.UserZhangSan,
            "wangwu", "王五", "小王", GenderEnum.Female, AccountTypeEnum.User,
            "13800000004", "wangwu@axis-tech.cn", "D003", 4,
            "前端开发组工程师，擅长 Vue 技术栈");

        // 赵六 - 产品经理
        yield return User(SysSeedIds.UserZhaoLiu, SysSeedIds.OrgProdDept, SysSeedIds.PosDesigner, SysSeedIds.UserAdmin,
            "zhaoliu", "赵六", "六哥", GenderEnum.Male, AccountTypeEnum.User,
            "13800000005", "zhaoliu@axis-tech.cn", "P001", 5,
            "产品部负责人，主导产品规划与迭代");

        // 孙七 - 人事专员
        yield return User(SysSeedIds.UserSunQi, SysSeedIds.OrgHrDept, SysSeedIds.PosHr, SysSeedIds.UserAdmin,
            "sunqi", "孙七", "小孙", GenderEnum.Female, AccountTypeEnum.User,
            "13800000006", "sunqi@axis-tech.cn", "H001", 6,
            "人力资源部专员，负责招聘与员工关系");
    }

    private static SysUser User(long id, long orgId, long posId, long? managerId,
        string account, string realName, string nickName, GenderEnum sex, AccountTypeEnum accountType,
        string phone, string email, string jobNum, int order, string remark)
    {
        return new SysUser
        {
            Id = id,
            TenantId = SysSeedIds.TenantDefault,
            OrgId = orgId,
            PosId = posId,
            ManagerUserId = managerId,
            Account = account,
            Password = DefaultPasswordHash,
            RealName = realName,
            NickName = nickName,
            Sex = sex,
            AccountType = accountType,
            Phone = phone,
            Email = email,
            JobNum = jobNum,
            JoinDate = new DateTime(2024, 3, 1),
            OrderNo = order,
            Status = StatusEnum.Enable,
            Remark = remark,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}