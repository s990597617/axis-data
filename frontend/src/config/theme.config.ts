import type { ThemeConfig } from '@/shared/types/app-theme'

/**
 * 默认主题配置
 */
export const defaultThemeConfig: ThemeConfig = {
  primaryColor: '#3AA675',
  borderRadius: 6,
  themeMode: 'light',
  sidebarTheme: 'light',
  sidebarColor: '',
  topbarTheme: 'light',
  topbarColor: '',
  colorWeak: false,
  grayMode: false
}

/**
 * 预设主题色
 *
 * @description
 * 设置抽屉中的调色板快选项。用户仍然可以通过自定义色值覆盖。
 */
export interface PresetColor {
  /** 预设色的展示名称 */
  label: string
  /** 主色十六进制值 */
  value: string
}

export const presetPrimaryColors: readonly PresetColor[] = [
  { label: '山岚绿', value: '#3AA675' },
  { label: '海潮蓝', value: '#2080f0' },
  { label: '活力橙', value: '#f0a020' },
  { label: '玫瑰红', value: '#d03050' },
  { label: '紫罗兰', value: '#7e57c2' },
  { label: '沉稳青', value: '#0ca678' },
  { label: '深邃靛', value: '#4f46e5' },
  { label: '墨石黑', value: '#1f2937' }
] as const

/**
 * 圆角预设（px）
 *
 * @description
 * 抽屉中以连体按钮形式提供的圆角快选值；用户也可通过其它手段传入任意值。
 */
export const BORDER_RADIUS_OPTIONS: readonly number[] = [0, 4, 6, 8, 12] as const

// Storage key 集中写在 `@/config/storage-keys.ts`
