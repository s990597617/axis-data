import type { AppRouteMeta, AppRouteRecord } from '@/shared/types/app-route'

/**
 * 路由 IR 节点
 *
 * @description
 * 以 `AppRouteRecord` 为上游数据源，经过统一预处理后产出的中间表示（IR）：
 * - `path` 已拼接为绝对路径
 * - `title` 已回退为 `name`
 * - `hidden` 父级会被"扁平化"（自身不出现在 IR 中，其可见子节点上提到当前层）
 * - `layout: 'full'` 的路由（登录 / 403 / 404 等）一律剔除
 *
 * 菜单渲染、全局搜索、标签固定等上层消费方都以 IR 为输入，避免在各处重复
 * 实现路径拼接、hidden 扁平化、排序等逻辑。
 */
export interface RouteNode {
  /** 路由名（唯一标识，用于 keep-alive / 菜单选中联动） */
  name: string
  /** 已解析为绝对形式的访问路径 */
  path: string
  /** 显示标题（缺省时回退为 `name`） */
  title: string
  /** 图标字符串（iconify 名，例：`ant-design:dashboard-outlined`） */
  icon?: string
  /** 是否启用 keep-alive 缓存 */
  keepAlive: boolean
  /** 是否固定标签页 */
  affix: boolean
  /** 祖先标题链（不含自身），用于面包屑 / 搜索上下文 */
  parents: string[]
  /** 原始 meta（如需访问 roles / permissions 等自定义字段） */
  meta?: AppRouteMeta
  /** 子节点（hidden 扁平化后的可见直属子级） */
  children?: RouteNode[]
}

/**
 * 按 `meta.order` 升序排列同级路由（不改动原数组）
 */
export function sortRoutes<T extends { meta?: { order?: number } }>(routes: readonly T[]): T[] {
  return [...routes].sort((a, b) => (a.meta?.order ?? 0) - (b.meta?.order ?? 0))
}

/**
 * 拼接父路径与子路径
 *
 * @remarks
 * - 子路径以 `/` 开头视为绝对路径，直接返回
 * - 自动折叠重复斜杠，并去掉父路径尾部斜杠
 */
export function resolveRoutePath(parentPath: string, childPath: string): string {
  if (childPath.startsWith('/')) return childPath
  if (!parentPath) return `/${childPath}`
  return `${parentPath.replace(/\/$/, '')}/${childPath}`.replace(/\/+/g, '/')
}

/**
 * 由 `AppRouteRecord` 构建路由 IR
 *
 * @remarks
 * - `layout === 'full'` 的路由整棵剪枝
 * - `hidden === true` 的父级：自身不入 IR；其可见子节点上提到当前层
 * - `hidden === true` 的叶子：直接丢弃
 * - `parents` / `path` 在递归中向下透传，避免调用方再拼接
 */
export function buildRouteTree(
  routes: readonly AppRouteRecord[],
  parents: string[] = [],
  parentPath = '',
): RouteNode[] {
  const result: RouteNode[] = []
  for (const route of sortRoutes(routes)) {
    if (route.meta?.layout === 'full') continue

    const path = resolveRoutePath(parentPath, route.path)
    const title = route.meta?.title ?? route.name

    if (route.meta?.hidden) {
      // 隐藏父级：提升其可见子节点到当前层
      if (route.children?.length) {
        result.push(...buildRouteTree(route.children, parents, path))
      }
      continue
    }

    const children = route.children?.length
      ? buildRouteTree(route.children, [...parents, title], path)
      : undefined

    const node: RouteNode = {
      name: route.name,
      path,
      title,
      icon: route.meta?.icon,
      keepAlive: !!route.meta?.keepAlive,
      affix: !!route.meta?.affix,
      parents,
      meta: route.meta,
    }
    if (children?.length) node.children = children
    result.push(node)
  }
  return result
}

/**
 * 收集 IR 中的全部叶子节点（DFS 保序）
 *
 * @description
 * 搜索、affix 标签提取等"按叶子组织"的场景请使用此函数。
 */
export function collectLeafNodes(tree: readonly RouteNode[]): RouteNode[] {
  const out: RouteNode[] = []
  for (const node of tree) {
    if (node.children?.length) out.push(...collectLeafNodes(node.children))
    else out.push(node)
  }
  return out
}

/**
 * 构建 `route.name → 绝对路径` 映射
 *
 * @description
 * 用于菜单选中状态同步等"按 name 跳转"的场景。
 * 与 IR 不同，这里**包含** hidden 路由，保证中转页面也能被 name 查到。
 */
export function buildNameToPathMap(
  routes: readonly AppRouteRecord[],
  parentPath = '',
  map = new Map<string, string>(),
): Map<string, string> {
  for (const route of routes) {
    const path = resolveRoutePath(parentPath, route.path)
    map.set(route.name, path)
    if (route.children?.length) buildNameToPathMap(route.children, path, map)
  }
  return map
}
