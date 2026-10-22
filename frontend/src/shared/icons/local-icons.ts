import { addIcon } from '@iconify/vue'

/**
 * 本地 SVG 前缀
 *
 * @description
 * 使用方式：`<Icon icon="axis:<filename>" />`
 * 例如 `src/assets/icons/logo.svg` -> `axis:logo`
 */
const LOCAL_PREFIX = 'axis'

/**
 * 构建时静态扫描本地 SVG
 *
 * - `eager: true`：打包时全部同步内联，启动即可用
 * - `query: '?raw'`：拿到 SVG 源文本
 *
 * @remarks
 * 文件命名决定图标名：建议使用短横线命名（kebab-case），例如 `arrow-up.svg` -> `axis:arrow-up`。
 */
const SVG_MODULES = import.meta.glob('/src/assets/icons/**/*.svg', {
  query: '?raw',
  import: 'default',
  eager: true,
}) as Record<string, string>

const VIEW_BOX_RE = /viewBox\s*=\s*["']([^"']+)["']/i
const WIDTH_RE = /\bwidth\s*=\s*["']?([\d.]+)/i
const HEIGHT_RE = /\bheight\s*=\s*["']?([\d.]+)/i
const SVG_OPEN_RE = /<svg\b[^>]*>/i
const SVG_CLOSE_RE = /<\/svg>\s*$/i
const HEX_FILL_RE = /fill=(["'])#[0-9a-f]{3,8}\1/gi

/**
 * 本地 SVG 集成配置
 */
export interface LocalIconsOptions {
  /**
   * 是否把 body 中硬编码的 `fill="#xxxxxx"` 统一替换为 `fill="currentColor"`
   * 让图标可以通过 CSS `color` 跟随主题色
   *
   * @default true
   */
  forceCurrentColor?: boolean

  /**
   * 即使开启 `forceCurrentColor`，也要**保留原色**的图标
   *
   * @description
   * - 数组形式：精确命中图标名（不含 `axis:` 前缀，支持子目录短横线形式，如 `brand-github`）
   * - 函数形式：返回 true 表示跳过替换
   *
   * @example
   * preserveColorFor: ['axis-logo', 'brand-github']
   * preserveColorFor: (name) => name.startsWith('brand-')
   */
  preserveColorFor?: string[] | ((name: string) => boolean)
}

const DEFAULT_OPTIONS: Required<Pick<LocalIconsOptions, 'forceCurrentColor'>> = {
  forceCurrentColor: true,
}

function shouldPreserveColor(
  name: string,
  preserve: LocalIconsOptions['preserveColorFor'],
): boolean {
  if (!preserve) return false
  if (typeof preserve === 'function') return preserve(name)
  return preserve.includes(name)
}

interface ParsedSvg {
  body: string
  width: number
  height: number
}

/**
 * 从原始 SVG 字符串解析出 iconify 需要的数据
 */
function parseSvg(raw: string): ParsedSvg {
  const openTagMatch = raw.match(SVG_OPEN_RE)
  const openTag = openTagMatch?.[0] ?? ''

  let width = 24
  let height = 24

  const viewBoxMatch = openTag.match(VIEW_BOX_RE)
  if (viewBoxMatch?.[1]) {
    const parts = viewBoxMatch[1].trim().split(/[\s,]+/)
    if (parts.length === 4) {
      const w = Number(parts[2])
      const h = Number(parts[3])
      if (Number.isFinite(w)) width = w
      if (Number.isFinite(h)) height = h
    }
  } else {
    const widthMatch = openTag.match(WIDTH_RE)
    const heightMatch = openTag.match(HEIGHT_RE)
    if (widthMatch) width = Number(widthMatch[1]) || width
    if (heightMatch) height = Number(heightMatch[1]) || height
  }

  const body = raw.replace(SVG_OPEN_RE, '').replace(SVG_CLOSE_RE, '').trim()

  return { body, width, height }
}

/**
 * 从文件路径生成图标名（不含前缀）
 * 支持多级目录：`icons/brand/github.svg` -> `brand-github`
 */
function resolveIconName(path: string): string {
  const relative = path.replace(/^\/?src\/assets\/icons\//, '').replace(/\.svg$/i, '')
  return relative.replace(/\//g, '-')
}

/**
 * 把 `src/assets/icons` 下的 SVG 全部注册为 iconify 图标
 *
 * @param options 见 {@link LocalIconsOptions}
 */
export function registerLocalIcons(options: LocalIconsOptions = {}) {
  const forceCurrentColor = options.forceCurrentColor ?? DEFAULT_OPTIONS.forceCurrentColor

  Object.entries(SVG_MODULES).forEach(([path, raw]) => {
    try {
      const { body, width, height } = parseSvg(raw)
      const name = resolveIconName(path)

      const needNormalize =
        forceCurrentColor && !shouldPreserveColor(name, options.preserveColorFor)

      // 1. 将硬编码 fill="#xxx" 统一替换为 currentColor
      // 2. 外层再包一个 <g fill="currentColor">，为 **未声明 fill** 的子元素提供继承默认值
      //    未显式声明 fill 的 path，SVG 规范默认黑色 —— 不做这步的话颜色无法跟随
      const finalBody = needNormalize
        ? `<g fill="currentColor">${body.replace(HEX_FILL_RE, 'fill="currentColor"')}</g>`
        : body

      addIcon(`${LOCAL_PREFIX}:${name}`, { body: finalBody, width, height })
    } catch (error) {
      console.warn(`[axis-icons] 注册本地 SVG 失败：${path}`, error)
    }
  })
}
