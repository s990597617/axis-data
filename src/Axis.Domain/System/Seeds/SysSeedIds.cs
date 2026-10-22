namespace Axis.Domain.System.Seeds;

/// <summary>
///     种子数据固定 Id 约定。
///     <para>
///         采用固定的 19 位数字（模拟雪花 Id 格式），避免与运行时生成的 Id 冲突。
///         约定段位：<c>1000_xxxx_xxxx_xxxxx</c>，其中 xxxx 为业务分区。
///     </para>
/// </summary>
internal static class SysSeedIds
{
    /// <summary>默认租户固定编号。</summary>
    public const long TenantDefault = 1000_0001_0000_00001L;

    /// <summary>机构种子固定编号。</summary>
    public const long OrgRoot = 1000_0002_0000_00001L;          // 集团总部
    public const long OrgTechCompany = 1000_0002_0000_00002L;   // 科技公司
    public const long OrgDevDept = 1000_0002_0000_00003L;       // 研发部
    public const long OrgProdDept = 1000_0002_0000_00004L;      // 产品部
    public const long OrgHrDept = 1000_0002_0000_00005L;        // 人力资源部
    public const long OrgFinDept = 1000_0002_0000_00006L;       // 财务部
    public const long OrgBackendTeam = 1000_0002_0000_00007L;   // 后端组
    public const long OrgFrontendTeam = 1000_0002_0000_00008L;  // 前端组
    public const long OrgQaTeam = 1000_0002_0000_00009L;        // 测试组

    /// <summary>岗位种子固定编号。</summary>
    public const long PosCeo = 1000_0003_0000_00001L;           // 总经理
    public const long PosCto = 1000_0003_0000_00002L;           // 技术总监
    public const long PosDevManager = 1000_0003_0000_00003L;    // 研发经理
    public const long PosDeveloper = 1000_0003_0000_00004L;     // 开发工程师
    public const long PosDesigner = 1000_0003_0000_00005L;      // 产品设计师
    public const long PosHr = 1000_0003_0000_00006L;            // 人事专员
    public const long PosFinance = 1000_0003_0000_00007L;       // 财务专员

    /// <summary>角色种子固定编号。</summary>
    public const long RoleSuperAdmin = 1000_0004_0000_00001L;   // 超级管理员
    public const long RoleSysAdmin = 1000_0004_0000_00002L;     // 系统管理员
    public const long RoleDevLead = 1000_0004_0000_00003L;      // 研发主管
    public const long RoleDeveloper = 1000_0004_0000_00004L;    // 开发人员
    public const long RoleViewer = 1000_0004_0000_00005L;       // 只读用户

    /// <summary>用户种子固定编号。</summary>
    public const long UserAdmin = 1000_0005_0000_00001L;        // admin 超管
    public const long UserZhangSan = 1000_0005_0000_00002L;     // 张三 技术总监
    public const long UserLiSi = 1000_0005_0000_00003L;         // 李四 后端开发
    public const long UserWangWu = 1000_0005_0000_00004L;       // 王五 前端开发
    public const long UserZhaoLiu = 1000_0005_0000_00005L;      // 赵六 产品经理
    public const long UserSunQi = 1000_0005_0000_00006L;        // 孙七 人事专员

    /// <summary>
    /// 工作台（顶级菜单，原静态路由）。
    /// </summary>
    public const long MenuHome = 1000_0006_0000_10900L;

    /// <summary>
    /// 系统管理目录及页面菜单固定编号（平台专属，不可分配给租户）。
    /// </summary>
    public const long MenuSystem = 1000_0006_0000_11000L;
    public const long MenuMenu = 1000_0006_0000_11030L;
    public const long MenuDict = 1000_0006_0000_11060L;
    public const long MenuConfig = 1000_0006_0000_11070L;
    public const long MenuCodeRule = 1000_0006_0000_11075L;
    public const long MenuTenant = 1000_0006_0000_11080L;
    public const long MenuLog = 1000_0006_0000_11100L;
    public const long MenuLogLogin = 1000_0006_0000_11110L;
    public const long MenuLogVisit = 1000_0006_0000_11120L;
    public const long MenuLogOperate = 1000_0006_0000_11130L;
    public const long MenuLogError = 1000_0006_0000_11140L;
    public const long MenuOnline = 1000_0006_0000_11145L;
    public const long MenuMonitor = 1000_0006_0000_11150L;
    public const long MenuFile = 1000_0006_0000_11155L;
    public const long MenuCache = 1000_0006_0000_11160L;
    public const long MenuJob = 1000_0006_0000_11170L;
    public const long MenuCodeGen = 1000_0006_0000_11186L;
    public const long MenuOpenApi = 1000_0006_0000_11190L;

