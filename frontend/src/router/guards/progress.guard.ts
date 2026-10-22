import NProgress from 'nprogress'
import 'nprogress/nprogress.css'
import type { Router } from 'vue-router'

/**
 * 路由切换顶部进度条守卫
 *
 * @description
 * 使用 NProgress 在路由切换时显示顶部进度条。
 * 样式覆盖集中在 `@/assets/styles/nprogress.scss`，本文件仅负责生命周期。
 */

NProgress.configure({
  speed: 300,
  showSpinner: false,
  minimum: 0.2,
  trickleSpeed: 150
})

export function createProgressGuard(router: Router) {
  router.beforeEach(() => {
    NProgress.start()
  })

  router.afterEach(() => {
    NProgress.done()
  })

  router.onError(() => {
    NProgress.done()
  })
}
