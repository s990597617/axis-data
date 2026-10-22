import type { App } from 'vue'

import { message } from '@/shared/utils/message'

import { reportError } from './reporter'

export { reportError, setErrorReporter, type ErrorReport, type ErrorReporter } from './reporter'

/** 最近一次向用户提示错误的时间戳，做节流，避免同类错误刷屏 */
let lastNoticeAt = 0
const NOTICE_INTERVAL = 2000

function notifyUser(text: string) {
  const now = Date.now()
  if (now - lastNoticeAt < NOTICE_INTERVAL) return
  lastNoticeAt = now
  try {
    message.error(text)
  } catch {
    // message 离散 API 尚未就绪时忽略
  }
}

function stringifyError(error: unknown): string {
  if (error instanceof Error) return error.message || error.name
  if (typeof error === 'string') return error
  try {
    return JSON.stringify(error)
  } catch {
    return String(error)
  }
}

/** Naive UI / Async-validator 表单校验失败（字段下方已提示，无需顶部再弹） */
function isFormValidateError(error: unknown): boolean {
  if (!Array.isArray(error) || error.length === 0) return false
  const first = error[0]
  if (Array.isArray(first)) {
    return first.some(
      item => item && typeof item === 'object' && 'message' in item && 'field' in item
    )
  }
  return Boolean(first && typeof first === 'object' && 'message' in first && 'field' in first)
}

/**
 * 判断是否是动态 chunk 加载失败
 *
 * @description
 * Vite / Webpack 懒加载失败时抛出的错误不统一，这里按 message 做兜底匹配：
 * - `ChunkLoadError`
 * - `Failed to fetch dynamically imported module`
 * - `Importing a module script failed`
 */
export function isChunkLoadError(error: unknown): boolean {
  if (!error) return false
  const msg = error instanceof Error ? `${error.name}: ${error.message}` : String(error)
  return (
    /ChunkLoadError/i.test(msg) ||
    /Failed to fetch dynamically imported module/i.test(msg) ||
    /Importing a module script failed/i.test(msg) ||
    /Loading chunk .* failed/i.test(msg)
  )
}

/**
 * 注册全局错误处理
 *
 * @description
 * 捕获 Vue 组件、原生 JS、Promise 三类未处理异常，统一走 `reporter`，
 * 并给用户一个温和的提示。chunk 加载失败由路由守卫专门处理，这里只做兜底。
 */
export function setupGlobalErrorHandler(app: App) {
  // 1. Vue 组件错误
  app.config.errorHandler = (error, instance, info) => {
    if (isFormValidateError(error)) return

    reportError({
      source: 'vue',
      error,
      context: {
        hook: info,
        component: instance?.$options?.name ?? instance?.$?.type?.name,
      },
    })
    if (import.meta.env.DEV) {
      notifyUser(`[Vue] ${stringifyError(error)}`)
    }
  }

  // 2. 原生未捕获异常
  window.addEventListener('error', (event) => {
    const err = event.error ?? event.message
    if (isChunkLoadError(err)) return // 交给路由 chunk handler
    reportError({
      source: 'window',
      error: err,
      context: {
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
      },
    })
  })

  // 3. 未处理的 Promise reject
  window.addEventListener('unhandledrejection', (event) => {
    const reason = event.reason
    if (isChunkLoadError(reason)) return
    if (isFormValidateError(reason)) {
      event.preventDefault()
      return
    }
    reportError({ source: 'promise', error: reason })
    if (import.meta.env.DEV) {
      notifyUser(`[Promise] ${stringifyError(reason)}`)
    }
  })
}
