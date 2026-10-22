import type { RouteLocationMatched } from 'vue-router'

import type { AccessMeta, AccessRouteRecord, AccessSubject } from './access.types'

/** 通配权限标识，命中后视为拥有全部访问能力。 */
export const SUPER_ACCESS_MARK = '*'

/** 后端真实的超级管理员角色编码。 */
export const SUPER_ADMIN_ROLE = 'super-admin'

function toList(value: readonly string[] | undefined): readonly string[] {
  return value ?? []
}

function hasAny(source: readonly string[], targets: readonly string[]): boolean {
  return targets.some(item => source.includes(item))
}

/**
 * 判断用户是否为超级权限用户。
 *
 * 命中账号类型 999、`*` 角色、`*` 权限码或 `super-admin` 角色时直接放行。
 * 该规则是全项目唯一的超级权限约定，路由守卫、菜单过滤和按钮权限都应复用它。
 */
export function isSuperAdmin(subject: AccessSubject): boolean {
  if (subject.accountType === 999) return true
  const roles = toList(subject.roles)
  const permissions = toList(subject.permissions)
  return (
    roles.includes(SUPER_ACCESS_MARK) ||
    roles.includes(SUPER_ADMIN_ROLE) ||
    permissions.includes(SUPER_ACCESS_MARK)
  )
}

/** 判断用户是否命中任意一个角色。 */
export function hasAnyRole(subject: AccessSubject, roles: readonly string[] = []): boolean {
  if (!roles.length) return true
  if (isSuperAdmin(subject)) return true
  return hasAny(toList(subject.roles), roles)
}

/** 判断用户是否命中任意一个权限码。 */
export function hasAnyPermission(
  subject: AccessSubject,
  permissions: readonly string[] = []
): boolean {
  if (!permissions.length) return true
  if (isSuperAdmin(subject)) return true
  return hasAny(toList(subject.permissions), permissions)
}

/**
 * 判断用户是否满足单层路由元信息。
 *
 * 角色和权限码采用 AND 语义：如果同时声明 roles 和 permissions，
 * 用户必须同时命中角色约束和权限约束才可访问。
 */
export function canAccessByMeta(meta: AccessMeta | undefined, subject: AccessSubject): boolean {
  if (meta?.accessMode === 'trustedRemote') return true
  if (!meta?.roles?.length && !meta?.permissions?.length) return true
  if (isSuperAdmin(subject)) return true
  return hasAnyRole(subject, meta.roles) && hasAnyPermission(subject, meta.permissions)
}

/** 判断用户是否能访问单条路由记录。 */
export function canAccessRoute(route: AccessRouteRecord, subject: AccessSubject): boolean {
  return canAccessByMeta(route.meta, subject)
}

/**
 * 判断用户是否能访问当前匹配到的完整路由链。
 *
 * 父子路由采用逐级 AND 语义：父级权限会天然约束子级，
 * 子级可以继续声明更细的角色或权限码。
 */
export function canAccessMatchedRoutes(
  matched: readonly RouteLocationMatched[],
  subject: AccessSubject
): boolean {
  return matched.every(record => canAccessByMeta(record.meta, subject))
}

/**
 * 递归过滤当前用户可访问的路由。
 *
 * 过滤时会浅克隆路由对象，避免改写原始路由表。
 * 如果父级无权限，整棵子树都会被剔除。
 */
export function filterAccessRoutes(
  routes: readonly AccessRouteRecord[],
  subject: AccessSubject
): AccessRouteRecord[] {
  const result: AccessRouteRecord[] = []

  routes.forEach(route => {
    if (!canAccessRoute(route, subject)) return

    const cloned: AccessRouteRecord = { ...route }
    if (route.children?.length) {
      cloned.children = filterAccessRoutes(route.children, subject)

      // 父级菜单通常只是目录容器；当子级全部被权限过滤后，需要同步移除父级，避免出现空菜单。
      if (!cloned.children.length) return
    }
    result.push(cloned)
  })

  return result
}
