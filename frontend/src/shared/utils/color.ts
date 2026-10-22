/**
 * 颜色工具
 *
 * @description
 * 基于 RGB 的极简明度调整工具，用于从主色派生 hover / pressed / suppl 色。
 * 如后续接入更复杂的色彩体系（HSL / OKLCH），可在此集中替换实现。
 */

/**
 * 解析十六进制颜色为 RGB 通道
 */
function parseHex(hex: string): [number, number, number] {
  const normalized = hex.replace('#', '').trim()
  const full =
    normalized.length === 3
      ? normalized
          .split('')
          .map(c => c + c)
          .join('')
      : normalized
  const int = parseInt(full, 16)
  return [(int >> 16) & 255, (int >> 8) & 255, int & 255]
}

/**
 * 将 RGB 通道转换为十六进制字符串
 */
function toHex(r: number, g: number, b: number): string {
  const clamp = (v: number) => Math.max(0, Math.min(255, Math.round(v)))
  const toChannel = (v: number) => clamp(v).toString(16).padStart(2, '0')
  return `#${toChannel(r)}${toChannel(g)}${toChannel(b)}`
}

/**
 * 调整颜色亮度
 *
 * @param hex 原始颜色
 * @param amount 正数向白色靠拢，负数向黑色靠拢，范围 [-1, 1]
 */
export function adjustBrightness(hex: string, amount: number): string {
  const [r, g, b] = parseHex(hex)
  const target = amount >= 0 ? 255 : 0
  const ratio = Math.abs(amount)
  return toHex(r + (target - r) * ratio, g + (target - g) * ratio, b + (target - b) * ratio)
}

/**
 * 将十六进制颜色转换为 `rgba()` 字符串
 */
export function hexToRgba(hex: string, alpha: number): string {
  const [r, g, b] = parseHex(hex)
  return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

/**
 * 主色派生调色板
 *
 * @description
 * 以主色派生出 naive-ui `common` 中对应的 hover / pressed / suppl 四档颜色。
 * 任何需要一致主色派生的消费点（NConfigProvider / 离散 API / CSS 变量注入）都应通过此函数，
 * 避免不同位置使用不同的偏移常量导致配色漂移。
 */
export interface PrimaryPalette {
  primaryColor: string
  primaryColorHover: string
  primaryColorPressed: string
  primaryColorSuppl: string
}

export function derivePrimaryPalette(hex: string): PrimaryPalette {
  return {
    primaryColor: hex,
    primaryColorHover: adjustBrightness(hex, 0.1),
    primaryColorPressed: adjustBrightness(hex, -0.1),
    primaryColorSuppl: adjustBrightness(hex, 0.15)
  }
}
