import type { Router } from 'vue-router'

import { STORAGE_KEYS } from '@/config/storage-keys'
import { isChunkLoadError, reportError } from '@/shared/error'

/**
 * 10 秒内同一会话不允许再次自动重载，避免死循环
 */
const RELOAD_COOLDOWN = 10_000

function shouldAutoReload() {
  try {
    const last = Number(sessionStorage.getItem(STORAGE_KEYS.CHUNK_RELOAD_AT) || 0)
    return !last || Date.now() - last > RELOAD_COOLDOWN
  } catch {
    return true
  }
}

function markReloaded() {
  try {
    sessionStorage.setItem(STORAGE_KEYS.CHUNK_RELOAD_AT, String(Date.now()))
  } catch {
    // sessionStorage 不可用时忽略
  }
}

/**
 * 注册路由 chunk 加载失败处理器
 *
 * @description
 * 生产环境部署后，旧页面持有过期的 chunk 文件名，点击路由跳转会 404。
 * 这里捕获该类错误并用目标地址 `window.location.href = to.fullPath` 整页重载，
 * 让浏览器重新请求最新的 `index.html` 拿到新 chunk 指纹。
 */
export function setupRouterChunkErrorHandler(router: Router) {
  router.onError((error, to) => {
    reportError({
      source: 'chunk',
      error,
      context: { target: to?.fullPath }
    })

    if (!isChunkLoadError(error)) return

    if (!shouldAutoReload()) {
      // 短时间内已经尝试过重载，说明新部署也没救，让用户手动处理
      // eslint-disable-next-line no-console
      console.warn('[router] chunk load failed repeatedly, skip auto reload')
      return
    }

    markReloaded()
    const target = to?.fullPath
    if (target) {
      // 使用 location 硬跳转，走一次完整资源请求
      window.location.href = window.location.pathname + window.location.search + '#' + target
      window.location.reload()
    } else {
      window.location.reload()
    }
  })
}
