namespace Axis.Application.Author.Dto;

/// <summary>
/// 登录菜单路由元数据，与前端 Vue Router <c>meta</c> 字段对齐。
/// </summary>
public sealed class LoginMenuMeta
{
    /// <summary>
    /// 菜单标题；用于侧栏与页签展示。
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 图标标识；可为空。
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 是否以内嵌 iframe 打开。
    /// </summary>
    public bool IsIframe { get; set; }

    /// <summary>
    /// 外链地址；非外链时为空。
    /// </summary>
    public string IsLink { get; set; }

    /// <summary>
    /// 是否在菜单中隐藏。
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否启用页面缓存。
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否固定在页签栏。
    /// </summary>
    public bool IsAffix { get; set; }
}
