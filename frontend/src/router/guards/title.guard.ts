import type { Router } from 'vue-router'
import { appConfig } from '@/config/app.config'

export function createTitleGuard(router: Router) {
  router.afterEach((to) => {
    const pageTitle = typeof to.meta?.title === 'string' ? to.meta.title : ''
    document.title = pageTitle ? `${pageTitle} - ${appConfig.appName}` : appConfig.defaultTitle
  })
}
