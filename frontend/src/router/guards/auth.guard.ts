import type { Router, RouteLocationNormalized } from 'vue-router'

import { adaptMenuTreeToRemoteRoutes } from '@/router/helpers/menu-adapter'
import { ensureCatchAllRoute } from '@/router/helpers/catch-all'
import { useUserStore } from '@/store/modules/user.store'
import { usePermissionStore } from '@/store/modules/permission.store'

/**
 * 不需要登录即可访问的白名单路由名称
 */
const WHITE_LIST = new Set(['Login', 'Forbidden', 'NotFound'])

function isWhiteList(route: RouteLocationNormalized): boolean {
  if (typeof route.name === 'string' && WHITE_LIST.has(route.name)) {
    return true
  }

  // 兼容首次加载、刷新等尚未匹配到具体路由名的场景
  return ['/login', '/403', '/404'].includes(route.path)
}

/**
 * 登录鉴权 & 动态路由装配守卫
 */
export function createAuthGuard(router: Router) {
  let appliedAuthorizationVersion = -1

  router.beforeEach(async to => {
    const userStore = useUserStore()
    const permissionStore = usePermissionStore()

    // 首次导航会校验过期时间、按需刷新 Token，并重新拉取最新权限信息。
    const hasSession = await userStore.ensureSession()

    // 已登录
    if (hasSession) {
      if (
        permissionStore.isBuilt &&
        appliedAuthorizationVersion !== userStore.authorizationVersion
      ) {
        permissionStore.reset(router)
      }

      // 登录状态下访问登录页，直接跳首页
      if (to.name === 'Login') {
        return { path: '/', replace: true }
      }

      // 动态路由尚未装配 -> 构建后重定向重新匹配
      if (!permissionStore.isBuilt) {
        const roles = userStore.roles ?? []
        const permissions = userStore.permissions ?? []
        const remoteMenus = adaptMenuTreeToRemoteRoutes(userStore.userInfo?.menuTrees ?? [])
        const accessRoutes = permissionStore.buildRoutes(roles, permissions, remoteMenus, {
          remoteMenuAccessMode: 'trusted'
        })

        accessRoutes.forEach(route => {
          router.addRoute(route)
        })

        ensureCatchAllRoute(router)
        appliedAuthorizationVersion = userStore.authorizationVersion

        return { ...to, replace: true }
      }

      return true
    }

    // 未登录
    if (isWhiteList(to)) {
      return true
    }

    return {
      name: 'Login',
      query: to.fullPath !== '/' ? { redirect: to.fullPath } : undefined,
      replace: true
    }
  })
}
