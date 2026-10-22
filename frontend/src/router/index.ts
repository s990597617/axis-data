import type { App } from 'vue'
import { createRouter, createWebHashHistory } from 'vue-router'

import { setupRouterGuards } from '@/router/guards'
import { staticRoutes } from '@/router/routes/static-routes'
import { setupRouterChunkErrorHandler } from '@/router/helpers/chunk-error'

export const router = createRouter({
  history: createWebHashHistory(),
  routes: staticRoutes,
  scrollBehavior: (_to, _from, savedPosition) => savedPosition ?? { left: 0, top: 0 }
})

export function setupRouter(app: App) {
  setupRouterGuards(router)
  setupRouterChunkErrorHandler(router)
  app.use(router)
}