    /// <summary>
    /// 个人空间目录及页面菜单固定编号。
    /// </summary>
    public const long MenuPersonal = 1000_0006_0000_11200L;
    public const long MenuProfile = 1000_0006_0000_11090L;
    public const long MenuInbox = 1000_0006_0000_11210L;

    /// <summary>
    /// 基础管理目录及页面菜单固定编号（可分配给租户）。
    /// </summary>
    public const long MenuBase = 1000_0006_0000_11300L;
    public const long MenuUser = 1000_0006_0000_11010L;
    public const long MenuRole = 1000_0006_0000_11020L;
    public const long MenuOrg = 1000_0006_0000_11040L;
    public const long MenuPos = 1000_0006_0000_11050L;
    public const long MenuNotice = 1000_0006_0000_11180L;

    /// <summary>
    /// 用户管理按钮固定编号。
    /// </summary>
    public const long ButtonUserQuery = 1000_0006_0000_11011L;
    public const long ButtonUserDetail = 1000_0006_0000_11012L;
    public const long ButtonUserCreate = 1000_0006_0000_11013L;
    public const long ButtonUserEdit = 1000_0006_0000_11014L;
    public const long ButtonUserDelete = 1000_0006_0000_11015L;
    public const long ButtonUserResetPassword = 1000_0006_0000_11016L;
    public const long ButtonUserImport = 1000_0006_0000_11017L;
    public const long ButtonUserExport = 1000_0006_0000_11018L;

    /// <summary>
    /// 七个系统管理页面的按钮固定编号。
    /// </summary>
    public const long ButtonRoleQuery = 1000_0006_0000_11021L;
    public const long ButtonRoleCreate = 1000_0006_0000_11022L;
    public const long ButtonRoleEdit = 1000_0006_0000_11023L;
    public const long ButtonRoleDelete = 1000_0006_0000_11024L;
    public const long ButtonRoleGrant = 1000_0006_0000_11025L;
    public const long ButtonRoleDetail = 1000_0006_0000_11026L;
    public const long ButtonMenuQuery = 1000_0006_0000_11031L;
    public const long ButtonMenuCreate = 1000_0006_0000_11032L;
    public const long ButtonMenuEdit = 1000_0006_0000_11033L;
    public const long ButtonMenuDelete = 1000_0006_0000_11034L;
    public const long ButtonOrgQuery = 1000_0006_0000_11041L;
    public const long ButtonOrgCreate = 1000_0006_0000_11042L;
    public const long ButtonOrgEdit = 1000_0006_0000_11043L;
    public const long ButtonOrgDelete = 1000_0006_0000_11044L;
    public const long ButtonPosQuery = 1000_0006_0000_11051L;
    public const long ButtonPosCreate = 1000_0006_0000_11052L;
    public const long ButtonPosEdit = 1000_0006_0000_11053L;
    public const long ButtonPosDelete = 1000_0006_0000_11054L;
    public const long ButtonDictQuery = 1000_0006_0000_11061L;
    public const long ButtonDictCreate = 1000_0006_0000_11062L;
    public const long ButtonDictEdit = 1000_0006_0000_11063L;
    public const long ButtonDictDelete = 1000_0006_0000_11064L;
    public const long ButtonDictDataQuery = 1000_0006_0000_11065L;
    public const long ButtonDictDataCreate = 1000_0006_0000_11066L;
    public const long ButtonDictDataEdit = 1000_0006_0000_11067L;
    public const long ButtonDictDataDelete = 1000_0006_0000_11068L;
    public const long ButtonConfigQuery = 1000_0006_0000_11071L;
    public const long ButtonConfigCreate = 1000_0006_0000_11072L;
    public const long ButtonConfigEdit = 1000_0006_0000_11073L;
    public const long ButtonConfigDelete = 1000_0006_0000_11074L;
    public const long ButtonCodeRuleQuery = 1000_0006_0000_11076L;
    public const long ButtonCodeRuleCreate = 1000_0006_0000_11077L;
    public const long ButtonCodeRuleEdit = 1000_0006_0000_11078L;
    public const long ButtonCodeRuleDelete = 1000_0006_0000_11079L;
    public const long ButtonCodeRulePreview = 1000_0006_0000_11196L;
    public const long ButtonTenantQuery = 1000_0006_0000_11081L;
    public const long ButtonTenantCreate = 1000_0006_0000_11082L;
    public const long ButtonTenantEdit = 1000_0006_0000_11083L;
    public const long ButtonTenantDelete = 1000_0006_0000_11084L;
    public const long ButtonTenantGrant = 1000_0006_0000_11085L;

