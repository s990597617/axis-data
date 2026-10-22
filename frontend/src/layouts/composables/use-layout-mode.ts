import { computed, defineAsyncComponent, markRaw, type Component } from 'vue'

import { useAppStore } from '@/store/modules/app.store'
import type { LayoutMode } from '@/shared/types/app-layout'

/**
 * 布局模式 -> 布局组件映射
 *
 * @description
 * 使用 `defineAsyncComponent` 懒加载真实布局组件，避免进入应用时一次性加载全部布局模式。
 * 尚未实现的模式暂时回退到 `vertical-layout`，后续落地时仅需替换对应项。
 */
const layoutMap: Record<LayoutMode, () => Promise<Component>> = {
  vertical: () => import('@/layouts/modes/vertical-layout.vue'),
  horizontal: () => import('@/layouts/modes/horizontal-layout.vue'),
  split: () => import('@/layouts/modes/split-layout.vue'),
  mix: () => import('@/layouts/modes/mix-layout.vue'),
}

/**
 * 布局模式消费 Hook
 *
 * @description
 * 对外暴露当前布局组件以及切换方法，由 `layouts/index.vue` 动态渲染。
 */
export function useLayoutMode() {
  const appStore = useAppStore()

  /** 当前布局组件（响应式） */
  const layoutComponent = computed<Component>(() =>
    markRaw(defineAsyncComponent(layoutMap[appStore.layoutMode])),
  )

  /** 当前布局模式 */
  const mode = computed(() => appStore.layoutMode)

  /**
   * 切换布局模式
   */
  function setMode(next: LayoutMode) {
    appStore.setLayoutMode(next)
  }

  return {
    mode,
    layoutComponent,
    setMode,
  }
}
