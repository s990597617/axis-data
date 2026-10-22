import type { AxiosError, AxiosInstance } from 'axios'
import type { Router } from 'vue-router'

import { ContentTypeEnum } from '../models/axios-model'
import type { AxiosRequestConfigRetry } from '../models/axios-model'

import type { useUserStore } from '@/store/modules/user.store'

type UserStore = ReturnType<typeof useUserStore>

interface MessageApi {
  error: (content: string) => void
}

export interface HttpErrorHandlerOptions {
  /**
   * 延迟获取当前应用路由实例，用于 401 后跳转登录页。
   * 必须延迟求值以避免与 store / API 模块互相 import 触发的初始化循环。
   */
  getRouter: () => Router
  /** 延迟获取用户 store，避免在 Pinia 初始化前读取 store。 */
  getUserStore: () => UserStore
  /** 全局消息 API，用于统一错误提示。 */
  messageApi: MessageApi
}

// ---- Token 静默刷新 ----

/** 是否正在刷新 Token */
let isRefreshing = false
/** 等待刷新完成的请求队列 */
interface PendingRequest {
  resolve: (token: string) => void
  reject: (error: unknown) => void
}

let pendingRequests: PendingRequest[] = []

function subscribeTokenRefresh(request: PendingRequest) {
  pendingRequests.push(request)
}

function onTokenRefreshed(newToken: string) {
  const requests = pendingRequests
  pendingRequests = []
  requests.forEach(request => request.resolve(newToken))
}

function onTokenRefreshFailed(error: unknown) {
  const requests = pendingRequests
  pendingRequests = []
  requests.forEach(request => request.reject(error))
}

const errorMessages: Record<number, string> = {
  400: '请求参数错误',
  401: '登录已过期，请重新登录',
  403: '没有权限访问该资源',
  404: '请求的资源不存在',
  500: '服务器内部错误',
  502: '网关错误',
  503: '服务不可用',
  504: '网关超时'
}

function extractBackendMessage(data: unknown): string | undefined {
  if (!data) return undefined
  if (typeof data === 'string') return data || undefined
  if (typeof data !== 'object') return undefined

  const record = data as Record<string, unknown>
  const { message, msg, error, errors, detail } = record

  if (typeof message === 'string' && message) return message
  if (typeof msg === 'string' && msg) return msg
  if (typeof error === 'string' && error) return error
  if (typeof detail === 'string' && detail) return detail

  // 422 常见结构：errors 为字段错误数组或对象
  if (Array.isArray(errors)) {
    const flat = errors
      .map(item => {
        if (typeof item === 'string') return item
        if (item && typeof item === 'object') {
          const it = item as Record<string, unknown>
          return (it.message as string) || (it.msg as string) || (it.detail as string) || ''
        }
        return ''
      })
      .filter(Boolean)
    if (flat.length) return flat.join('；')
  } else if (errors && typeof errors === 'object') {
    const flat: string[] = []
    for (const v of Object.values(errors as Record<string, unknown>)) {
      if (typeof v === 'string') flat.push(v)
      else if (Array.isArray(v)) flat.push(...v.filter(x => typeof x === 'string'))
    }
    if (flat.length) return flat.join('；')
  }

  if (Array.isArray(detail)) {
    const flat = detail
      .map(item => (typeof item === 'string' ? item : (item as { msg?: string })?.msg || ''))
      .filter(Boolean)
    if (flat.length) return flat.join('；')
  }

  return undefined
}

function getErrorMessage(error: AxiosError): string {
  // 优先使用后端返回体中的 message，保证 422 等带具体错误信息能被展示
  const backendMsg = extractBackendMessage(error.response?.data)
  if (backendMsg) return backendMsg

  const status = error.response?.status
  if (status && errorMessages[status]) return errorMessages[status]
  return error.message || '请求失败'
}

function redirectToLogin(router: Router, userStore: UserStore) {
  userStore.clearSession(router)

  const currentFullPath = router.currentRoute.value.fullPath
  if (router.currentRoute.value.name !== 'Login') {
    router.replace({
      name: 'Login',
      query: currentFullPath && currentFullPath !== '/' ? { redirect: currentFullPath } : undefined
    })
  }
}

