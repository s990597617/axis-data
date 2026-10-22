namespace Axis.Application.System.Menu.Dto;

/// <summary>
/// 权限目录中的单条动作。
/// </summary>
public sealed class PermissionCatalogItemDto
{
    /// <summary>
    /// 权限码，写入菜单按钮的 <c>Permission</c> 字段。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 动作显示名；可用于预填按钮标题。
    /// </summary>
    public string ActionName { get; set; } = null!;
}
