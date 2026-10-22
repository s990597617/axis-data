import type { AxiosRequestConfig } from 'axios'

/** 常用请求 Content-Type。 */
export enum ContentTypeEnum {
  Json = 'application/json;charset=UTF-8',
  FormURLEncoded = 'application/x-www-form-urlencoded;charset=UTF-8',
  FormData = 'multipart/form-data;charset=UTF-8'
}

/**
 * 项目级请求选项。
 *
 * 这些选项不属于 Axios 原生配置，而是由 shared/http 的插件层消费。
 * 单个接口可以在调用 request.get/post 时传入局部 options 覆盖默认行为。
 */
export interface RequestOptions {
  /** 接口基础地址，通常用于生产环境直连真实 API 域名。 */
  apiUrl?: string
  /** 是否自动拼接接口前缀。 */
  isJoinPrefix?: boolean
  /** 接口前缀，例如 `/api`。 */
  urlPrefix?: string
  /** POST/PUT/PATCH/DELETE 请求是否把参数同步拼到 URL 上。 */
  joinParamsToUrl?: boolean
  /** 是否格式化提交参数中的日期对象，并裁剪字符串前后空白。 */
  formatDate?: boolean
  /** 是否启用业务响应适配，启用后默认返回 `data.data`。 */
  isTransformResponse?: boolean
  /** 是否直接返回 Axios 原始响应对象。 */
  isReturnNativeResponse?: boolean
  /** 是否忽略重复请求取消逻辑。 */
  ignoreCancelToken?: boolean
  /** 是否自动为 GET 请求追加时间戳，避免浏览器缓存。 */
  joinTime?: boolean
  /** 是否携带 Token。 */
  withToken?: boolean
  /** 是否禁止 401 自动刷新，登录、刷新和退出接口应开启。 */
  skipAuthRefresh?: boolean
  /** 请求失败重试配置。 */
  retry?: {
    /** 最大重试次数。 */
    count: number
    /** 每次重试间隔，单位毫秒。 */
    delay: number
  }
  /** 接口级节流配置。 */
  throttle?: {
    /** 节流间隔，单位毫秒。 */
    delay: number
    /** 调度分组 key；默认按请求方法和 URL 分组。 */
    key?: string
  }
  /** 接口级防抖配置。 */
  debounce?: {
    /** 防抖间隔，单位毫秒。 */
    delay: number
    /** 调度分组 key；默认按请求方法和 URL 分组。 */
    key?: string
  }
}

/**
 * 默认业务响应结构。
 *
 * 如果项目后端返回格式不同，优先替换 response adapter，而不是在页面中手动拆包。
 */
export interface Result<T = any> {
  /** 业务状态码，默认 `10000` 表示成功（前 3 位等于 HTTP 状态码）。 */
  code: number
  /** 是否成功；后端固定提供，等价于 `code === 10000`。 */
  success?: boolean
  /** 业务数据。 */
  data: T
  /** 业务提示信息。 */
  message?: string
  /** 链路追踪 ID。 */
  traceId?: string
  /** 请求路径。 */
  path?: string
  /** 服务端时间戳（ISO-8601）。 */
  timestamp?: string
}

/** 扩展后的 Axios 请求配置，供重试和插件层使用。 */
export interface AxiosRequestConfigRetry extends AxiosRequestConfig {
  /** 当前请求已经重试的次数。 */
  retryCount?: number
  /** 该请求是否已经执行过一次 401 Token 刷新重放。 */
  authRefreshRetried?: boolean
  /** 合并后的项目级请求选项。 */
  requestOptions?: RequestOptions
}
