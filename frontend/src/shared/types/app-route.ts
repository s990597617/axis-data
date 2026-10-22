import type { RouteRecordRaw } from 'vue-router'

import type { LayoutMode } from '@/shared/types/app-layout'

/**
 * 应用路由元信息。
 *
 * 这是路由、菜单、权限、面包屑、标签页和页面缓存的统一数据源。
 * 新增路由能力时应优先扩展这里，而不是在菜单或标签页中再声明一套并行结构。
 */
export interface AppRouteMeta {
  [key: string]: unknown
  [key: symbol]: unknown
  /** 页面、菜单、面包屑和标签页展示标题。 */
  title: string
  /** 菜单、标签页等位置使用的图标名称，默认使用 Iconify 图标名。 */
  icon?: string
  /** 是否从菜单树中隐藏。隐藏路由仍然可以被直接访问和权限校验。 */
  hidden?: boolean
  /** 同级菜单排序号，数值越小越靠前。 */
  order?: number
  /** 允许访问当前路由的角色列表；未声明时表示不限制角色。 */
  roles?: string[]
  /** 允许访问当前路由的权限码列表；未声明时表示不限制权限码。 */
  permissions?: string[]
  /**
   * 路由访问控制模式。
   *
   * - frontend：由前端根据 roles 和 permissions 进行过滤与守卫校验。
   * - trustedRemote：后端菜单已经按当前用户裁剪完成，前端只校验页面路径是否在白名单内。
   */
  accessMode?: 'frontend' | 'trustedRemote'
  /** 是否启用 keep-alive 页面缓存。 */
  keepAlive?: boolean
  /** 是否固定在标签页栏中，通常用于首页、工作台等核心页面。 */
  affix?: boolean
  /** 当前页面需要高亮的菜单路径，常用于详情页、编辑页等隐藏路由。 */
  activeMenu?: string
  /** 页面使用的外层布局类型，full 通常用于登录页和异常页。 */
  layout?: 'default' | 'full'
  /** 页面在默认布局中的呈现模式，可覆盖全局布局偏好。 */
  layoutMode?: LayoutMode
}

declare module 'vue-router' {
  interface RouteMeta extends Partial<AppRouteMeta> {}
}

/**
 * 应用内部路由记录。
 *
 * 在本项目中，路由 name 必须保持全局唯一，因为菜单选中、标签页缓存、
 * 动态路由卸载和权限映射都依赖这个稳定标识。
 */
export type AppRouteRecord = RouteRecordRaw & {
  /** 路由唯一名称。 */
  name: string
  /** 路由访问路径。 */
  path: string
  /** 应用级路由元信息。 */
  meta: AppRouteMeta
  /** 子路由列表。 */
  children?: AppRouteRecord[]
}
