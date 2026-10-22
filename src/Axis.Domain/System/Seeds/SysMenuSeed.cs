using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
/// 默认菜单种子：工作台 + 个人空间 + 基础管理（可分租户）+ 系统管理（平台专属）。
/// </summary>
public sealed class SysMenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
        // 工作台：原静态路由，纳入菜单管理后按角色/租户授权
        yield return Menu(SysSeedIds.MenuHome, 0, MenuTypeEnum.Menu,
            "工作台", "Home", "/home", "modules/home/views/index", null, "ant-design:dashboard-outlined", 0);

        // 个人空间：面向当前登录用户
        yield return Menu(SysSeedIds.MenuPersonal, 0, MenuTypeEnum.Dir,
            "个人空间", "Personal", "/personal",
            icon: "ant-design:user-outlined", order: 1);

        yield return Menu(SysSeedIds.MenuProfile, SysSeedIds.MenuPersonal, MenuTypeEnum.Menu,
            "个人中心", "SysProfile", "/personal/profile", "modules/personal/views/profile/index", "sys:profile", "ant-design:idcard-outlined", 1);

        yield return Menu(SysSeedIds.MenuInbox, SysSeedIds.MenuPersonal, MenuTypeEnum.Menu,
            "我的消息", "SysInbox", "/personal/inbox", "modules/personal/views/inbox/index", "sys:inbox", "ant-design:bell-outlined", 2);

        // 基础管理：可分配给租户的业务菜单
        yield return Menu(SysSeedIds.MenuBase, 0, MenuTypeEnum.Dir,
            "基础管理", "Base", "/base",
            icon: "ant-design:appstore-outlined", order: 2);

        yield return Menu(SysSeedIds.MenuUser, SysSeedIds.MenuBase, MenuTypeEnum.Menu,
            "用户管理", "SysUser", "/base/user", "modules/system/views/user/index", "sys:user", "ant-design:user-outlined", 1);

        yield return Menu(SysSeedIds.MenuRole, SysSeedIds.MenuBase, MenuTypeEnum.Menu,
            "角色管理", "SysRole", "/base/role", "modules/system/views/role/index", "sys:role", "ant-design:team-outlined", 2);

        yield return Menu(SysSeedIds.MenuOrg, SysSeedIds.MenuBase, MenuTypeEnum.Menu,
            "机构管理", "SysOrg", "/base/org", "modules/system/views/org/index", "sys:org", "ant-design:apartment-outlined", 3);

        yield return Menu(SysSeedIds.MenuPos, SysSeedIds.MenuBase, MenuTypeEnum.Menu,
            "岗位管理", "SysPos", "/base/pos", "modules/system/views/pos/index", "sys:pos", "ant-design:idcard-outlined", 4);

        yield return Menu(SysSeedIds.MenuNotice, SysSeedIds.MenuBase, MenuTypeEnum.Menu,
            "通知公告", "SysNotice", "/base/notice", "modules/system/views/notice/index", "sys:notice", "ant-design:notification-outlined", 5);

        // 系统管理：平台运维，整棵子树 IsPlatform，不可纳入租户启用集
        yield return Menu(SysSeedIds.MenuSystem, 0, MenuTypeEnum.Dir,
            "系统管理", "System", "/system",
            icon: "ant-design:setting-outlined", order: 3, isPlatform: true);

        yield return Menu(SysSeedIds.MenuMenu, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "菜单管理", "SysMenu", "/system/menu", "modules/system/views/menu/index", "sys:menu", "ant-design:menu-outlined", 1, isPlatform: true);

        yield return Menu(SysSeedIds.MenuDict, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "字典管理", "SysDict", "/system/dict", "modules/system/views/dict/index", "sys:dict", "ant-design:book-outlined", 2, isPlatform: true);

        yield return Menu(SysSeedIds.MenuConfig, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "参数配置", "SysConfig", "/system/config", "modules/system/views/config/index", "sys:config", "ant-design:tool-outlined", 3, isPlatform: true);

        yield return Menu(SysSeedIds.MenuCodeRule, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "编码规则", "SysCodeRule", "/system/code-rule", "modules/system/views/code-rule/index", "sys:codeRule", "ant-design:barcode-outlined", 4, isPlatform: true);

        yield return Menu(SysSeedIds.MenuTenant, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "租户管理", "SysTenant", "/system/tenant", "modules/system/views/tenant/index", "sys:tenant", "ant-design:cloud-outlined", 5,
            isPlatform: true);

        yield return Menu(SysSeedIds.MenuLog, SysSeedIds.MenuSystem, MenuTypeEnum.Dir,
            "日志管理", "SysLog", "/system/log",
            icon: "ant-design:file-search-outlined", order: 5, isPlatform: true);

        yield return Menu(SysSeedIds.MenuLogLogin, SysSeedIds.MenuLog, MenuTypeEnum.Menu,
            "登录日志", "SysLogLogin", "/system/log/login", "modules/system/views/log/login/index", "sys:logLogin", "ant-design:login-outlined", 1, isPlatform: true);

        yield return Menu(SysSeedIds.MenuLogVisit, SysSeedIds.MenuLog, MenuTypeEnum.Menu,
            "访问日志", "SysLogVisit", "/system/log/visit", "modules/system/views/log/visit/index", "sys:logVisit", "ant-design:eye-outlined", 2, isPlatform: true);

        yield return Menu(SysSeedIds.MenuLogOperate, SysSeedIds.MenuLog, MenuTypeEnum.Menu,
            "操作日志", "SysLogOperate", "/system/log/operate", "modules/system/views/log/operate/index", "sys:logOperate", "ant-design:audit-outlined", 3, isPlatform: true);

        yield return Menu(SysSeedIds.MenuLogError, SysSeedIds.MenuLog, MenuTypeEnum.Menu,
            "异常日志", "SysLogError", "/system/log/error", "modules/system/views/log/error/index", "sys:logError", "ant-design:bug-outlined", 4, isPlatform: true);

        yield return Menu(SysSeedIds.MenuOnline, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "在线用户", "SysOnline", "/system/online", "modules/system/views/online/index", "sys:online", "ant-design:team-outlined", 6, isPlatform: true);

        yield return Menu(SysSeedIds.MenuMonitor, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "服务监控", "SysMonitor", "/system/monitor", "modules/system/views/monitor/index", "sys:monitor", "ant-design:dashboard-outlined", 7, isPlatform: true);

        yield return Menu(SysSeedIds.MenuFile, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "文件管理", "SysFile", "/system/file", "modules/system/views/file/index", "sys:file", "ant-design:folder-open-outlined", 8, isPlatform: true);

        yield return Menu(SysSeedIds.MenuCache, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "缓存管理", "SysCache", "/system/cache", "modules/system/views/cache/index", "sys:cache", "ant-design:cloud-server-outlined", 9, isPlatform: true);

        yield return Menu(SysSeedIds.MenuJob, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "定时任务", "SysJob", "/system/job", "modules/system/views/job/index", "sys:job", "ant-design:schedule-outlined", 10, isPlatform: true);

        yield return Menu(SysSeedIds.MenuCodeGen, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "代码生成", "SysCodeGen", "/system/code-gen", "modules/system/views/code-gen/index", "sys:codeGen", "ant-design:code-outlined", 11, isPlatform: true);

        yield return Menu(SysSeedIds.MenuOpenApi, SysSeedIds.MenuSystem, MenuTypeEnum.Menu,
            "开放接口", "SysOpenApi", "/system/open-api", "modules/system/views/open-api/index", "sys:openApi", "ant-design:api-outlined", 12, isPlatform: true);

        foreach (var button in BuildButtons())
            yield return button;
    }

    private static SysMenu Menu(
        long id, long parentId, MenuTypeEnum type, string title, string name,
        string path, string component = null, string permission = null,
        string icon = null, int order = 100, bool isPlatform = false)
    {
        return new SysMenu
        {
            Id = id,
            ParentId = parentId,
            Type = type,
            Title = title,
            Name = name,
            Path = path,
            Component = component,
            Permission = permission,
            Icon = icon,
            OrderNo = order,
            IsPlatform = isPlatform,
            Status = StatusEnum.Enable,
            IsKeepAlive = true,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }

    /// <summary>
    /// 创建各页面对应的按钮权限节点。
    /// </summary>
    private static IEnumerable<SysMenu> BuildButtons()
    {
        // —— 基础管理（可分租户）——
        yield return Button(SysSeedIds.ButtonUserQuery, SysSeedIds.MenuUser, "查询用户", SystemPermissionCodes.User.Query, 1);
        yield return Button(SysSeedIds.ButtonUserDetail, SysSeedIds.MenuUser, "用户详情", SystemPermissionCodes.User.Detail, 2);
        yield return Button(SysSeedIds.ButtonUserCreate, SysSeedIds.MenuUser, "新增用户", SystemPermissionCodes.User.Create, 3);
        yield return Button(SysSeedIds.ButtonUserEdit, SysSeedIds.MenuUser, "编辑用户", SystemPermissionCodes.User.Edit, 4);
        yield return Button(SysSeedIds.ButtonUserDelete, SysSeedIds.MenuUser, "删除用户", SystemPermissionCodes.User.Delete, 5);
        yield return Button(SysSeedIds.ButtonUserResetPassword, SysSeedIds.MenuUser, "重置密码", SystemPermissionCodes.User.ResetPassword, 6);
        yield return Button(SysSeedIds.ButtonUserImport, SysSeedIds.MenuUser, "导入用户", SystemPermissionCodes.User.Import, 7);
        yield return Button(SysSeedIds.ButtonUserExport, SysSeedIds.MenuUser, "导出用户", SystemPermissionCodes.User.Export, 8);

        yield return Button(SysSeedIds.ButtonRoleQuery, SysSeedIds.MenuRole, "查询角色", SystemPermissionCodes.Role.Query, 1);
        yield return Button(SysSeedIds.ButtonRoleDetail, SysSeedIds.MenuRole, "角色详情", SystemPermissionCodes.Role.Detail, 2);
        yield return Button(SysSeedIds.ButtonRoleCreate, SysSeedIds.MenuRole, "新增角色", SystemPermissionCodes.Role.Create, 3);
        yield return Button(SysSeedIds.ButtonRoleEdit, SysSeedIds.MenuRole, "编辑角色", SystemPermissionCodes.Role.Edit, 4);
        yield return Button(SysSeedIds.ButtonRoleDelete, SysSeedIds.MenuRole, "删除角色", SystemPermissionCodes.Role.Delete, 5);
        yield return Button(SysSeedIds.ButtonRoleGrant, SysSeedIds.MenuRole, "分配权限", SystemPermissionCodes.Role.Grant, 6);

        yield return Button(SysSeedIds.ButtonOrgQuery, SysSeedIds.MenuOrg, "查询机构", SystemPermissionCodes.Org.Query, 1);
        yield return Button(SysSeedIds.ButtonOrgCreate, SysSeedIds.MenuOrg, "新增机构", SystemPermissionCodes.Org.Create, 2);
        yield return Button(SysSeedIds.ButtonOrgEdit, SysSeedIds.MenuOrg, "编辑机构", SystemPermissionCodes.Org.Edit, 3);
        yield return Button(SysSeedIds.ButtonOrgDelete, SysSeedIds.MenuOrg, "删除机构", SystemPermissionCodes.Org.Delete, 4);

        yield return Button(SysSeedIds.ButtonPosQuery, SysSeedIds.MenuPos, "查询岗位", SystemPermissionCodes.Position.Query, 1);
        yield return Button(SysSeedIds.ButtonPosCreate, SysSeedIds.MenuPos, "新增岗位", SystemPermissionCodes.Position.Create, 2);
        yield return Button(SysSeedIds.ButtonPosEdit, SysSeedIds.MenuPos, "编辑岗位", SystemPermissionCodes.Position.Edit, 3);
        yield return Button(SysSeedIds.ButtonPosDelete, SysSeedIds.MenuPos, "删除岗位", SystemPermissionCodes.Position.Delete, 4);

        yield return Button(SysSeedIds.ButtonNoticeQuery, SysSeedIds.MenuNotice, "查询公告", SystemPermissionCodes.Notice.Query, 1);
        yield return Button(SysSeedIds.ButtonNoticeCreate, SysSeedIds.MenuNotice, "新增公告", SystemPermissionCodes.Notice.Create, 2);
        yield return Button(SysSeedIds.ButtonNoticeEdit, SysSeedIds.MenuNotice, "编辑公告", SystemPermissionCodes.Notice.Edit, 3);
        yield return Button(SysSeedIds.ButtonNoticeDelete, SysSeedIds.MenuNotice, "删除公告", SystemPermissionCodes.Notice.Delete, 4);
        yield return Button(SysSeedIds.ButtonNoticePublish, SysSeedIds.MenuNotice, "发布公告", SystemPermissionCodes.Notice.Publish, 5);

        // —— 个人空间 ——
        yield return Button(SysSeedIds.ButtonProfileQuery, SysSeedIds.MenuProfile, "查询个人资料", SystemPermissionCodes.Profile.Query, 1);
        yield return Button(SysSeedIds.ButtonProfileUpdate, SysSeedIds.MenuProfile, "更新个人资料", SystemPermissionCodes.Profile.Update, 2);
        yield return Button(SysSeedIds.ButtonProfileChangePassword, SysSeedIds.MenuProfile, "修改密码", SystemPermissionCodes.Profile.ChangePassword, 3);

        yield return Button(SysSeedIds.ButtonInboxQuery, SysSeedIds.MenuInbox, "查询我的消息", SystemPermissionCodes.Inbox.Query, 1);
        yield return Button(SysSeedIds.ButtonInboxRead, SysSeedIds.MenuInbox, "标记已读", SystemPermissionCodes.Inbox.Read, 2);
        yield return Button(SysSeedIds.ButtonInboxReadAll, SysSeedIds.MenuInbox, "全部已读", SystemPermissionCodes.Inbox.ReadAll, 3);

        // —— 系统管理（平台专属）——
        yield return Button(SysSeedIds.ButtonMenuQuery, SysSeedIds.MenuMenu, "查询菜单", SystemPermissionCodes.Menu.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonMenuCreate, SysSeedIds.MenuMenu, "新增菜单", SystemPermissionCodes.Menu.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonMenuEdit, SysSeedIds.MenuMenu, "编辑菜单", SystemPermissionCodes.Menu.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonMenuDelete, SysSeedIds.MenuMenu, "删除菜单", SystemPermissionCodes.Menu.Delete, 4, isPlatform: true);

        yield return Button(SysSeedIds.ButtonDictQuery, SysSeedIds.MenuDict, "查询字典", SystemPermissionCodes.Dict.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictCreate, SysSeedIds.MenuDict, "新增字典", SystemPermissionCodes.Dict.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictEdit, SysSeedIds.MenuDict, "编辑字典", SystemPermissionCodes.Dict.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictDelete, SysSeedIds.MenuDict, "删除字典", SystemPermissionCodes.Dict.Delete, 4, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictDataQuery, SysSeedIds.MenuDict, "查询字典项", SystemPermissionCodes.Dict.DataQuery, 5, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictDataCreate, SysSeedIds.MenuDict, "新增字典项", SystemPermissionCodes.Dict.DataCreate, 6, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictDataEdit, SysSeedIds.MenuDict, "编辑字典项", SystemPermissionCodes.Dict.DataEdit, 7, isPlatform: true);
        yield return Button(SysSeedIds.ButtonDictDataDelete, SysSeedIds.MenuDict, "删除字典项", SystemPermissionCodes.Dict.DataDelete, 8, isPlatform: true);

        yield return Button(SysSeedIds.ButtonCodeRuleQuery, SysSeedIds.MenuCodeRule, "查询编码规则", SystemPermissionCodes.CodeRule.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeRuleCreate, SysSeedIds.MenuCodeRule, "新增编码规则", SystemPermissionCodes.CodeRule.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeRuleEdit, SysSeedIds.MenuCodeRule, "编辑编码规则", SystemPermissionCodes.CodeRule.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeRuleDelete, SysSeedIds.MenuCodeRule, "删除编码规则", SystemPermissionCodes.CodeRule.Delete, 4, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeRulePreview, SysSeedIds.MenuCodeRule, "预览编码规则", SystemPermissionCodes.CodeRule.Preview, 5, isPlatform: true);

        yield return Button(SysSeedIds.ButtonConfigQuery, SysSeedIds.MenuConfig, "查询参数", SystemPermissionCodes.Config.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonConfigCreate, SysSeedIds.MenuConfig, "新增参数", SystemPermissionCodes.Config.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonConfigEdit, SysSeedIds.MenuConfig, "编辑参数", SystemPermissionCodes.Config.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonConfigDelete, SysSeedIds.MenuConfig, "删除参数", SystemPermissionCodes.Config.Delete, 4, isPlatform: true);

        yield return Button(SysSeedIds.ButtonTenantQuery, SysSeedIds.MenuTenant, "查询租户", SystemPermissionCodes.Tenant.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonTenantCreate, SysSeedIds.MenuTenant, "新增租户", SystemPermissionCodes.Tenant.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonTenantEdit, SysSeedIds.MenuTenant, "编辑租户", SystemPermissionCodes.Tenant.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonTenantDelete, SysSeedIds.MenuTenant, "删除租户", SystemPermissionCodes.Tenant.Delete, 4, isPlatform: true);
        yield return Button(SysSeedIds.ButtonTenantGrant, SysSeedIds.MenuTenant, "分配菜单", SystemPermissionCodes.Tenant.Grant, 5, isPlatform: true);

        yield return Button(SysSeedIds.ButtonLogLoginQuery, SysSeedIds.MenuLogLogin, "查询登录日志", SystemPermissionCodes.LogLogin.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonLogLoginClear, SysSeedIds.MenuLogLogin, "清空登录日志", SystemPermissionCodes.LogLogin.Clear, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonLogVisitQuery, SysSeedIds.MenuLogVisit, "查询访问日志", SystemPermissionCodes.LogVisit.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonLogVisitClear, SysSeedIds.MenuLogVisit, "清空访问日志", SystemPermissionCodes.LogVisit.Clear, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonLogOperateQuery, SysSeedIds.MenuLogOperate, "查询操作日志", SystemPermissionCodes.LogOperate.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonLogOperateClear, SysSeedIds.MenuLogOperate, "清空操作日志", SystemPermissionCodes.LogOperate.Clear, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonLogErrorQuery, SysSeedIds.MenuLogError, "查询异常日志", SystemPermissionCodes.LogError.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonLogErrorClear, SysSeedIds.MenuLogError, "清空异常日志", SystemPermissionCodes.LogError.Clear, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonOnlineQuery, SysSeedIds.MenuOnline, "查询在线用户", SystemPermissionCodes.Online.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonOnlineKick, SysSeedIds.MenuOnline, "强制下线", SystemPermissionCodes.Online.Kick, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonMonitorQuery, SysSeedIds.MenuMonitor, "查询服务监控", SystemPermissionCodes.Monitor.Query, 1, isPlatform: true);

        yield return Button(SysSeedIds.ButtonFileQuery, SysSeedIds.MenuFile, "查询文件", SystemPermissionCodes.File.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonFileUpload, SysSeedIds.MenuFile, "上传文件", SystemPermissionCodes.File.Upload, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonFileDownload, SysSeedIds.MenuFile, "下载文件", SystemPermissionCodes.File.Download, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonFileDelete, SysSeedIds.MenuFile, "删除文件", SystemPermissionCodes.File.Delete, 4, isPlatform: true);

        yield return Button(SysSeedIds.ButtonCacheQuery, SysSeedIds.MenuCache, "查询缓存", SystemPermissionCodes.Cache.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCacheClear, SysSeedIds.MenuCache, "清理缓存", SystemPermissionCodes.Cache.Clear, 2, isPlatform: true);

        yield return Button(SysSeedIds.ButtonJobQuery, SysSeedIds.MenuJob, "查询任务", SystemPermissionCodes.Job.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonJobCreate, SysSeedIds.MenuJob, "新增任务", SystemPermissionCodes.Job.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonJobEdit, SysSeedIds.MenuJob, "编辑任务", SystemPermissionCodes.Job.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonJobDelete, SysSeedIds.MenuJob, "删除任务", SystemPermissionCodes.Job.Delete, 4, isPlatform: true);
        yield return Button(SysSeedIds.ButtonJobRun, SysSeedIds.MenuJob, "立即执行", SystemPermissionCodes.Job.Run, 5, isPlatform: true);
        yield return Button(SysSeedIds.ButtonJobLog, SysSeedIds.MenuJob, "执行日志", SystemPermissionCodes.Job.Log, 6, isPlatform: true);

        yield return Button(SysSeedIds.ButtonCodeGenQuery, SysSeedIds.MenuCodeGen, "查询代码生成", SystemPermissionCodes.CodeGen.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeGenPreview, SysSeedIds.MenuCodeGen, "预览生成", SystemPermissionCodes.CodeGen.Preview, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonCodeGenGenerate, SysSeedIds.MenuCodeGen, "执行生成", SystemPermissionCodes.CodeGen.Generate, 3, isPlatform: true);

        yield return Button(SysSeedIds.ButtonOpenApiQuery, SysSeedIds.MenuOpenApi, "查询开放接口身份", SystemPermissionCodes.OpenApiIdentity.Query, 1, isPlatform: true);
        yield return Button(SysSeedIds.ButtonOpenApiCreate, SysSeedIds.MenuOpenApi, "新增开放接口身份", SystemPermissionCodes.OpenApiIdentity.Create, 2, isPlatform: true);
        yield return Button(SysSeedIds.ButtonOpenApiEdit, SysSeedIds.MenuOpenApi, "编辑开放接口身份", SystemPermissionCodes.OpenApiIdentity.Edit, 3, isPlatform: true);
        yield return Button(SysSeedIds.ButtonOpenApiDelete, SysSeedIds.MenuOpenApi, "删除开放接口身份", SystemPermissionCodes.OpenApiIdentity.Delete, 4, isPlatform: true);
        yield return Button(SysSeedIds.ButtonOpenApiResetSecret, SysSeedIds.MenuOpenApi, "重置密钥", SystemPermissionCodes.OpenApiIdentity.ResetSecret, 5, isPlatform: true);
    }

    /// <summary>
    /// 创建按钮类型菜单节点。
    /// </summary>
    private static SysMenu Button(long id, long parentId, string title, string permission, int order, bool isPlatform = false)
        => Menu(id, parentId, MenuTypeEnum.Btn, title, string.Empty, string.Empty, permission: permission, order: order, isPlatform: isPlatform);
}
