import { defineStore } from 'pinia'
import { computed, reactive, watch } from 'vue'

import { defaultLayoutConfig } from '@/config/layout.config'
import { STORAGE_KEYS } from '@/config/storage-keys'
import type { LayoutConfig, LayoutMode } from '@/shared/types/app-layout'

/**
 * 读取持久化的布局配置
 *
 * @description
 * 读取失败或字段缺失时，会自动与 `defaultLayoutConfig` 合并，保证新增字段向后兼容。
 */
function readPersistedConfig(): LayoutConfig {
  try {
    const raw = localStorage.getItem(STORAGE_KEYS.LAYOUT_CONFIG)
    if (!raw) return { ...defaultLayoutConfig }
    const parsed = JSON.parse(raw) as Partial<LayoutConfig>
    return { ...defaultLayoutConfig, ...parsed }
  } catch {
    return { ...defaultLayoutConfig }
  }
}

/**
 * 应用全局状态 Store
 *
 * @description
 * 承载布局配置、折叠状态、布局模式等系统级偏好。
 * 布局层与公共组件仅消费本 store 暴露的只读状态与 action，不直接访问 localStorage。
 */
export const useAppStore = defineStore('app', () => {
  const layout = reactive<LayoutConfig>(readPersistedConfig())

  /** 是否处于折叠态（只读计算属性） */
  const collapsed = computed(() => layout.collapsed)
  /** 当前布局模式（只读计算属性） */
  const layoutMode = computed(() => layout.mode)

  /**
   * 设置布局模式
   */
  function setLayoutMode(mode: LayoutMode) {
    layout.mode = mode
  }

  /**
   * 切换侧边栏折叠态
   */
  function toggleCollapsed() {
    layout.collapsed = !layout.collapsed
  }

  /**
   * 显式设置侧边栏折叠态
   */
  function setCollapsed(value: boolean) {
    layout.collapsed = value
  }

  /**
   * 批量更新布局配置
   */
  function updateLayout(patch: Partial<LayoutConfig>) {
    Object.assign(layout, patch)
  }

  /**
   * 重置为默认布局配置
   */
  function resetLayout() {
    Object.assign(layout, defaultLayoutConfig)
  }

  // 持久化：任意布局字段变更都同步写入 localStorage
  watch(
    () => ({ ...layout }),
    value => {
      try {
        localStorage.setItem(STORAGE_KEYS.LAYOUT_CONFIG, JSON.stringify(value))
      } catch {
        // localStorage 不可用时静默忽略
      }
    },
    { deep: true }
  )

  return {
    layout,
    collapsed,
    layoutMode,
    setLayoutMode,
    toggleCollapsed,
    setCollapsed,
    updateLayout,
    resetLayout
  }
})
