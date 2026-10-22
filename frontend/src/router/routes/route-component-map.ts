import { ParentView, RootLayout } from './static-routes'

import type { RouteRecordRaw } from 'vue-router'

type RouteComponent = NonNullable<RouteRecordRaw['component']>
type LazyRouteComponent = () => Promise<unknown>
type RouteComponentMap = Record<string, RouteComponent | LazyRouteComponent>

/** 顶级布局容器组件在后端菜单中的保留路径标识。 */
export const ROOT_LAYOUT_COMPONENT_PATH = 'RootLayout'

/** 嵌套目录透传组件：只渲染子路由，不重复套布局壳。 */
export const PARENT_VIEW_COMPONENT_PATH = 'ParentView'

/** 后端系统里常见的布局组件占位写法，都会统一映射为 RootLayout。 */
const ROOT_LAYOUT_COMPONENT_PATH_ALIASES = new Set(['RootLayout', 'Layout', 'LAYOUT', 'layout'])

/** 嵌套目录占位写法，统一映射为 ParentView。 */
const PARENT_VIEW_COMPONENT_PATH_ALIASES = new Set([
  'ParentView',
  'ParentLayout',
  'PARENT_VIEW',
  'parent'
])

/**
 * 自动扫描业务页面组件。
 *
 * 后端可以返回页面路径，但前端只会从这个扫描结果中解析组件。
 * 这样既能兼容后端菜单表存路径的习惯，也不会让后端任意加载未纳入前端工程的文件。
 */
const pageComponentModules = import.meta.glob('/src/{pages,modules}/**/*.vue')

/** 域根保留目录：不是功能页，不做 views 兼容改写。 */
const MODULE_ROOT_RESERVED = new Set(['api', 'permissions', 'routes', 'stores', 'views'])

/**
 * 将后端返回的页面路径标准化为前端组件表使用的绝对路径。
 *
 * 支持的写法示例：
 * 1. RootLayout（顶级布局）。
 * 2. ParentView（嵌套目录透传）。
 * 3. modules/system/views/user/index（推荐，业务页）。
 * 4. modules/home/views/index.vue。
 * 5. pages/login/index（仅登录/异常等壳页面）。
 * 6. modules/system/user/index（兼容旧路径：自动尝试插入 views/）。
 * 7. system/user/index（兼容：会依次尝试 pages/、modules/、modules/.../views/）。
 */
export function normalizeRouteComponentPath(componentPath: string): string {
  const trimmedPath = componentPath.trim().replaceAll('\\', '/')

  if (ROOT_LAYOUT_COMPONENT_PATH_ALIASES.has(trimmedPath)) {
    return ROOT_LAYOUT_COMPONENT_PATH
  }

  if (PARENT_VIEW_COMPONENT_PATH_ALIASES.has(trimmedPath)) {
    return PARENT_VIEW_COMPONENT_PATH
  }

  const pathWithoutAlias = trimmedPath
    .replace(/^@\//, '')
    .replace(/^\/+/, '')
    .replace(/^src\//, '')

  const pathWithExtension = pathWithoutAlias.endsWith('.vue')
    ? pathWithoutAlias
    : `${pathWithoutAlias}.vue`

  return `/src/${pathWithExtension}`
}

/**
 * 旧菜单路径 modules/{domain}/{rest} → modules/{domain}/views/{rest}。
 * 已含 views/ 或落在域根横切目录时返回 null。
 */
function toModuleViewsRelativePath(relativePath: string): string | null {
  const match = /^modules\/([^/]+)\/(.+)$/.exec(relativePath)
  if (!match) return null

  const domain = match[1]
  const rest = match[2]
  const firstSegment = rest.split('/')[0]
  if (MODULE_ROOT_RESERVED.has(firstSegment)) return null

  return `modules/${domain}/views/${rest}`
}

/** 根据后端页面路径生成可能命中的前端页面路径候选列表。 */
function createRouteComponentPathCandidates(componentPath: string): string[] {
  const normalizedPath = normalizeRouteComponentPath(componentPath)
  if (
    normalizedPath === ROOT_LAYOUT_COMPONENT_PATH ||
    normalizedPath === PARENT_VIEW_COMPONENT_PATH
  ) {
    return [normalizedPath]
  }

  const relativePath = normalizedPath.replace(/^\/src\//, '')
  const candidates: string[] = []

  const pushRelative = (path: string) => {
    const absolutePath = `/src/${path}`
    if (!candidates.includes(absolutePath)) {
      candidates.push(absolutePath)
    }
  }

  if (relativePath.startsWith('pages/') || relativePath.startsWith('modules/')) {
    pushRelative(relativePath)
    const viewsPath = toModuleViewsRelativePath(relativePath)
    if (viewsPath) pushRelative(viewsPath)
    return candidates
  }

  pushRelative(`pages/${relativePath}`)
  pushRelative(`modules/${relativePath}`)
  const viewsFromModules = toModuleViewsRelativePath(`modules/${relativePath}`)
  if (viewsFromModules) pushRelative(viewsFromModules)
  pushRelative(relativePath)
  return candidates
}

/**
 * 动态路由组件映射表。
 *
 * key 使用标准化后的页面路径，例如 /src/modules/system/views/user/index.vue。
 * 后端返回的路径必须能被标准化并命中这里的白名单，否则该路由不会被注册。
 */
export const routeComponentMap: RouteComponentMap = {
  [ROOT_LAYOUT_COMPONENT_PATH]: RootLayout,
  [PARENT_VIEW_COMPONENT_PATH]: ParentView,
  ...pageComponentModules
}

/** 后端菜单可使用的组件路径。 */
export type RouteComponentPath = keyof typeof routeComponentMap

/** 判断传入路径是否能解析为受信任的前端组件。 */
export function isRouteComponentPath(value: string): value is RouteComponentPath {
  return createRouteComponentPathCandidates(value).some(componentPath =>
    Object.hasOwn(routeComponentMap, componentPath)
  )
}

/** 根据后端返回的页面路径解析前端路由组件。 */
export function resolveRouteComponent(componentPath: string) {
  const matchedPath = createRouteComponentPathCandidates(componentPath).find(candidatePath =>
    Object.hasOwn(routeComponentMap, candidatePath)
  )

  return matchedPath ? routeComponentMap[matchedPath] : undefined
}
