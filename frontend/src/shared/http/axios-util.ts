import isObject from 'lodash-es/isObject'
import isString from 'lodash-es/isString'

/**
 * 定义时间格式
 */
const DATE_TIME_FORMAT = 'YYYY-MM-DD HH:mm:ss'

interface MomentLike {
  _isAMomentObject?: boolean
  format: (format: string) => string
}

function isRecord(value: unknown): value is Recordable {
  return Object.prototype.toString.call(value) === '[object Object]'
}

function isMomentLike(value: unknown): value is MomentLike {
  return isRecord(value) && value._isAMomentObject === true && typeof value.format === 'function'
}

/**
 * 添加时间戳
 */
export function joinTimestamp<T extends boolean>(
  join: boolean,
  restful: T
): T extends true ? string : object

/**
 * 添加时间戳
 */
export function joinTimestamp(join: boolean, restful = false): string | object {
  if (!join) {
    return restful ? '' : {}
  }
  const now = new Date().getTime()
  if (restful) {
    return `?_t=${now}`
  }
  return { _t: now }
}

/**
 * 格式化提交参数时间
 */
export function formatRequestDate(params: Recordable) {
  if (!isRecord(params)) {
    return
  }

  for (const key in params) {
    // eslint-disable-next-line no-underscore-dangle
    if (isMomentLike(params[key])) {
      params[key] = params[key].format(DATE_TIME_FORMAT)
    }
    if (isString(key)) {
      const value = params[key]
      if (value) {
        try {
          params[key] = isString(value) ? value.trim() : value
        } catch (error: any) {
          throw new Error(error)
        }
      }
    }
    if (isObject(params[key]) && isRecord(params[key])) {
      formatRequestDate(params[key])
    }
  }
}

/**
 * 将对象转为Url参数
 */
export function setObjToUrlParams(baseUrl: string, obj: { [index: string]: any }): string {
  let parameters = ''
  for (const key in obj) {
    parameters += `${key}=${encodeURIComponent(obj[key])}&`
  }
  parameters = parameters.replace(/&$/, '')
  return /\?$/.test(baseUrl) ? baseUrl + parameters : baseUrl.replace(/\/?$/, '?') + parameters
}
