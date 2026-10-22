namespace Axis.Application.System.Menu.Dto;

/// <summary>
/// 权限目录中的一个业务模块分组。
/// </summary>
public sealed class PermissionCatalogModuleDto
{
    /// <summary>
    /// 模块键，如 <c>config</c>。
    /// </summary>
    public string Module { get; set; } = null!;

    /// <summary>
    /// 模块中文名，如「参数配置」。
    /// </summary>
    public string ModuleName { get; set; } = null!;

    /// <summary>
    /// 该模块下的权限动作列表。
    /// </summary>
    public IReadOnlyList<PermissionCatalogItemDto> Items { get; set; } = [];
}
