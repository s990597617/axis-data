import type { Router } from 'vue-router'

/**
 * 确保兜底路由始终在动态路由完成注册后装配。
 *
 * 首次登录与刷新恢复登录态会走不同的动态路由构建入口，统一通过该函数
 * 注册 CatchAll，避免其中一个入口遗漏 404 处理。
 */
export function ensureCatchAllRoute(router: Router) {
  if (router.hasRoute('CatchAll')) {
    router.removeRoute('CatchAll')
  }

  router.addRoute({
    path: '/:pathMatch(.*)*',
    name: 'CatchAll',
    redirect: '/404',
    meta: {
      title: '未匹配路由',
      layout: 'full',
      hidden: true
    }
  })
}
