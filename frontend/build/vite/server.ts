import type { UserConfig } from 'vite'

export interface CreateViteServerOptions {
  /** 开发服务器端口。 */
  port: number
  /** 接口代理前缀。 */
  apiPrefix: string
  /** 接口代理目标地址。 */
  proxyTarget: string
  /** 是否启用 Vite 开发代理。 */
  useProxy: boolean
}

const warmupClientFiles = [
  './src/main.ts',
  './src/App.vue',
  './src/layouts/index.vue',
  './src/router/**/*.ts',
  './src/store/**/*.ts',
  './src/shared/icons/**/*.ts'
]

/** 创建开发服务器配置，集中处理端口、代理和冷启动预热。 */
export function createViteServer(options: CreateViteServerOptions): UserConfig['server'] {
  const useProxy = options.useProxy && !!options.apiPrefix && !!options.proxyTarget

  return {
    host: '0.0.0.0',
    port: options.port,
    open: false,
    proxy: useProxy
      ? {
          // 后端 FastEndpoints 全局 RoutePrefix = "api"，前缀需要原样转发，不做 rewrite。
          [options.apiPrefix]: {
            target: options.proxyTarget,
            changeOrigin: true,
            secure: false
          },
          // SignalR Hub（WebSocket）
          '/hubs': {
            target: options.proxyTarget,
            changeOrigin: true,
            secure: false,
            ws: true
          },
          // 本地上传静态目录（文件管理缩略图 / 预览）
          '/Uploads': {
            target: options.proxyTarget,
            changeOrigin: true,
            secure: false
          }
        }
      : undefined,
    warmup: {
      clientFiles: warmupClientFiles
    }
  }
}
