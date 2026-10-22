import type { AxiosRequestConfig } from 'axios'
import isString from 'lodash-es/isString'

import { formatRequestDate, joinTimestamp, setObjToUrlParams } from '../axios-util'
import type { RequestOptions } from '@/shared/http'

/**
 * 应用项目级请求选项。
 *
 * 该插件只处理 URL、参数、时间戳和日期格式化，不关心认证、错误提示和业务响应结构。
 * 这样 HTTP core 可以保持稳定，项目差异通过插件和适配器扩展。
 */
export function applyRequestOptions(
  config: AxiosRequestConfig,
  options: RequestOptions
): AxiosRequestConfig {
  const { apiUrl, isJoinPrefix, urlPrefix, joinParamsToUrl, formatDate, joinTime = true } = options

  if (isJoinPrefix && urlPrefix && isString(urlPrefix)) {
    config.url = `${urlPrefix}${config.url}`
  }

  if (apiUrl && isString(apiUrl)) {
    config.url = `${apiUrl}${config.url}`
  }

  const params = config.params || {}
  const data = config.data || false

  if (formatDate && data && !isString(data)) {
    formatRequestDate(data)
  }

  if (config.method?.toUpperCase() === 'GET') {
    if (!isString(params)) {
      config.params = Object.assign(params || {}, joinTimestamp(joinTime, false))
    } else {
      config.url = `${config.url + params}${joinTimestamp(joinTime, true)}`
      config.params = undefined
    }
  } else if (!isString(params)) {
    if (formatDate) {
      formatRequestDate(params)
    }

    if (
      Reflect.has(config, 'data') &&
      config.data &&
      (Object.keys(config.data).length > 0 || data instanceof FormData)
    ) {
      config.data = data
      config.params = params
    } else {
      config.data = params
      config.params = undefined
    }

    if (joinParamsToUrl) {
      config.url = setObjToUrlParams(config.url as string, { ...config.params, ...config.data })
    }
  } else {
    config.url += params
    config.params = undefined
  }

  return config
}
