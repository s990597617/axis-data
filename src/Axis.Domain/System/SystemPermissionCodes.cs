using Axis.Infrastructure.Permissions;

namespace Axis.Domain.System;

/// <summary>
/// 系统管理模块权限码。
/// </summary>
/// <remarks>
/// 权限码由菜单按钮种子、接口鉴权和前端按钮显隐共同使用。
/// 命名统一采用 <c>sys:模块:动作</c>；带 <see cref="PermissionModuleAttribute"/> /
/// <see cref="PermissionActionAttribute"/> 的常量会进入 <see cref="PermissionCatalog"/> 供管理端选择。
/// </remarks>
public static class SystemPermissionCodes
{
    /// <summary>
    /// 用户管理权限。
    /// </summary>
    [PermissionModule("user", "用户管理", Order = 10)]
    public static class User
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:user:query";

        [PermissionAction("详情", Order = 2)]
        public const string Detail = "sys:user:detail";

        [PermissionAction("新增", Order = 3)]
        public const string Create = "sys:user:create";

        [PermissionAction("编辑", Order = 4)]
        public const string Edit = "sys:user:edit";

        [PermissionAction("删除", Order = 5)]
        public const string Delete = "sys:user:delete";

        [PermissionAction("重置密码", Order = 6)]
        public const string ResetPassword = "sys:user:resetPwd";

        [PermissionAction("导入", Order = 7)]
        public const string Import = "sys:user:import";

