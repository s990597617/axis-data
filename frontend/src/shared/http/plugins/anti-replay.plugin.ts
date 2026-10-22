import type { AxiosRequestConfig } from 'axios'

import type { AxiosRequestConfigRetry } from '../models/axios-model'

const MUTATING_METHODS = new Set(['post', 'put', 'patch', 'delete'])

/** 与后端 AxisAntiReplayOptions 默认头名对齐。 */
export const AXIS_TIMESTAMP_HEADER = 'X-Axis-Timestamp'
export const AXIS_NONCE_HEADER = 'X-Axis-Nonce'
export const AXIS_SIGN_HEADER = 'X-Axis-Sign'

function createNonce(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID().replace(/-/g, '')
  }

  const bytes = new Uint8Array(16)
  if (typeof crypto !== 'undefined' && typeof crypto.getRandomValues === 'function') {
    crypto.getRandomValues(bytes)
  } else {
    for (let i = 0; i < bytes.length; i += 1) {
      bytes[i] = Math.floor(Math.random() * 256)
    }
  }
  return Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('')
}

/** 与后端 RequestHmacSigner.NormalizePath 对齐。 */
function resolveRequestPath(url?: string): string {
  if (!url) return '/'

  let value = url.trim()
  try {
    if (/^https?:\/\//i.test(value)) {
      value = new URL(value).pathname
    }
  } catch {
    // keep raw
  }

  const query = value.indexOf('?')
  if (query >= 0) value = value.slice(0, query)
  if (!value.startsWith('/')) value = `/${value}`
  return value
}

/** HMAC-SHA256 → Base64，密钥为 AccessToken。 */
async function hmacSha256Base64(secret: string, payload: string): Promise<string> {
  const encoder = new TextEncoder()
  const key = await crypto.subtle.importKey(
    'raw',
    encoder.encode(secret),
    { name: 'HMAC', hash: 'SHA-256' },
    false,
    ['sign']
  )
  const signature = await crypto.subtle.sign('HMAC', key, encoder.encode(payload))
  const bytes = new Uint8Array(signature)
  let binary = ''
  for (let i = 0; i < bytes.length; i += 1) {
    binary += String.fromCharCode(bytes[i]!)
  }
  return btoa(binary)
}

/**
 * 为已带 Token 的写请求注入时间戳、Nonce、HMAC 签名。
 *
 * - 仅 POST/PUT/PATCH/DELETE
 * - `withToken === false`（登录/验证码等）跳过
 * - 签名串：`METHOD&path&timestamp&nonce`，密钥为 AccessToken（管理端暂用；OpenAPI 另走 AccessKey）
 */
export async function applyAntiReplayHeaders(
  config: AxiosRequestConfig,
  accessToken: string
): Promise<AxiosRequestConfig> {
  const method = (config.method ?? 'get').toLowerCase()
  if (!MUTATING_METHODS.has(method)) return config

  const requestConfig = config as AxiosRequestConfigRetry
  if (requestConfig.requestOptions?.withToken === false) return config
  if (!accessToken) return config

  const timestamp = String(Date.now())
  const nonce = createNonce()
  const path = resolveRequestPath(config.url)
  const payload = `${method.toUpperCase()}&${path}&${timestamp}&${nonce}`
  const sign = await hmacSha256Base64(accessToken, payload)

  config.headers = config.headers ?? {}
  config.headers[AXIS_TIMESTAMP_HEADER] = timestamp
  config.headers[AXIS_NONCE_HEADER] = nonce
  config.headers[AXIS_SIGN_HEADER] = sign

  return config
}
