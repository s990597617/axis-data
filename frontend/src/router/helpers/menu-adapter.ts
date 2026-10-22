import type { MenuTreeNode } from '@/modules/system/api/author'
import type { RemoteMenuRoute } from '@/router/routes/async-routes'

/**
 * 将后端返回的菜单树节点（MenuTreeNode）适配为前端路由系统需要的 RemoteMenuRoute 格式。
 *
 * 后端返回结构：
 * - type: 0=目录, 1=菜单, 2=按钮（按钮已被后端剔除）
 * - component: 页面组件路径（如 "modules/system/views/user/index"）
 * - meta 内的字段：title, icon, isHide, isKeepAlive, isAffix, isIframe, isLink
 *
 * 前端需要的结构：
 * - path, name, component（页面路径字符串）, redirect, meta（AppRouteMeta）, children
 *
 * 转换要点：
 * 1. 顶级目录（type=0 且无父路径）使用 RootLayout；嵌套目录使用 ParentView，避免重复套壳。
 * 2. 顶级菜单页（如工作台）同样包一层 RootLayout，子路由 path 为空串挂在同路径下。
 * 3. 目录自动 redirect 到第一个子菜单。
 * 4. 子路由 path 转换为相对路径（去掉与父路由重复的前缀），符合 vue-router 嵌套约定。
 * 5. 菜单节点（type=1）使用后端的 component 字段作为页面组件路径。
 */
export function adaptMenuTreeToRemoteRoutes(menuTrees: MenuTreeNode[]): RemoteMenuRoute[] {
  return menuTrees
    .map(node => adaptNode(node, ''))
    .filter((r): r is RemoteMenuRoute => r !== null)
}

function adaptNode(node: MenuTreeNode, parentPath: string): RemoteMenuRoute | null {
  if (!node.path && !node.name) return null

  const isDir = node.type === 0
  const isTopLevel = !parentPath
  const isTopLevelDir = isDir && isTopLevel
  const fullPath = node.path ?? ''

  // 子路由使用相对路径
  const relativePath = parentPath && fullPath.startsWith(parentPath + '/')
    ? fullPath.slice(parentPath.length + 1)
    : fullPath

  const children = node.children
    ?.map(child => adaptNode(child, fullPath))
    .filter((r): r is RemoteMenuRoute => r !== null)

  const meta = {
    title: node.meta?.title ?? node.name ?? '',
    icon: node.meta?.icon,
    hidden: node.meta?.isHide ?? false,
    keepAlive: node.meta?.isKeepAlive ?? true,
    affix: node.meta?.isAffix ?? false,
    order: node.orderNo
  }

  // 顶级菜单页（工作台等）：包 RootLayout，避免无壳布局
  if (!isDir && isTopLevel) {
    return {
      path: fullPath,
      name: `${node.name}Layout`,
      component: 'RootLayout',
      meta: {
        ...meta,
        hidden: true
      },
      children: [
        {
          path: '',
          name: node.name ?? '',
          component: node.component || undefined,
          meta
        }
      ]
    }
  }

  // 目录自动 redirect 到第一个子菜单
  const firstChildPath = children?.[0]?.path
  const redirect = isDir && firstChildPath
    ? (firstChildPath.startsWith('/') ? firstChildPath : `${fullPath}/${firstChildPath}`)
    : (node.redirect || undefined)

  return {
    path: relativePath,
    name: node.name ?? '',
    component: isDir
      ? (isTopLevelDir ? 'RootLayout' : 'ParentView')
      : (node.component || undefined),
    redirect,
    meta,
    ...(children?.length ? { children } : {})
  }
}
