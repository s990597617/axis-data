using Axis.Application.Author.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;

namespace Axis.Application.Author.Internal;

/// <summary>
/// 将扁平菜单列表装配为登录菜单树。
/// </summary>
/// <remarks>
/// 仅供 Author 资料装配使用；会剔除按钮节点，按钮权限由 Buttons 字段单独下发。
/// </remarks>
internal static class LoginMenuTreeBuilder
{
    /// <summary>
    /// 按 OrderNo 组装树，并移除按钮节点。
    /// </summary>
    /// <param name="menus">当前用户可见的全部菜单（含按钮）。</param>
    /// <returns>仅含目录与菜单的根级树。</returns>
    public static IReadOnlyList<LoginMenuTreeNode> Build(IReadOnlyList<SysMenu> menus)
    {
        var map = menus.ToDictionary(m => m.Id, ToNode);
        var roots = new List<LoginMenuTreeNode>();

        foreach (var m in menus.OrderBy(m => m.OrderNo).ThenBy(m => m.Id))
        {
            var node = map[m.Id];
            if (m.ParentId == 0 || !map.TryGetValue(m.ParentId, out var parent))
                roots.Add(node);
            else
                parent.Children.Add(node);
        }

        RemoveButtonNodes(roots);
        return roots;
    }

    private static void RemoveButtonNodes(List<LoginMenuTreeNode> nodes)
    {
        for (var i = nodes.Count - 1; i >= 0; i--)
        {
            var n = nodes[i];
            if (n.Type == MenuTypeEnum.Btn)
            {
                nodes.RemoveAt(i);
                continue;
            }

            if (n.Children.Count > 0) RemoveButtonNodes(n.Children);
        }
    }

    private static LoginMenuTreeNode ToNode(SysMenu m) => new()
    {
        Id = m.Id,
        Pid = m.ParentId,
        Type = m.Type,
        Name = m.Name,
        Path = m.Path,
        Component = m.Component,
        Permission = m.Permission,
        Redirect = m.Redirect,
        OrderNo = m.OrderNo,
        Status = m.Status,
        Remark = m.Remark,
        CreateTime = m.CreatedTime,
        UpdateTime = m.ModifiedTime,
        Meta = new LoginMenuMeta
        {
            Title = m.Title,
            Icon = m.Icon,
            IsIframe = m.IsFrame,
            IsLink = m.Redirect,
            IsHide = m.IsHide,
            IsKeepAlive = m.IsKeepAlive,
            // 工作台固定标签；其余菜单暂无 IsAffix 字段，默认不固定
            IsAffix = string.Equals(m.Name, "Home", StringComparison.Ordinal),
        }
    };
}
