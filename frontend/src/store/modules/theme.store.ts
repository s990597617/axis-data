import { defineStore } from 'pinia'
import { computed, reactive, ref, watch } from 'vue'

import { defaultThemeConfig } from '@/config/theme.config'
import { STORAGE_KEYS } from '@/config/storage-keys'
import type { ThemeConfig, ThemeMode } from '@/shared/types/app-theme'

/**
 * 读取持久化主题配置
 *
 * @description
 * 兼容早期版本存在的 `darkMode` 字段：若发现旧字段，将其映射为新的 `themeMode`。
 */
function readPersisted(): ThemeConfig {
  try {
    const raw = localStorage.getItem(STORAGE_KEYS.THEME_CONFIG)
    if (!raw) return { ...defaultThemeConfig }
    const parsed = JSON.parse(raw) as Partial<ThemeConfig> & { darkMode?: boolean }
    if (parsed && typeof parsed.darkMode === 'boolean' && !parsed.themeMode) {
      parsed.themeMode = parsed.darkMode ? 'dark' : 'light'
    }
    delete parsed.darkMode
    return { ...defaultThemeConfig, ...parsed }
  } catch {
    return { ...defaultThemeConfig }
  }
}

/**
 * 读取系统当前暗色偏好
 */
function readSystemDark(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-color-scheme: dark)').matches
  )
}

/**
 * 主题 Store
 *
 * @description
 * 承载主题相关的用户偏好：
 * - 主色、圆角
 * - 主题模式（亮 / 暗 / 跟随系统）
 * - 侧边栏 / 顶部栏独立主题与自定义色
 * - 色弱、灰色模式
 *
 * 组件应通过 `useTheme` 或本 store 暴露的计算属性消费，不直接操作 localStorage。
 */
export const useThemeStore = defineStore('theme', () => {
  const config = reactive<ThemeConfig>(readPersisted())

  /** 系统当前是否为暗色（响应 `prefers-color-scheme` 变化） */
  const systemDark = ref(readSystemDark())

  if (typeof window !== 'undefined' && typeof window.matchMedia === 'function') {
    const media = window.matchMedia('(prefers-color-scheme: dark)')
    const listener = (event: MediaQueryListEvent) => {
      systemDark.value = event.matches
    }
    media.addEventListener?.('change', listener)
  }

  /** 实际生效的暗色状态（由 themeMode + 系统偏好共同决定） */
  const darkMode = computed(() => {
    if (config.themeMode === 'auto') return systemDark.value
    return config.themeMode === 'dark'
  })

  const primaryColor = computed(() => config.primaryColor)
  const borderRadius = computed(() => config.borderRadius)
  const themeMode = computed(() => config.themeMode)

  function setPrimaryColor(color: string) {
    config.primaryColor = color
  }

  function setBorderRadius(radius: number) {
    config.borderRadius = radius
  }

  function setThemeMode(mode: ThemeMode) {
    config.themeMode = mode
  }

  function update(patch: Partial<ThemeConfig>) {
    Object.assign(config, patch)
  }

  function reset() {
    Object.assign(config, defaultThemeConfig)
  }

  watch(
    () => ({ ...config }),
    value => {
      try {
        localStorage.setItem(STORAGE_KEYS.THEME_CONFIG, JSON.stringify(value))
      } catch {
        // 忽略 storage 异常
      }
    },
    { deep: true }
  )

  return {
    config,
    primaryColor,
    borderRadius,
    themeMode,
    darkMode,
    systemDark,
    setPrimaryColor,
    setBorderRadius,
    setThemeMode,
    update,
    reset
  }
})
