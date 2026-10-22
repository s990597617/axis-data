import { createHttpClient } from './core/create-http-client'

export { createHttpClient }
export { isHandledHttpError } from './plugins/error.plugin'
export { runRequest, tryRequest, type RunRequestOptions, type TryResult } from './utils/run-request'
export type { CreateAxiosOptions } from './axios-transform'
export type { RequestOptions, Result } from './models/axios-model'

/** 项目默认 HTTP 请求实例。 */
export const request = createHttpClient()
