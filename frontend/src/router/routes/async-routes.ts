import type { RouteRecordRaw } from 'vue-router'

import { resolveRouteComponent } from './route-component-map'

import type { AppRouteMeta, AppRouteRecord } from '@/shared/types/app-route'

/**
 * 后端菜单路由契约。
 *
 * 边界约定：
 * 1. 后端负责返回用户可见的菜单树、路由名称、路径、排序、角色和权限码。
 * 2. 后端返回页面路径，路径可以来自菜单表中已有的 component 或 componentPath 字段。
 * 3. 前端通过 import.meta.glob 生成组件白名单，并根据页面路径解析真实组件。
 * 4. 未命中白名单的页面路径会被前端丢弃，避免未知组件进入路由表。
 */
export interface RemoteMenuRoute {
  /** 路由访问路径。 */
  path: string
  /** 路由唯一名称。 */
  name: string
  /** 后端菜单表中的页面路径字段，推荐 modules/system/views/user/index；兼容历史 modules/system/user/index、system/user/index。 */
  component?: string
  /** 页面路径字段的语义化别名；如果同时存在，会优先使用 componentPath。 */
  componentPath?: string
  /** 重定向地址。 */
  redirect?: RouteRecordRaw['redirect']
  /** 路由元信息。 */
  meta: AppRouteMeta
  /** 子菜单路由。 */
  children?: RemoteMenuRoute[]
}

export interface NormalizeRemoteMenuRoutesOptions {
  /**
   * 后端菜单访问控制模式。
   *
   * 默认使用 frontend，表示后端菜单仍会经过前端 roles/permissions 二次过滤。
   * 使用 trustedRemote 时，表示后端已经按当前用户裁剪菜单，前端只负责页面路径白名单校验。
   */
  accessMode?: AppRouteMeta['accessMode']
}

/**
 * 前端本地动态路由。
 *
 * 这部分路由由前端维护，适合放置不需要后端菜单驱动、
 * 但仍然需要经过登录态和权限过滤的业务路由。
 */
export const localAsyncRoutes: AppRouteRecord[] = []

/** 兼容既有调用的动态路由导出，当前等同于前端本地动态路由。 */
export const asyncRoutes = localAsyncRoutes

function normalizeRemoteRoute(
  route: RemoteMenuRoute,
  options: NormalizeRemoteMenuRoutesOptions
): AppRouteRecord | null {
  const componentPath = route.componentPath ?? route.component ?? (route.children?.length ? 'RootLayout' : undefined)
  if (!componentPath) return null

  const component = resolveRouteComponent(componentPath)
  if (!component) return null

  const children = route.children
    ?.map(child => normalizeRemoteRoute(child, options))
    .filter((item): item is AppRouteRecord => !!item)

  return {
    path: route.path,
    name: route.name,
    component,
    meta: {
      ...route.meta,
      accessMode: options.accessMode ?? route.meta.accessMode ?? 'frontend'
    },
    ...(route.redirect ? { redirect: route.redirect } : {}),
    ...(children?.length ? { children } : {})
  } as AppRouteRecord
}

/** 将后端菜单树转换为 vue-router 可注册的动态路由记录。 */
export function normalizeRemoteMenuRoutes(
  routes: readonly RemoteMenuRoute[] = [],
  options: NormalizeRemoteMenuRoutesOptions = {}
): AppRouteRecord[] {
  return routes
    .map(route => normalizeRemoteRoute(route, options))
    .filter((route): route is AppRouteRecord => !!route)
}