        [PermissionAction("导出", Order = 8)]
        public const string Export = "sys:user:export";
    }

    /// <summary>
    /// 角色管理权限。
    /// </summary>
    [PermissionModule("role", "角色管理", Order = 20)]
    public static class Role
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:role:query";

        [PermissionAction("详情", Order = 2)]
        public const string Detail = "sys:role:detail";

        [PermissionAction("新增", Order = 3)]
        public const string Create = "sys:role:create";

        [PermissionAction("编辑", Order = 4)]
        public const string Edit = "sys:role:edit";

        [PermissionAction("删除", Order = 5)]
        public const string Delete = "sys:role:delete";

        [PermissionAction("分配权限", Order = 6)]
        public const string Grant = "sys:role:grant";
    }

    /// <summary>
    /// 菜单管理权限。
    /// </summary>
    [PermissionModule("menu", "菜单管理", Order = 30)]
    public static class Menu
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:menu:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:menu:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:menu:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:menu:delete";
    }

    /// <summary>
    /// 机构管理权限。
    /// </summary>
    [PermissionModule("org", "机构管理", Order = 40)]
    public static class Org
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:org:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:org:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:org:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:org:delete";
    }

    /// <summary>
    /// 岗位管理权限。
    /// </summary>
    [PermissionModule("pos", "岗位管理", Order = 50)]
    public static class Position
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:pos:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:pos:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:pos:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:pos:delete";
    }

    /// <summary>
    /// 字典管理权限。
    /// </summary>
    [PermissionModule("dict", "字典管理", Order = 60)]
    public static class Dict
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:dict:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:dict:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:dict:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:dict:delete";

        [PermissionAction("查询字典项", Order = 5)]
        public const string DataQuery = "sys:dict:data:query";

        [PermissionAction("新增字典项", Order = 6)]
        public const string DataCreate = "sys:dict:data:create";

        [PermissionAction("编辑字典项", Order = 7)]
        public const string DataEdit = "sys:dict:data:edit";

        [PermissionAction("删除字典项", Order = 8)]
        public const string DataDelete = "sys:dict:data:delete";
    }

    /// <summary>
    /// 参数配置权限。
    /// </summary>
    [PermissionModule("config", "参数配置", Order = 70)]
    public static class Config
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:config:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:config:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:config:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:config:delete";
    }

    /// <summary>
    /// 编码规则权限。
    /// </summary>
    [PermissionModule("codeRule", "编码规则", Order = 75)]
    public static class CodeRule
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:codeRule:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:codeRule:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:codeRule:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:codeRule:delete";

        [PermissionAction("预览", Order = 5)]
        public const string Preview = "sys:codeRule:preview";
    }

    /// <summary>
    /// 租户管理权限。
    /// </summary>
    [PermissionModule("tenant", "租户管理", Order = 80)]
    public static class Tenant
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:tenant:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:tenant:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:tenant:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:tenant:delete";

        [PermissionAction("分配菜单", Order = 5)]
        public const string Grant = "sys:tenant:grant";
    }

    /// <summary>
    /// 个人中心权限。
    /// </summary>
    [PermissionModule("profile", "个人中心", Order = 85)]
    public static class Profile
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:profile:query";

        [PermissionAction("更新资料", Order = 2)]
        public const string Update = "sys:profile:update";

        [PermissionAction("修改密码", Order = 3)]
        public const string ChangePassword = "sys:profile:changePwd";
    }

    /// <summary>
    /// 登录日志权限。
    /// </summary>
    [PermissionModule("logLogin", "登录日志", Order = 90)]
    public static class LogLogin
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:logLogin:query";

        [PermissionAction("清空", Order = 2)]
        public const string Clear = "sys:logLogin:clear";
    }

    /// <summary>
    /// 访问日志权限。
    /// </summary>
    [PermissionModule("logVisit", "访问日志", Order = 91)]
    public static class LogVisit
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:logVisit:query";

        [PermissionAction("清空", Order = 2)]
        public const string Clear = "sys:logVisit:clear";
    }

    /// <summary>
    /// 操作日志权限。
    /// </summary>
    [PermissionModule("logOperate", "操作日志", Order = 92)]
    public static class LogOperate
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:logOperate:query";

        [PermissionAction("清空", Order = 2)]
        public const string Clear = "sys:logOperate:clear";
    }

    /// <summary>
    /// 异常日志权限。
    /// </summary>
    [PermissionModule("logError", "异常日志", Order = 93)]
    public static class LogError
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:logError:query";

        [PermissionAction("清空", Order = 2)]
        public const string Clear = "sys:logError:clear";
    }

    /// <summary>
    /// 在线用户权限。
    /// </summary>
    [PermissionModule("online", "在线用户", Order = 99)]
    public static class Online
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:online:query";

        [PermissionAction("强制下线", Order = 2)]
        public const string Kick = "sys:online:kick";
    }

    /// <summary>
    /// 文件管理权限。
    /// </summary>
    [PermissionModule("file", "文件管理", Order = 98)]
    public static class File
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:file:query";

        [PermissionAction("上传", Order = 2)]
        public const string Upload = "sys:file:upload";

        [PermissionAction("下载", Order = 3)]
        public const string Download = "sys:file:download";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:file:delete";
    }

    /// <summary>
    /// 服务监控权限。
    /// </summary>
    [PermissionModule("monitor", "服务监控", Order = 100)]
    public static class Monitor
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:monitor:query";
    }

    /// <summary>
    /// 缓存管理权限。
    /// </summary>
    [PermissionModule("cache", "缓存管理", Order = 101)]
    public static class Cache
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:cache:query";

        [PermissionAction("清理", Order = 2)]
        public const string Clear = "sys:cache:clear";
    }

    /// <summary>
    /// 定时任务权限。
    /// </summary>
    [PermissionModule("job", "定时任务", Order = 102)]
    public static class Job
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:job:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:job:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:job:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:job:delete";

        [PermissionAction("立即执行", Order = 5)]
        public const string Run = "sys:job:run";

        [PermissionAction("执行日志", Order = 6)]
        public const string Log = "sys:job:log";
    }

    /// <summary>
    /// 通知公告权限。
    /// </summary>
    [PermissionModule("notice", "通知公告", Order = 103)]
    public static class Notice
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:notice:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:notice:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:notice:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:notice:delete";

        [PermissionAction("发布", Order = 5)]
        public const string Publish = "sys:notice:publish";
    }

    /// <summary>
    /// 我的消息（个人收件箱）权限。
    /// </summary>
    [PermissionModule("inbox", "我的消息", Order = 86)]
    public static class Inbox
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:inbox:query";

        [PermissionAction("标记已读", Order = 2)]
        public const string Read = "sys:inbox:read";

        [PermissionAction("全部已读", Order = 3)]
        public const string ReadAll = "sys:inbox:readAll";
    }

    /// <summary>
    /// 代码生成权限（平台专属，生产默认关闭）。
    /// </summary>
    [PermissionModule("codeGen", "代码生成", Order = 105)]
    public static class CodeGen
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:codeGen:query";

        [PermissionAction("预览", Order = 2)]
        public const string Preview = "sys:codeGen:preview";

        [PermissionAction("生成", Order = 3)]
        public const string Generate = "sys:codeGen:generate";
    }

    /// <summary>
    /// 开放接口身份权限。
    /// </summary>
    [PermissionModule("openApi", "开放接口", Order = 104)]
    public static class OpenApiIdentity
    {
        [PermissionAction("查询", Order = 1)]
        public const string Query = "sys:openApi:query";

        [PermissionAction("新增", Order = 2)]
        public const string Create = "sys:openApi:create";

        [PermissionAction("编辑", Order = 3)]
        public const string Edit = "sys:openApi:edit";

        [PermissionAction("删除", Order = 4)]
        public const string Delete = "sys:openApi:delete";

        [PermissionAction("重置密钥", Order = 5)]
        public const string ResetSecret = "sys:openApi:resetSecret";
    }
}
