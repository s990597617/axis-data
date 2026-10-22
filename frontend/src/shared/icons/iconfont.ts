import { addIcon } from '@iconify/vue'

/**
 * 阿里 iconfont Symbol JS 前缀
 *
 * @description
 * 使用方式：`<Icon icon="iconfont:<name>" />`
 * 其中 `<name>` 对应 iconfont 里的图标 id 去掉 `icon-` 前缀。
 * 例如 id="icon-alipay" -> `iconfont:alipay`
 */
const ICONFONT_PREFIX = 'iconfont'

/**
 * 构建时静态扫描 iconfont Symbol JS
 *
 * @description
 * 把从 iconfont.cn 下载的 `iconfont.js`（Symbol 方式）放到 `src/assets/iconfont/` 下，
 * 本模块会把里面的 `<symbol>` 批量转换为 iconify 图标。
 *
 * 支持同时放多个 JS（例如多个 iconfont 项目），文件名不影响结果。
 */
const ICONFONT_MODULES = import.meta.glob('/src/assets/iconfont/*.js', {
  query: '?raw',
  import: 'default',
  eager: true,
}) as Record<string, string>

const SYMBOL_RE = /<symbol\b([^>]*)>([\s\S]*?)<\/symbol>/gi
const ID_RE = /\bid\s*=\s*["']([^"']+)["']/i
const VIEW_BOX_RE = /\bviewBox\s*=\s*["']([^"']+)["']/i
const HEX_FILL_RE = /fill=(["'])#[0-9a-f]{3,8}\1/gi

/**
 * iconfont 集成配置
 */
export interface IconfontOptions {
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
   * - 数组形式：精确命中图标名（已剥掉 `icon-` 前缀）
   * - 函数形式：返回 true 表示跳过替换
   *
   * @example
   * preserveColorFor: ['brand-logo', 'alipay']
   * preserveColorFor: (name) => name.startsWith('brand-')
   */
  preserveColorFor?: string[] | ((name: string) => boolean)
}

const DEFAULT_OPTIONS: Required<Pick<IconfontOptions, 'forceCurrentColor'>> = {
  forceCurrentColor: true,
}

function shouldPreserveColor(name: string, preserve: IconfontOptions['preserveColorFor']): boolean {
  if (!preserve) return false
  if (typeof preserve === 'function') return preserve(name)
  return preserve.includes(name)
}

interface Parsed {
  name: string
  body: string
  width: number
  height: number
}

/**
 * 从 symbol 元素属性中解析 viewBox，默认 1024x1024（iconfont 常用规格）
 */
function parseSize(attrs: string): { width: number; height: number } {
  const viewBoxMatch = attrs.match(VIEW_BOX_RE)
  if (viewBoxMatch?.[1]) {
    const parts = viewBoxMatch[1].trim().split(/[\s,]+/)
    if (parts.length === 4) {
      const w = Number(parts[2])
      const h = Number(parts[3])
      if (Number.isFinite(w) && Number.isFinite(h)) {
        return { width: w, height: h }
      }
    }
  }
  return { width: 1024, height: 1024 }
}

/**
 * 从一段 iconfont JS 文本中提取所有 symbol
 */
function extractSymbols(source: string, options: IconfontOptions): Parsed[] {
  const results: Parsed[] = []
  const forceCurrentColor = options.forceCurrentColor ?? DEFAULT_OPTIONS.forceCurrentColor

  for (const match of source.matchAll(SYMBOL_RE)) {
    const attrs = match[1] ?? ''
    const body = (match[2] ?? '').trim()

    const idMatch = attrs.match(ID_RE)
    if (!idMatch?.[1]) continue

    // iconfont 里 id 一般是 "icon-xxx"，剥离前缀对齐使用习惯
    const rawId = idMatch[1]
    const name = rawId.replace(/^icon[-_]/i, '')

    const needNormalize = forceCurrentColor && !shouldPreserveColor(name, options.preserveColorFor)

    // 1. 将硬编码 fill="#xxx" 统一替换为 currentColor
    // 2. 外层再包一个 <g fill="currentColor">，为 **未声明 fill** 的子元素提供继承默认值
    //    iconfont 的 path 大多不带 fill 属性，SVG 规范默认黑色 —— 不做这步的话颜色无法跟随
    const finalBody = needNormalize
      ? `<g fill="currentColor">${body.replace(HEX_FILL_RE, 'fill="currentColor"')}</g>`
      : body

    const { width, height } = parseSize(attrs)
    results.push({ name, body: finalBody, width, height })
  }

  return results
}

/**
 * 把 `src/assets/iconfont/*.js` 中所有图标注册到 iconify
 *
 * @param options 见 {@link IconfontOptions}
 */
export function registerIconfont(options: IconfontOptions = {}) {
  Object.entries(ICONFONT_MODULES).forEach(([path, source]) => {
    try {
      const icons = extractSymbols(source, options)
      icons.forEach(({ name, body, width, height }) => {
        addIcon(`${ICONFONT_PREFIX}:${name}`, { body, width, height })
      })
    } catch (error) {
      console.warn(`[axis-icons] 解析 iconfont 文件失败：${path}`, error)
    }
  })
}
