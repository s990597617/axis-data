import { darkTheme, type GlobalTheme, type GlobalThemeOverrides } from 'naive-ui'
import { computed, watchEffect } from 'vue'

import { derivePrimaryPalette, hexToRgba } from '@/shared/utils/color'
import { useAppStore } from '@/store/modules/app.store'
import { useThemeStore } from '@/store/modules/theme.store'

/**
 * 应用主题消费 Hook
 *
 * @description
 * 根据 `theme.store` 的主题偏好计算 naive-ui 的 `theme` 与 `themeOverrides`，
 * 同时同步 `<html>` 的 class 与 `--axis-*` CSS 变量，便于业务样式复用主题色。
 * 建议在应用根部（`App.vue`）调用一次。
 */
export function useTheme() {
  const themeStore = useThemeStore()
  const appStore = useAppStore()

  /** naive-ui 主题对象：`null` 表示亮色，`darkTheme` 表示暗色 */
  const theme = computed<GlobalTheme | null>(() => (themeStore.darkMode ? darkTheme : null))

  /**
   * naive-ui 主题覆盖
   *
   * @description
   * 以主色派生 hover / pressed / suppl 四个状态色，并统一应用圆角。
   * 细粒度颜色（如 info / warning）保持 naive 默认值。
   */
  /**
   * 全局字体栈
   *
   * @description
   * - 优先使用操作系统原生界面字体，保证 Windows / macOS 小字号渲染清晰
   * - Noto Sans SC 仅作为缺少中文系统字体时的 fallback
   * - 等宽字体栈单独维护，代码块 / 图标名等场景可继续用 `fontFamilyMono`
   */
  const fontFamily =
    'system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI Variable", "Segoe UI", ' +
    '"Microsoft YaHei UI", "Microsoft YaHei", "PingFang SC", "Hiragino Sans GB", ' +
    '"Noto Sans SC", Arial, sans-serif'
  const fontFamilyMono =
    'ui-monospace, "SF Mono", "Fira Code", Menlo, Consolas, "Courier New", monospace'

  const themeOverrides = computed<GlobalThemeOverrides>(() => {
    const palette = derivePrimaryPalette(themeStore.primaryColor)
    return {
      common: {
        ...palette,
        borderRadius: `${themeStore.borderRadius}px`,
        borderRadiusSmall: `${Math.max(0, themeStore.borderRadius - 2)}px`,
        fontFamily,
        fontFamilyMono,
        fontWeight: '400',
        fontWeightStrong: '600',
        fontSizeMini: '12px',
        fontSizeTiny: '12px',
        fontSizeSmall: '14px',
        fontSizeMedium: '14px',
        fontSizeLarge: '15px',
        fontSizeHuge: '16px',
        heightMini: '24px',
        heightTiny: '26px',
        heightSmall: '28px',
        heightMedium: '32px',
        heightLarge: '36px'
      },
      Button: {
        fontSizeTiny: '12px',
        fontSizeSmall: '13px',
        fontSizeMedium: '14px'
      },
      DataTable: {
        fontSizeSmall: '13px',
        fontSizeMedium: '14px'
      },
      Input: {
        fontSizeSmall: '13px',
        fontSizeMedium: '14px'
      },
      Select: {
        fontSizeSmall: '13px',
        fontSizeMedium: '14px'
      },
      Form: {
        labelFontSizeLeftSmall: '13px',
        labelFontSizeLeftMedium: '14px'
      },
      Pagination: {
        itemFontSizeSmall: '13px',
        itemFontSizeMedium: '14px'
      }
    }
  })

  /**
   * 亮 / 暗色对应的表面色板
   *
   * @description
   * 用于业务自写 SCSS 中的非 naive-ui 原生组件（如自定义 header / tabs 等）保持主题一致。
   * naive-ui 的 `--n-*` 变量只对 naive 组件生效，此处补齐全局可用的 `--axis-*` 变量。
   */
  const surfaces = {
    light: {
      bg: '#f5f6fb',
      card: '#ffffff',
      border: '#efeff5',
      action: '#f5f5f5',
      text1: 'rgba(0, 0, 0, 0.88)',
      text2: 'rgba(0, 0, 0, 0.65)',
      text3: 'rgba(0, 0, 0, 0.45)',
      railBg: '#001529'
    },
    dark: {
      bg: '#101014',
      card: '#18181c',
      border: '#2c2c32',
      action: '#2c2c32',
      text1: 'rgba(255, 255, 255, 0.9)',
      text2: 'rgba(255, 255, 255, 0.7)',
      text3: 'rgba(255, 255, 255, 0.45)',
      railBg: '#0b0b0e'
    }
  } as const

  /**
   * 解析表面实际背景 / 文字色
   *
   * @param theme 预设主题（light / dark）
   * @param override 自定义十六进制色；为空时落到预设
   */
  function resolveSurface(theme: 'light' | 'dark', override: string) {
    const preset = theme === 'dark' ? '#1b2029' : '#ffffff'
    const bg = override?.trim() ? override : preset
    // 暗色表面上的文字使用白色系，亮色表面使用黑色系
    const text = theme === 'dark' ? 'rgba(255, 255, 255, 0.9)' : 'rgba(0, 0, 0, 0.88)'
    const textSecondary = theme === 'dark' ? 'rgba(255, 255, 255, 0.7)' : 'rgba(0, 0, 0, 0.65)'
    const border = theme === 'dark' ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.06)'
    const action = theme === 'dark' ? 'rgba(255, 255, 255, 0.08)' : 'rgba(0, 0, 0, 0.04)'
    return { bg, text, textSecondary, border, action }
  }

  /**
   * 同步 DOM：`<html>` 的 class + CSS 变量 + 滤镜
   */
  watchEffect(() => {
    const root = document.documentElement
    const surface = themeStore.darkMode ? surfaces.dark : surfaces.light
    root.classList.toggle('dark', themeStore.darkMode)
    root.classList.toggle('light', !themeStore.darkMode)

    const primary = themeStore.primaryColor
    const palette = derivePrimaryPalette(primary)
    root.style.setProperty('--axis-primary-color', palette.primaryColor)
    root.style.setProperty('--axis-primary-color-hover', palette.primaryColorHover)
    root.style.setProperty('--axis-primary-color-pressed', palette.primaryColorPressed)
    root.style.setProperty('--axis-primary-color-suppl', palette.primaryColorSuppl)
    root.style.setProperty('--axis-primary-color-soft', hexToRgba(primary, 0.12))
    root.style.setProperty('--axis-border-radius', `${themeStore.borderRadius}px`)

    root.style.setProperty('--axis-bg-color', surface.bg)
    root.style.setProperty('--axis-card-color', surface.card)
    root.style.setProperty('--axis-border-color', surface.border)
    root.style.setProperty('--axis-action-color', surface.action)
    root.style.setProperty('--axis-text-color-1', surface.text1)
    root.style.setProperty('--axis-text-color-2', surface.text2)
    root.style.setProperty('--axis-text-color-3', surface.text3)
    root.style.setProperty('--axis-rail-bg', surface.railBg)

    // 侧边栏 & 顶部栏独立表面
    const sider = resolveSurface(themeStore.config.sidebarTheme, themeStore.config.sidebarColor)
    root.style.setProperty('--axis-sider-bg', sider.bg)
    root.style.setProperty('--axis-sider-text', sider.text)
    root.style.setProperty('--axis-sider-text-2', sider.textSecondary)
    root.style.setProperty('--axis-sider-border', sider.border)
    root.style.setProperty('--axis-sider-action', sider.action)

    const topbar = resolveSurface(themeStore.config.topbarTheme, themeStore.config.topbarColor)
    root.style.setProperty('--axis-header-bg', topbar.bg)
    root.style.setProperty('--axis-header-text', topbar.text)
    root.style.setProperty('--axis-header-text-2', topbar.textSecondary)
    root.style.setProperty('--axis-header-border', topbar.border)
    root.style.setProperty('--axis-header-action', topbar.action)

    // 色弱 / 灰色 滤镜
    const filters: string[] = []
    if (themeStore.config.grayMode) filters.push('grayscale(1)')
    if (themeStore.config.colorWeak) filters.push('saturate(0.3)')
    root.style.setProperty('--axis-filter', filters.length ? filters.join(' ') : 'none')

    // 布局尺寸
    root.style.setProperty('--axis-header-height', `${appStore.layout.headerHeight}px`)
    root.style.setProperty('--axis-tabs-height', `${appStore.layout.tabsHeight}px`)
  })

  return {
    theme,
    themeOverrides
  }
}
