import type { AxiosResponse } from 'axios'

import type { RequestOptions, Result } from '../models/axios-model'

/**
 * 默认业务响应适配器。
 *
 * 项目默认约定后端响应结构为 `{ code, success, message, data }`，其中
 * `success === true` 或 `code === 10000` 表示业务成功。不同项目如果响应结构
 * 不同，应优先替换本适配器，而不是修改 HTTP core 或业务页面中的请求调用。
 */
export function defaultResponseAdapter<T = unknown>(
  response: AxiosResponse<Result<T>>,
  options: RequestOptions
): T | Result<T> | AxiosResponse<Result<T>> {
  const { isTransformResponse, isReturnNativeResponse } = options
  if (response.status === 204) return undefined as T

  if (isReturnNativeResponse) {
    return response
  }

  if (!isTransformResponse) {
    return response.data
  }

  const { data } = response
  if (!data) {
    throw new Error('请求接口错误')
  }

  if (data.success === true || data.code === 10000) {
    return data.data
  }

  throw new Error(data.message || `请求接口错误，错误码：${data.code}`)
}
