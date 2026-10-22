import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { Router } from 'vue-router'

import { localAsyncRoutes, normalizeRemoteMenuRoutes } from '@/router/routes/async-routes'
import type { RemoteMenuRoute } from '@/router/routes/async-routes'
import { filterAccessRoutes } from '@/shared/access'
import type { AppRouteRecord } from '@/shared/types/app-route'

export interface BuildRoutesOptions {
  /**
   * 后端菜单权限模式。
   *
   * - frontend：后端返回菜单后，前端继续根据 roles/permissions 做二次过滤。
   * - trusted：后端已经按当前用户裁剪菜单，前端只做页面路径白名单映射和路由注册。
   */
  remoteMenuAccessMode?: 'frontend' | 'trusted'
}

export const usePermissionStore = defineStore('permission', () => {
  /** 当前用户可访问并已转换为 vue-router 结构的动态路由。 */
  const accessRoutes = ref<AppRouteRecord[]>([])

  /** 是否已经完成动态路由构建，避免路由守卫重复注册。 */
  const isBuilt = ref(false)

  /** 当前用户是否拥有至少一条动态路由。 */
  const hasAccessRoutes = computed(() => accessRoutes.value.length > 0)

  /**
   * 构建当前用户可访问的动态路由。
   *
   * 动态路由来源分为两类：
   * 1. localAsyncRoutes：前端本地维护的动态路由。
   * 2. remoteMenus：后端返回的菜单路由，必须通过页面路径映射到前端组件。
   *
   * 两类路由合并后统一交给权限服务过滤，保证菜单过滤、路由守卫和按钮权限
   * 使用同一套角色与权限码规则。
   */
  function buildRoutes(
    roles: string[] = [],
    permissions: string[] = [],
    remoteMenus: readonly RemoteMenuRoute[] = [],
    options: BuildRoutesOptions = {}
  ): AppRouteRecord[] {
    const remoteMenuAccessMode = options.remoteMenuAccessMode ?? 'frontend'
    const remoteRoutes = normalizeRemoteMenuRoutes(remoteMenus, {
      accessMode: remoteMenuAccessMode === 'trusted' ? 'trustedRemote' : 'frontend'
    })
    const candidateRoutes = [...localAsyncRoutes, ...remoteRoutes]

    accessRoutes.value = filterAccessRoutes(candidateRoutes, { roles, permissions })
    isBuilt.value = true
    return accessRoutes.value
  }

  /**
   * 重置权限路由状态。
   *
   * 登出或切换用户时应调用该方法；如果传入 router，会同步移除已经注册到
   * vue-router 实例中的动态路由和兜底 CatchAll 路由。
   */
  function reset(router?: Router) {
    if (router) {
      accessRoutes.value.forEach(route => {
        if (route.name && router.hasRoute(route.name)) {
          router.removeRoute(route.name)
        }
      })
      if (router.hasRoute('CatchAll')) {
        router.removeRoute('CatchAll')
      }
    }
    accessRoutes.value = []
    isBuilt.value = false
  }

  return {
    accessRoutes,
    isBuilt,
    hasAccessRoutes,
    buildRoutes,
    reset
  }
})
