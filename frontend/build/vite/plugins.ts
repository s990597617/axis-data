import path from 'node:path'

import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import UnoCSS from 'unocss/vite'
import AutoImport from 'unplugin-auto-import/vite'
import Components from 'unplugin-vue-components/vite'
import { NaiveUiResolver } from 'unplugin-vue-components/resolvers'
import { visualizer } from 'rollup-plugin-visualizer'
import { compression } from 'vite-plugin-compression2'
import type { PluginOption } from 'vite'

import { sharedRoot } from './paths'

export interface CreateVitePluginsOptions {
  /** 当前命令是否为生产构建。 */
  isBuild: boolean
  /** 是否开启构建产物分析。 */
  enableAnalyze: boolean
}

const compressedAssetExclude = [/\.(br|gz)$/, /\.(png|jpe?g|webp|avif|ico|woff2?)$/i]

/**
 * 创建 Vite 插件列表。
 *
 * 基础插件负责 Vue、JSX、UnoCSS、自动导入和 Naive UI 组件解析。
 * 构建阶段额外产出 gzip 与 brotli 资源，便于 Nginx 或 CDN 直接开启静态压缩资源。
 */
export function createVitePlugins(options: CreateVitePluginsOptions): PluginOption[] {
  const plugins: PluginOption[] = [
    vue(),
    vueJsx(),
    UnoCSS(),
    AutoImport({
      imports: ['vue'],
      dts: path.resolve(sharedRoot, 'types', 'auto-imports.d.ts')
    }),
    Components({
      resolvers: [NaiveUiResolver()],
      dirs: ['src/shared/components'],
      dts: path.resolve(sharedRoot, 'types', 'components.d.ts')
    })
  ]

  if (options.isBuild) {
    plugins.push(
      compression({
        algorithms: ['gzip'],
        exclude: compressedAssetExclude,
        threshold: 1024,
        deleteOriginalAssets: false
      }),
      compression({
        algorithms: ['brotliCompress'],
        exclude: compressedAssetExclude,
        threshold: 1024,
        deleteOriginalAssets: false
      })
    )
  }

  if (options.isBuild && options.enableAnalyze) {
    plugins.push(
      visualizer({
        filename: 'dist/stats.html',
        open: true,
        gzipSize: true,
        brotliSize: true,
        template: 'treemap'
      }) as PluginOption
    )
  }

  return plugins
}
