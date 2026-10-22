import type { UserConfig } from 'vite'

export interface CreateViteBuildOptions {
  /** 当前模式是否为 production。 */
  isProd: boolean
}

/** 根据资源类型输出到固定目录，方便 CDN 缓存和线上排查。 */
function createAssetFileName({ names }: { names: string[] }) {
  const name = names[0] ?? ''
  if (/\.css$/.test(name)) return 'assets/css/[name]-[hash][extname]'
  if (/\.(png|jpe?g|gif|svg|webp|avif|ico)$/i.test(name)) {
    return 'assets/images/[name]-[hash][extname]'
  }
  if (/\.(woff2?|eot|ttf|otf)$/i.test(name)) {
    return 'assets/fonts/[name]-[hash][extname]'
  }
  return 'assets/[name]-[hash][extname]'
}

/**
 * 第三方依赖分包策略。
 *
 * 分包以“稳定程度”和“体积”作为主要依据：
 * 图标数据、UI 框架、Vue 生态、工具库分别拆包，业务迭代时可以尽量保持
 * vendor chunk 的 hash 稳定，提高浏览器长缓存命中率。
 */
const codeSplittingGroups = [
  {
    name: 'icon-data-antd',
    test: /[\\/]node_modules[\\/]@iconify-json[\\/]ant-design[\\/]/,
    priority: 100
  },
  {
    name: 'icon-data-tabler',
    test: /[\\/]node_modules[\\/]@iconify-json[\\/]tabler[\\/]/,
    priority: 100
  },
  {
    name: 'icon-data-misc',
    test: /[\\/]node_modules[\\/]@iconify-json[\\/]/,
    priority: 90
  },
  {
    name: 'iconify',
    test: /[\\/]node_modules[\\/]@iconify[\\/](vue|core|utils)[\\/]/,
    priority: 80
  },
  {
    name: 'naive-ui',
    test: /[\\/]node_modules[\\/](naive-ui|@css-render|vueuc)[\\/]/,
    priority: 70
  },
  {
    name: 'vue-vendor',
    test: /[\\/]node_modules[\\/](vue-router|pinia|@vue)[\\/]/,
    priority: 60
  },
  {
    name: 'utils-vendor',
    test: /[\\/]node_modules[\\/](lodash-es|axios|qs|dayjs)[\\/]/,
    priority: 60
  },
  {
    name: 'vendor',
    test: /[\\/]node_modules[\\/]/,
    priority: 10
  }
]

/** 创建生产构建配置，集中维护压缩、产物目录和分包策略。 */
export function createViteBuild(options: CreateViteBuildOptions): UserConfig['build'] {
  return {
    target: 'es2022',
    cssTarget: 'chrome90',
    minify: options.isProd ? 'terser' : 'oxc',
    terserOptions: options.isProd
      ? {
          compress: {
            drop_console: true,
            drop_debugger: true
          }
        }
      : undefined,
    sourcemap: false,
    cssCodeSplit: true,
    reportCompressedSize: false,
    chunkSizeWarningLimit: 3000,
    assetsInlineLimit: 4096,
    emptyOutDir: true,
    modulePreload: { polyfill: false },
    rolldownOptions: {
      output: {
        chunkFileNames: 'assets/js/[name]-[hash].js',
        entryFileNames: 'assets/js/[name]-[hash].js',
        assetFileNames: createAssetFileName,
        codeSplitting: {
          groups: codeSplittingGroups
        }
      }
    }
  }
}
