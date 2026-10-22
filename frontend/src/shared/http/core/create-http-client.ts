import merge from 'lodash-es/merge'

import { defaultResponseAdapter } from '../adapters/default-response.adapter'
import { PAxios } from '../axios-setup'
import type { AxiosTransform, CreateAxiosOptions } from '../axios-transform'
import { ContentTypeEnum } from '../models/axios-model'
import { applyAuthHeader } from '../plugins/auth.plugin'
import { applyAntiReplayHeaders } from '../plugins/anti-replay.plugin'
import { createResponseErrorHandler } from '../plugins/error.plugin'
import { applyRequestOptions } from '../plugins/request-options.plugin'

import { router } from '@/router'
import { message } from '@/shared/utils/message'
import { useUserStore } from '@/store/modules/user.store'

const env = import.meta.env.MODE || 'development'

/**
 * 计算 Axios 直连地址。
 *
 * 开发代理开启时，请求保持同源路径，由 Vite proxy 转发；
 * 生产或关闭代理时，才使用 VITE_API_BASE_URL 拼接真实接口域名。
 */
const host =
  env === 'mock' || import.meta.env.VITE_USE_PROXY === 'true'
    ? ''
    : import.meta.env.VITE_API_BASE_URL

function createDefaultTransform(): AxiosTransform {
  return {
    beforeRequestHook: applyRequestOptions,
    transformRequestHook: defaultResponseAdapter,
    requestInterceptors: async (config, options) => {
      const userStore = useUserStore()
      applyAuthHeader(config, options, userStore.token)
      return applyAntiReplayHeaders(config, userStore.token)
    },
    responseInterceptors: response => response,
    responseInterceptorsCatch: createResponseErrorHandler({
      // 用 getter 延迟解析 router / store，规避
      // `shared/http -> router -> guards -> store -> api -> shared/http`
      // 的循环 import 在模块初始化期触发的 TDZ。
      getRouter: () => router,
      getUserStore: () => useUserStore(),
      messageApi: message
    })
  }
}

/**
 * 创建项目默认 HTTP 客户端。
 *
 * 调用方可以传入局部配置覆盖默认值，但不应绕过 core 直接 new PAxios，
 * 以免丢失认证、错误处理和响应适配等统一能力。
 */
export function createHttpClient(opt?: Partial<CreateAxiosOptions>) {
  return new PAxios(
    merge(
      <CreateAxiosOptions>{
        authenticationScheme: 'Bearer',
        timeout: 10 * 1000,
        withCredentials: true,
        headers: { 'Content-Type': ContentTypeEnum.Json },
        transform: createDefaultTransform(),
        requestOptions: {
          apiUrl: host,
          isJoinPrefix: true,
          urlPrefix: import.meta.env.VITE_API_PREFIX,
          isReturnNativeResponse: false,
          isTransformResponse: true,
          joinParamsToUrl: false,
          formatDate: true,
          joinTime: true,
          ignoreCancelToken: true,
          withToken: true,
          retry: {
            count: 0,
            delay: 1000
          }
        }
      },
      opt || {}
    )
  )
}
