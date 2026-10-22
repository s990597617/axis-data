import type { Router } from 'vue-router'

import { createAuthGuard } from './auth.guard'
import { createPermissionGuard } from './permission.guard'
import { createProgressGuard } from './progress.guard'
import { createTitleGuard } from './title.guard'

/**
 * 注册路由守卫
 *
 * 执行顺序（beforeEach 按注册顺序串行）：
 * 1. auth       —— 登录态校验、动态路由装配
 * 2. permission —— 细粒度角色 / 权限校验
 * 3. progress   —— 顶部进度条
 * 4. title      —— afterEach，更新页面标题
 */
export function setupRouterGuards(router: Router) {
  createAuthGuard(router)
  createPermissionGuard(router)
  createProgressGuard(router)
  createTitleGuard(router)
}
