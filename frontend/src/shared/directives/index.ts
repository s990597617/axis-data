import type { App } from 'vue'

import { setupPermissionDirective } from './permission'

/**
 * 统一注册共享层指令
 */
export function setupDirectives(app: App) {
  setupPermissionDirective(app)
}

export { permissionDirective } from './permission'
