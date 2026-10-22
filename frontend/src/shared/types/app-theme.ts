/**
 * 主题模式
 *
 * @description
 * - `light` 始终亮色
 * - `dark` 始终暗色
 * - `auto` 跟随系统 `prefers-color-scheme`
 */
export type ThemeMode = 'light' | 'dark' | 'auto'

/**
 * 表面主题（侧边栏 / 顶部栏）
 */
export type SurfaceTheme = 'light' | 'dark'

/**
 * 应用主题配置
 *
 * @description
 * 承载运行时主题相关的用户偏好，与布局配置分离。
 * 由 `theme.store` 持有并持久化，最终通过 `NConfigProvider` 下发到整个应用。
 */
export interface ThemeConfig {
  /** 品牌主色（十六进制，大小写不敏感） */
  primaryColor: string
  /** 组件圆角（px） */
  borderRadius: number
  /** 主题模式 */
  themeMode: ThemeMode
  /** 侧边栏配色（亮 / 暗） */
  sidebarTheme: SurfaceTheme
  /** 侧边栏自定义背景色；为空时使用 `sidebarTheme` 默认值 */
  sidebarColor: string
  /** 顶部栏配色（亮 / 暗） */
  topbarTheme: SurfaceTheme
  /** 顶部栏自定义背景色；为空时使用 `topbarTheme` 默认值 */
  topbarColor: string
  /** 色弱模式 */
  colorWeak: boolean
  /** 灰色模式（哀悼模式） */
  grayMode: boolean
}
