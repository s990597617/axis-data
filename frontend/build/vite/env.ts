import { loadEnv, type ConfigEnv } from 'vite'

export interface ViteAppEnv {
  /** 当前 Vite 运行模式。 */
  mode: string
  /** 当前命令是否为生产构建。 */
  isBuild: boolean
  /** 当前模式是否为生产环境。 */
  isProd: boolean
  /** 应用展示名称。 */
  appTitle: string
  /** 应用公共基础路径，对应 Vite 的 base 配置。 */
  publicPath: string
  /** 开发服务器端口。 */
  port: number
  /** 接口统一前缀。 */
  apiPrefix: string
  /** 生产或直连模式下的接口基础地址。 */
  apiBaseUrl: string
  /** 是否启用 Vite 开发代理。 */
  useProxy: boolean
  /** Vite 开发代理目标地址。 */
  proxyTarget: string
  /** 是否开启构建产物分析。 */
  enableAnalyze: boolean
}

function parseBoolean(value: string | undefined, defaultValue = false): boolean {
  if (value === undefined || value === '') return defaultValue
  return value === 'true'
}

function parsePort(value: string | undefined, defaultValue = 3000): number {
  const port = Number(value)
  return Number.isInteger(port) && port > 0 ? port : defaultValue
}

function normalizePublicPath(value: string | undefined): string {
  if (!value) return '/'
  if (value === '/') return '/'

  const withLeadingSlash = value.startsWith('/') ? value : `/${value}`
  return withLeadingSlash.endsWith('/') ? withLeadingSlash : `${withLeadingSlash}/`
}

/**
 * 读取并规范化 Vite 环境变量。
 *
 * Vite 从 `.env*` 文件读取到的值都是字符串，本函数负责将它们转换为
 * 构建配置真正需要的类型，并集中提供默认值。后续新增环境变量时，应优先
 * 在这里完成解析，避免在各个配置模块里重复处理字符串。
 */
export function resolveViteEnv(configEnv: ConfigEnv): ViteAppEnv {
  const rawEnv = loadEnv(configEnv.mode, process.cwd())

  return {
    mode: configEnv.mode,
    isBuild: configEnv.command === 'build',
    isProd: configEnv.mode === 'production',
    appTitle: rawEnv.VITE_APP_TITLE || '云枢 Axis · MES',
    publicPath: normalizePublicPath(rawEnv.VITE_PUBLIC_PATH),
    port: parsePort(rawEnv.VITE_PORT),
    apiPrefix: rawEnv.VITE_API_PREFIX || '/api',
    apiBaseUrl: rawEnv.VITE_API_BASE_URL || '',
    useProxy: parseBoolean(rawEnv.VITE_USE_PROXY, configEnv.command === 'serve'),
    proxyTarget: rawEnv.VITE_PROXY_TARGET || '',
    enableAnalyze: parseBoolean(rawEnv.VITE_ANALYZE) || process.env.ANALYZE === 'true'
  }
}
