import { computed } from 'vue'

import { useAppStore } from '@/store/modules/app.store'

/**
 * 侧边栏折叠态消费 Hook
 *
 * @description
 * 对 `app.store` 的折叠相关状态/动作做一层语义化封装，布局组件不直接访问 store 属性。
 */
export function useLayoutCollapse() {
  const appStore = useAppStore()

  /** 当前是否折叠 */
  const collapsed = computed(() => appStore.collapsed)

  /** 侧边栏实际宽度（px） */
  const siderWidth = computed(() =>
    appStore.collapsed ? appStore.layout.siderCollapsedWidth : appStore.layout.siderWidth,
  )

  /**
   * 切换折叠态
   */
  function toggle() {
    appStore.toggleCollapsed()
  }

  /**
   * 显式设置折叠态
   */
  function setCollapsed(value: boolean) {
    appStore.setCollapsed(value)
  }

  return {
    collapsed,
    siderWidth,
    toggle,
    setCollapsed,
  }
}
