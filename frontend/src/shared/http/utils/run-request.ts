import { message } from '@/shared/utils/message'

import { isHandledHttpError } from '../plugins/error.plugin'

export interface RunRequestOptions {
  /** HTTP 拦截器没有提示时使用的兜底文案。默认 '请求失败'。 */
  fallbackMessage?: string
  /**
   * 出错时的副作用回调（不影响是否弹提示）。
   * 适合用来重置 UI 状态、抖动表单、刷新验证码等。
   */
  onError?: (error: unknown) => void
  /** 是否在 catch 之后继续向上抛错。默认 false。 */
  rethrow?: boolean
  /**
   * 是否禁用兜底提示。
   * HTTP 拦截器已统一弹过的错误本来就不会重复提示，这里是针对
   * 非 HTTP 错误（如本地抛出的业务异常）想完全静默时使用。
   */
  silent?: boolean
}

/**
 * 统一封装 try/catch：
 * - HTTP 拦截器已统一提示过的错误不会重复弹窗（依赖 isHandledHttpError）。
 * - 其它错误使用 `error.message` 或 `fallbackMessage` 做兜底提示。
 * - 通过 `onError` 处理副作用，避免外层再写一层 try/catch。
 *
 * @returns 成功时返回业务数据，失败且未 rethrow 时返回 undefined。
 */
export async function runRequest<T>(
  task: () => Promise<T>,
  options: RunRequestOptions = {}
): Promise<T | undefined> {
  const { fallbackMessage = '请求失败', onError, rethrow = false, silent = false } = options
  try {
    return await task()
  } catch (error) {
    if (!silent && !isHandledHttpError(error)) {
      message.error(error instanceof Error && error.message ? error.message : fallbackMessage)
    }
    onError?.(error)
    if (rethrow) throw error
    return undefined
  }
}

export type TryResult<T> = [data: T, err: null] | [data: undefined, err: Error]

/**
 * Go 风格的 try 包装：返回 `[data, err]` 元组，调用方判 err 即可，无需手写 try/catch。
 *
 * 错误提示策略与 `runRequest` 一致：
 * - HTTP 拦截器已统一提示过的错误不会重复弹窗。
 * - 其它错误使用 `error.message` 或 `fallbackMessage` 做兜底提示。
 *
 * @example
 *   const [res, err] = await tryRequest(getCaptcha(), { fallbackMessage: '获取验证码失败' })
 *   if (err) return
 *   captchaId.value = res.id
 */
export async function tryRequest<T>(
  promise: Promise<T>,
  options: Omit<RunRequestOptions, 'onError' | 'rethrow'> = {}
): Promise<TryResult<T>> {
  const { fallbackMessage = '请求失败', silent = false } = options
  try {
    return [await promise, null]
  } catch (error) {
    const err = error instanceof Error ? error : new Error(String(error))
    if (!silent && !isHandledHttpError(error)) {
      message.error(err.message || fallbackMessage)
    }
    return [undefined, err]
  }
}
