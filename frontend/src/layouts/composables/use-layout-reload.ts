import { shallowRef } from 'vue'

/**
 * 当前生效的局部刷新函数
 *
 * @description
 * 使用模块级 `shallowRef` 作为注册表，避免 provide/inject 受组件树拓扑限制。
 * `layout-content` 挂载时将自己的刷新能力写入；任意处调用 `reload()` 即可触发。
 */
const reloadFn = shallowRef<(() => Promise<void> | void) | null>(null)

/**
 * 局部刷新能力 Hook
 *
 * @description
 * - `registerReload(fn)`：由 `layout-content` 注册可刷新的路由视图
 * - `reload()`：由刷新按钮等调用；未注册时回退为整页刷新，保证行为可用
 */
export function useLayoutReload() {
  function registerReload(fn: (() => Promise<void> | void) | null) {
    reloadFn.value = fn
  }

  async function reload() {
    const fn = reloadFn.value
    if (fn) {
      await fn()
    } else {
      window.location.reload()
    }
  }

  return {
    registerReload,
    reload,
  }
}
