import type { Router, RouteLocationNormalized } from 'vue-router'

import { canAccessMatchedRoutes } from '@/shared/access'
import { useUserStore } from '@/store/modules/user.store'

/**
 * 注册细粒度权限守卫。
 *
 * 该守卫只负责“已登录用户是否允许访问目标路由”的判断。
 * 登录态校验和动态路由注册由 auth.guard 负责，权限规则由 shared/access 统一维护。
 */
export function createPermissionGuard(router: Router) {
  router.beforeEach((to: RouteLocationNormalized) => {
    const userStore = useUserStore()

    if (!userStore.isLogin) {
      return true
    }

    const allowed = canAccessMatchedRoutes(to.matched, {
      roles: userStore.roles,
      permissions: userStore.permissions
    })

    if (allowed) {
      return true
    }

    return { name: 'Forbidden', replace: true }
  })
}
