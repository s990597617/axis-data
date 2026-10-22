namespace Axis.Infrastructure.Permissions;

/// <summary>
/// 单条权限码的目录描述（由 <see cref="PermissionCatalog"/> 从常量扫描得到）。
/// </summary>
/// <param name="Code">权限码，如 <c>sys:config:create</c>。</param>
/// <param name="Module">模块键，如 <c>config</c>。</param>
/// <param name="ModuleName">模块中文名。</param>
/// <param name="ActionName">动作中文名。</param>
/// <param name="ModuleOrder">模块排序。</param>
/// <param name="ActionOrder">动作排序。</param>
public sealed record PermissionDescriptor(
    string Code,
    string Module,
    string ModuleName,
    string ActionName,
    int ModuleOrder,
    int ActionOrder);
