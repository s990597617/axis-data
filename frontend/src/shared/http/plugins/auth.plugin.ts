import type { AxiosRequestConfig } from 'axios'

import type { CreateAxiosOptions } from '@/shared/http'
import type { AxiosRequestConfigRetry } from '../models/axios-model'

/**
 * 为请求注入认证头。
 *
 * 单个请求可以通过 `requestOptions.withToken = false` 跳过 Token 注入，
 * 例如登录、刷新验证码等公开接口。
 */
export function applyAuthHeader(
  config: AxiosRequestConfig,
  options: CreateAxiosOptions,
  token: string
): AxiosRequestConfig {
  const requestConfig = config as AxiosRequestConfigRetry
  const withToken = requestConfig.requestOptions?.withToken !== false

  if (!token || !withToken) {
    return config
  }

  config.headers = config.headers ?? {}
  config.headers.Authorization = options.authenticationScheme
    ? `${options.authenticationScheme} ${token}`
    : token

  return config
}