function retryRequest(
  error: AxiosError,
  instance: AxiosInstance
): Promise<unknown> | Promise<never> {
  const config = error.config as AxiosRequestConfigRetry | undefined

  if (!config || !config.requestOptions?.retry) {
    return Promise.reject(error)
  }

  config.retryCount = config.retryCount || 0

  if (config.retryCount >= config.requestOptions.retry.count) {
    return Promise.reject(error)
  }

  config.retryCount += 1

  return new Promise(resolve => {
    setTimeout(() => resolve(config), config.requestOptions?.retry?.delay || 1)
  }).then(config => {
    const retryConfig = config as AxiosRequestConfigRetry
    retryConfig.headers = { ...retryConfig.headers, 'Content-Type': ContentTypeEnum.Json }
    return instance.request(retryConfig)
  })
}

/**
 * 创建统一响应错误处理器。
 *
 * 该处理器负责错误提示、401 Token 静默刷新、登出跳转和请求重试。
 * 业务响应 code 的判断不在这里处理，而是交给 response adapter。
 */
export function createResponseErrorHandler(options: HttpErrorHandlerOptions) {
  return (error: AxiosError, instance: AxiosInstance) => {
    const status = error.response?.status

    // ---- 401: 尝试用 RefreshToken 静默续期 ----
    if (status === 401) {
      const userStore = options.getUserStore()
      const originalConfig = error.config as AxiosRequestConfigRetry | undefined

      // 刷新接口本身 401、无原始配置或已经重放过一次 → 直接登出。
      // 每个业务请求最多触发一次刷新，避免刷新成功但接口仍返回 401 时形成循环。
      if (
        !originalConfig ||
        originalConfig.requestOptions?.skipAuthRefresh ||
        originalConfig.authRefreshRetried
      ) {
        redirectToLogin(options.getRouter(), userStore)
        return Promise.reject(error)
      }

      originalConfig.authRefreshRetried = true

      // 已有刷新流程在跑 → 排队等新 Token
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          subscribeTokenRefresh({
            resolve: newToken => {
              originalConfig.headers = originalConfig.headers ?? {}
              originalConfig.headers.Authorization = `Bearer ${newToken}`
              resolve(instance.request(originalConfig))
            },
            reject
          })
        })
      }

      // 发起刷新
      isRefreshing = true

      return userStore
        .refresh()
        .then(success => {
          if (!success) {
            // 刷新失败 → 登出
            isRefreshing = false
            onTokenRefreshFailed(error)
            options.messageApi.error('登录已过期，请重新登录')
            redirectToLogin(options.getRouter(), userStore)
            return Promise.reject(error)
          }

          // 刷新成功 → 通知排队请求，重发当前请求
          const newToken = userStore.token
          isRefreshing = false
          onTokenRefreshed(newToken)

          originalConfig.headers = originalConfig.headers ?? {}
          originalConfig.headers.Authorization = `Bearer ${newToken}`
          return instance.request(originalConfig)
        })
        .catch(refreshError => {
          isRefreshing = false
          onTokenRefreshFailed(refreshError)
          return Promise.reject(refreshError)
        })
        .finally(() => {
          isRefreshing = false
        })
    }

    // ---- 非 401 错误的通用处理 ----
    const friendly = getErrorMessage(error)
    options.messageApi.error(friendly)

    // 把 axios 默认的 "Request failed with status code xxx" 改写为已展示的友好文案，
    // 同时打上 handled 标记，方便上层 catch 判断是否需要重复提示。
    try {
      error.message = friendly
      ;(error as AxiosError & { handled?: boolean }).handled = true
    } catch {
      // error 对象不可写时忽略
    }

    return retryRequest(error, instance)
  }
}

/** 判断该错误是否已经被 HTTP 错误拦截器统一提示过，避免上层重复弹窗。 */
export function isHandledHttpError(error: unknown): boolean {
  return !!(error && typeof error === 'object' && (error as { handled?: boolean }).handled)
}
