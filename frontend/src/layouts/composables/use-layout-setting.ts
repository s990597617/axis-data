import { ref } from 'vue'

/**
 * 设置抽屉的全局显隐状态
 *
 * @description
 * 模块级 ref，允许 `layout-header` 与 `layout-setting` 跨组件共享同一份显隐状态。
 * 不放入 store 是因为这只是 UI 级临时状态，无需持久化。
 */
const visible = ref(false)

/**
 * 布局设置抽屉控制 Hook
 */
export function useLayoutSetting() {
  function open() {
    visible.value = true
  }

  function close() {
    visible.value = false
  }

  function toggle() {
    visible.value = !visible.value
  }

  return {
    visible,
    open,
    close,
    toggle,
  }
}
