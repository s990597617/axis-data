using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Axis.Scaffold;

/// <summary>
/// 生成独立菜单种子类（EnableInitSeed 时由框架自动导入；不改 SysMenuSeed）。
/// </summary>
public static class MenuSeedWriter
{
    public static FileAction Write(string repoRoot, TokenContext tokens, bool dryRun, bool force)
    {
        var fileName = $"{tokens.Area}_{tokens.Module}MenuSeed.cs";
        var path = Path.Combine(
            repoRoot, "src", "Axis.Domain", "System", "Seeds", "Generated", fileName);
        var exists = File.Exists(path);
        if (exists && !force)
            return new FileAction(path, "SKIP", "菜单种子已存在（使用 --force 覆盖）");

        var content = Build(tokens);
        if (dryRun)
            return new FileAction(path, exists ? "OVERWRITE" : "CREATE", "dry-run · 菜单种子", Content: content);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        return new FileAction(path, exists ? "OVERWRITE" : "CREATE", "菜单种子", Content: content);
    }

    private static string Build(TokenContext tokens)
    {
        var isSystem = tokens.IsSystemArea;
        var (dirId, menuId, btnQuery, btnCreate, btnEdit, btnDelete) = AllocateIds(tokens.Area, tokens.Module);
        var parentExpr = isSystem
            ? "SysSeedIds.MenuSystem"
            : dirId.ToString(CultureInfo.InvariantCulture);
        var isPlatform = isSystem ? "true" : "false";
        var permType = tokens.PermissionCodesType;
        var permUsing = isSystem
            ? ""
            : $"using Axis.Domain.{tokens.Area};\r\n";
        var pathPrefix = isSystem ? "/system" : $"/{tokens.AreaLower}";
        var menuPath = $"{pathPrefix}/{tokens.RouteKey}";
        var component = $"modules/{tokens.AreaLower}/views/{tokens.ModuleKebab}/index";
        var routeName = tokens.Entity;
        var menuPerm = tokens.PermPrefix; // demo:sku / sys:widget
        var order = 100;

        // 非 System：同文件带上域目录，多次生成同一 Area 时 Id 稳定，Storageable 会 upsert。
        var dirBlock = isSystem
            ? ""
            : $$"""
        yield return Item({{dirId}}L, 0, MenuTypeEnum.Dir,
            "{{Escape(tokens.Area)}}", "{{tokens.Area}}", "{{pathPrefix}}",
            icon: "ant-design:appstore-outlined", order: 50, isPlatform: false);


""";

        return $$"""
using Axis.Domain.Enums;
{{permUsing}}using Axis.Domain.System.Seeds;

namespace Axis.Domain.System.Seeds.Generated;

/// <summary>
/// 脚手架生成：{{tokens.Area}}/{{tokens.Module}}（{{tokens.CnName}}）菜单与按钮种子。
/// </summary>
/// <remarks>
/// 仅在 EnableInitSeed=true 时导入。角色授权请在菜单管理中分配，或自行扩展 RoleMenu 种子。
/// </remarks>
public sealed class {{tokens.Area}}_{{tokens.Module}}MenuSeed : ISqlSugarEntitySeedData<SysMenu>
{
    public IEnumerable<SysMenu> HasData()
    {
{{dirBlock}}        yield return Item({{menuId}}L, {{parentExpr}}, MenuTypeEnum.Menu,
            "{{Escape(tokens.CnName)}}", "{{routeName}}", "{{menuPath}}", "{{component}}",
            "{{menuPerm}}", "ant-design:table-outlined", {{order}}, {{isPlatform}});

        yield return Item({{btnQuery}}L, {{menuId}}L, MenuTypeEnum.Btn,
            "查询{{Escape(tokens.CnName)}}", string.Empty, string.Empty,
            permission: {{permType}}.{{tokens.Module}}.Query, order: 1, isPlatform: {{isPlatform}});
        yield return Item({{btnCreate}}L, {{menuId}}L, MenuTypeEnum.Btn,
            "新增{{Escape(tokens.CnName)}}", string.Empty, string.Empty,
            permission: {{permType}}.{{tokens.Module}}.Create, order: 2, isPlatform: {{isPlatform}});
        yield return Item({{btnEdit}}L, {{menuId}}L, MenuTypeEnum.Btn,
            "编辑{{Escape(tokens.CnName)}}", string.Empty, string.Empty,
            permission: {{permType}}.{{tokens.Module}}.Edit, order: 3, isPlatform: {{isPlatform}});
        yield return Item({{btnDelete}}L, {{menuId}}L, MenuTypeEnum.Btn,
            "删除{{Escape(tokens.CnName)}}", string.Empty, string.Empty,
            permission: {{permType}}.{{tokens.Module}}.Delete, order: 4, isPlatform: {{isPlatform}});
    }

    private static SysMenu Item(
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
}

""";
    }

    /// <summary>
    /// 在 1000_0006_8xxx 段分配稳定 Id，避开 SysSeedIds 的 0000 段。
    /// </summary>
    private static (long Dir, long Menu, long BtnQ, long BtnC, long BtnE, long BtnD) AllocateIds(
        string area, string module)
    {
        var dirSlot = StableSlot(area, "dir", 1_000);
        var menuSlot = StableSlot(area + "." + module, "menu", 50_000) + 1_000;
        // 菜单基座：1000_0006_8000_00000 + slot*10
        const long baseId = 1000_0006_8000_00000L;
        var dirId = baseId + dirSlot * 10L;
        var menuId = baseId + menuSlot * 10L;
        return (dirId, menuId, menuId + 1, menuId + 2, menuId + 3, menuId + 4);
    }

    private static int StableSlot(string key, string salt, int mod)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(salt + ":" + key));
        var value = BitConverter.ToUInt32(bytes, 0);
        return (int)(value % (uint)mod);
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