    /// <summary>
    /// 默认租户菜单启用关系种子起始 Id（递增使用）。
    /// </summary>
    public const long TenantMenuBase = 1000_0011_0000_00000L;
    public const long ButtonProfileQuery = 1000_0006_0000_11091L;
    public const long ButtonProfileUpdate = 1000_0006_0000_11092L;
    public const long ButtonProfileChangePassword = 1000_0006_0000_11093L;
    public const long ButtonLogLoginQuery = 1000_0006_0000_11111L;
    public const long ButtonLogLoginClear = 1000_0006_0000_11112L;
    public const long ButtonLogVisitQuery = 1000_0006_0000_11121L;
    public const long ButtonLogVisitClear = 1000_0006_0000_11122L;
    public const long ButtonLogOperateQuery = 1000_0006_0000_11131L;
    public const long ButtonLogOperateClear = 1000_0006_0000_11132L;
    public const long ButtonLogErrorQuery = 1000_0006_0000_11141L;
    public const long ButtonLogErrorClear = 1000_0006_0000_11142L;
    public const long ButtonOnlineQuery = 1000_0006_0000_11146L;
    public const long ButtonOnlineKick = 1000_0006_0000_11147L;
    public const long ButtonMonitorQuery = 1000_0006_0000_11151L;
    public const long ButtonFileQuery = 1000_0006_0000_11156L;
    public const long ButtonFileUpload = 1000_0006_0000_11157L;
    public const long ButtonFileDownload = 1000_0006_0000_11158L;
    public const long ButtonFileDelete = 1000_0006_0000_11159L;
    public const long ButtonCacheQuery = 1000_0006_0000_11161L;
    public const long ButtonCacheClear = 1000_0006_0000_11162L;
    public const long ButtonJobQuery = 1000_0006_0000_11171L;
    public const long ButtonJobCreate = 1000_0006_0000_11172L;
    public const long ButtonJobEdit = 1000_0006_0000_11173L;
    public const long ButtonJobDelete = 1000_0006_0000_11174L;
    public const long ButtonJobRun = 1000_0006_0000_11175L;
    public const long ButtonJobLog = 1000_0006_0000_11176L;
    public const long ButtonNoticeQuery = 1000_0006_0000_11181L;
    public const long ButtonNoticeCreate = 1000_0006_0000_11182L;
    public const long ButtonNoticeEdit = 1000_0006_0000_11183L;
    public const long ButtonNoticeDelete = 1000_0006_0000_11184L;
    public const long ButtonNoticePublish = 1000_0006_0000_11185L;
    public const long ButtonCodeGenQuery = 1000_0006_0000_11187L;
    public const long ButtonCodeGenPreview = 1000_0006_0000_11188L;
    public const long ButtonCodeGenGenerate = 1000_0006_0000_11189L;
    public const long ButtonOpenApiQuery = 1000_0006_0000_11191L;
    public const long ButtonOpenApiCreate = 1000_0006_0000_11192L;
    public const long ButtonOpenApiEdit = 1000_0006_0000_11193L;
    public const long ButtonOpenApiDelete = 1000_0006_0000_11194L;
    public const long ButtonOpenApiResetSecret = 1000_0006_0000_11195L;
    public const long ButtonInboxQuery = 1000_0006_0000_11211L;
    public const long ButtonInboxRead = 1000_0006_0000_11212L;
    public const long ButtonInboxReadAll = 1000_0006_0000_11213L;

    /// <summary>定时任务种子固定编号。</summary>
    public const long JobHeartbeat = 1000_0011_0000_00001L;
    public const long JobCleanupRefreshToken = 1000_0011_0000_00002L;

    /// <summary>字典类型种子固定编号。</summary>
    public const long DictTypeStatus = 1000_0007_0000_00001L;
    public const long DictTypeGender = 1000_0007_0000_00002L;
    public const long DictTypeYesNo = 1000_0007_0000_00003L;
    public const long DictTypeAccountType = 1000_0007_0000_00004L;
    public const long DictTypeOrgType = 1000_0007_0000_00005L;
    public const long DictTypeCodeResetCycle = 1000_0007_0000_00006L;
    public const long DictTypeCodeCheckAlgo = 1000_0007_0000_00007L;
    public const long DictTypeCodeSourceType = 1000_0007_0000_00008L;
    public const long DictTypeMaterialUnit = 1000_0007_0000_00009L;
    public const long DictTypeMaterialSource = 1000_0007_0000_00010L;

