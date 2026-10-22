import { storeToRefs } from 'pinia'
import { type InjectionKey, type Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useLayoutReload } from '@/layouts/composables/use-layout-reload'
import { buildRouteTree, collectLeafNodes } from '@/router/helpers/route-tree'
import { staticRoutes } from '@/router/routes/static-routes'
import type { AppRouteRecord } from '@/shared/types/app-route'
import { usePermissionStore } from '@/store/modules/permission.store'
import { useTabsStore, type TabItem } from '@/store/modules/tabs.store'

/**
 * @deprecated 保留以兼容既有引用；实际刷新能力已改为基于 `useLayoutReload` 的模块级注册。
 */
export const LayoutReloadKey: InjectionKey<() => Promise<void> | void> = Symbol('LayoutReload')

/**
 * 标签栏消费 Hook
 *
 * @description
 * 将 `tabs.store` 与当前路由联动，同时提供标签页右键/关闭操作的路由跳转封装。
 * 推荐在布局层 `layout-content` 或 `layout-tabs` 内调用一次即可。
 */
export function useLayoutTabs() {
  const route = useRoute()
  const router = useRouter()
  const tabsStore = useTabsStore()
  const permissionStore = usePermissionStore()
  const { reload } = useLayoutReload()
  const { tabs, activeFullPath, cachedNames } = storeToRefs(tabsStore)

  // 首次调用时把静态 + 动态路由中所有 affix 叶子预置到前面
  const allRoutes: AppRouteRecord[] = [
    ...(staticRoutes as AppRouteRecord[]),
    ...permissionStore.accessRoutes
  ]
  const affixNodes = collectLeafNodes(buildRouteTree(allRoutes)).filter(node => node.affix)
  tabsStore.initAffixTabs(
    affixNodes.map(node => ({
      name: node.name,
      path: node.path,
      title: node.title,
      icon: node.icon,
      keepAlive: node.keepAlive
    }))
  )

  /**
   * 当前路由变化时同步到 tabs
   */
  watch(
    () => route.fullPath,
    () => {
      // 登录页、异常页等独立布局不加入标签栏
      if (route.meta?.layout === 'full') return
      tabsStore.addTab(route)
    },
    { immediate: true }
  )

  /**
   * 切换激活标签
   */
  function activateTab(tab: TabItem) {
    if (tab.fullPath === activeFullPath.value) return
    router.push(tab.fullPath)
  }

  /**
   * 关闭并跳转到下一个激活标签
   *
   * @description
   * 需要在调用 store 之前捕获 `wasActive`，因为 store 内部会立刻把 `activeFullPath` 置为下一项，
   * 如果放在 store 调用之后比较，相等判断恒为 false，就不会触发路由跳转。
   */
  function closeTab(fullPath: string) {
    const wasActive = fullPath === activeFullPath.value
    const next = tabsStore.closeTab(fullPath)
    if (wasActive && next) {
      router.push(next.fullPath)
    }
  }

  function closeOthers(fullPath: string) {
    const next = tabsStore.closeOthers(fullPath)
    if (next) router.push(next.fullPath)
  }

  function closeLeft(fullPath: string) {
    const next = tabsStore.closeLeft(fullPath)
    if (next) router.push(next.fullPath)
  }

  function closeRight(fullPath: string) {
    const next = tabsStore.closeRight(fullPath)
    if (next) router.push(next.fullPath)
  }

  function closeAll() {
    const next = tabsStore.closeAll()
    if (next) router.push(next.fullPath)
    else router.push('/')
  }

  /**
   * 刷新当前标签
   *
   * @description
   * 通过 `useLayoutReload` 注册表调用 `layout-content` 注入的局部刷新函数；
   * 当内容组件尚未挂载或未注册时回退为整页刷新。
   */
  async function refresh() {
    await reload()
  }

  return {
    tabs: tabs as Ref<TabItem[]>,
    activeFullPath,
    cachedNames,
    activateTab,
    closeTab,
    closeOthers,
    closeLeft,
    closeRight,
    closeAll,
    refresh
  }
}
