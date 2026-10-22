import { readFileSync } from 'node:fs'
import { fileURLToPath, URL } from 'node:url'

import { type ConfigEnv, defineConfig, type UserConfig } from 'vite'

import { resolveViteEnv } from './build/vite/env'
import { createViteBuild } from './build/vite/build'
import { createViteServer } from './build/vite/server'
import { createVitePlugins } from './build/vite/plugins'
import { optimizeDepsConfig } from './build/vite/optimize-deps'

const packageInfo = JSON.parse(
  readFileSync(new URL('./package.json', import.meta.url), 'utf8')
) as { version: string }

/** Vite 配置入口，只负责读取环境变量并组装各配置模块。 */
export default defineConfig((configEnv: ConfigEnv): UserConfig => {
  const viteEnv = resolveViteEnv(configEnv)

  return {
    base: viteEnv.publicPath,
    define: {
      __APP_VERSION__: JSON.stringify(packageInfo.version)
    },
    plugins: createVitePlugins({
      isBuild: viteEnv.isBuild,
      enableAnalyze: viteEnv.enableAnalyze
    }),
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url))
      }
    },
    server: createViteServer({
      port: viteEnv.port,
      apiPrefix: viteEnv.apiPrefix,
      proxyTarget: viteEnv.proxyTarget,
      useProxy: viteEnv.useProxy
    }),
    optimizeDeps: optimizeDepsConfig,
    build: createViteBuild({ isProd: viteEnv.isProd })
  }
})