    /// <summary>字典数据种子固定编号。</summary>
    // common_status 字典项
    public const long DictDataStatusEnable = 1000_0008_0001_00001L;
    public const long DictDataStatusDisable = 1000_0008_0001_00002L;
    // gender
    public const long DictDataGenderUnknown = 1000_0008_0002_00001L;
    public const long DictDataGenderMale = 1000_0008_0002_00002L;
    public const long DictDataGenderFemale = 1000_0008_0002_00003L;
    // yes_no
    public const long DictDataYesNoNo = 1000_0008_0003_00001L;
    public const long DictDataYesNoYes = 1000_0008_0003_00002L;
    // account_type
    public const long DictDataAccountUser = 1000_0008_0004_00001L;
    public const long DictDataAccountSysAdmin = 1000_0008_0004_00002L;
    public const long DictDataAccountSuperAdmin = 1000_0008_0004_00003L;
    // org_type
    public const long DictDataOrgGroup = 1000_0008_0005_00001L;
    public const long DictDataOrgCompany = 1000_0008_0005_00002L;
    public const long DictDataOrgDept = 1000_0008_0005_00003L;
    public const long DictDataOrgTeam = 1000_0008_0005_00004L;
    public const long DictDataResetNever = 1000_0008_0006_00001L;
    public const long DictDataResetDaily = 1000_0008_0006_00002L;
    public const long DictDataResetMonthly = 1000_0008_0006_00003L;
    public const long DictDataResetYearly = 1000_0008_0006_00004L;
    public const long DictDataCheckNone = 1000_0008_0007_00001L;
    public const long DictDataCheckMod10 = 1000_0008_0007_00002L;
    public const long DictDataSourceSelf = 1000_0008_0008_00001L;
    public const long DictDataSourceExternal = 1000_0008_0008_00002L;
    public const long DictDataUnitPcs = 1000_0008_0009_00001L;
    public const long DictDataUnitKg = 1000_0008_0009_00002L;
    public const long DictDataUnitM = 1000_0008_0009_00003L;
    public const long DictDataUnitSet = 1000_0008_0009_00004L;
    public const long DictDataUnitL = 1000_0008_0009_00005L;
    public const long DictDataMaterialSourceBuy = 1000_0008_0010_00001L;
    public const long DictDataMaterialSourceMake = 1000_0008_0010_00002L;
    public const long DictDataMaterialSourceOutsource = 1000_0008_0010_00003L;

    /// <summary>编码规则种子固定编号。</summary>
    public const long CodeRuleMat = 1000_0012_0000_00001L;
    public const long CodeRuleWo = 1000_0012_0000_00002L;
    public const long CodeRuleRpt = 1000_0012_0000_00003L;
    public const long CodeRuleIqc = 1000_0012_0000_00004L;
    public const long CodeRulePrc = 1000_0012_0000_00005L;
    public const long CodeRuleRtg = 1000_0012_0000_00006L;

    /// <summary>用户角色关联种子固定编号。</summary>
    public const long UserRoleAdmin = 1000_0009_0000_00001L;
    public const long UserRoleZhangSan = 1000_0009_0000_00002L;
    public const long UserRoleLiSi = 1000_0009_0000_00003L;
    public const long UserRoleWangWu = 1000_0009_0000_00004L;
    public const long UserRoleZhaoLiu = 1000_0009_0000_00005L;
    public const long UserRoleSunQi = 1000_0009_0000_00006L;

    /// <summary>角色菜单关联种子的起始编号。</summary>
    public const long RoleMenuBase = 1000_0010_0000_00000L;

    /// <summary>
    ///     种子数据的创建人 ID
    /// </summary>
    public const long SeedCreatorId = UserAdmin;

    /// <summary>
    ///     种子数据的创建人名称
    /// </summary>
    public const string SeedCreator = "admin";

    /// <summary>
    ///     种子数据的创建时间（固定值，保证 upsert 幂等）
    /// </summary>
    public static readonly DateTime SeedCreatedTime = new(2026, 1, 1, 8, 0, 0, DateTimeKind.Local);
}
