import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { STORAGE_KEYS } from '@/config/storage-keys'
import type { AppRouteMeta } from '@/shared/types/app-route'

/**
 * 标签页数据项
 */
export interface TabItem {
  /** 路由名称（同时作为 keep-alive include 的组件名约定） */
  name: string
  /** 路由路径（不含 query/hash） */
  path: string
  /** 完整路径（含 query/hash），作为标签唯一标识 */
  fullPath: string
  /** 标签显示标题 */
  title: string
  /** 标签图标（iconify 名） */
  icon?: string
  /** 是否固定（不可关闭） */
  affix?: boolean
  /** 是否启用页面缓存 */
  keepAlive?: boolean
}

/**
 * 读取持久化标签列表
 */
function readPersistedTabs(): TabItem[] {
  try {
    const raw = sessionStorage.getItem(STORAGE_KEYS.TABS)
    if (!raw) return []
    const parsed = JSON.parse(raw) as TabItem[]
    return Array.isArray(parsed) ? parsed : []
  } catch {
    return []
  }
}

/**
 * 根据路由信息构建标签项
 */
function toTabItem(route: RouteLocationNormalizedLoaded): TabItem | null {
  const name = typeof route.name === 'string' ? route.name : ''
  if (!name) return null
  const meta = route.meta as AppRouteMeta
  return {
    name,
    path: route.path,
    fullPath: route.fullPath,
    title: meta.title ?? name,
    icon: meta.icon,
    affix: meta.affix,
    keepAlive: meta.keepAlive
  }
}

/**
 * 标签页 Store
 *
 * @description
 * 维护已打开的路由标签列表，与面包屑、keep-alive 缓存名单联动。
 * 布局层组件与 composable 仅消费本 store 的状态与 action，不直接操作存储。
 */
export const useTabsStore = defineStore('tabs', () => {
  const tabs = ref<TabItem[]>(readPersistedTabs())
  const activeFullPath = ref<string>(tabs.value[0]?.fullPath ?? '')

  /**
   * keep-alive include 用的组件名数组
   *
   * @description
   * 仅包含当前已打开且声明了 `meta.keepAlive` 的路由 name。
   * 要求页面组件通过 `defineOptions({ name })` 或默认导出设置同名组件名。
   */
  const cachedNames = computed(() => tabs.value.filter(tab => tab.keepAlive).map(tab => tab.name))

  /**
   * 持久化到 sessionStorage
   */
  function persist() {
    try {
      sessionStorage.setItem(STORAGE_KEYS.TABS, JSON.stringify(tabs.value))
    } catch {
      // 忽略
    }
  }

  /**
   * 根据当前路由添加或激活标签
   */
  function addTab(route: RouteLocationNormalizedLoaded) {
    const tab = toTabItem(route)
    if (!tab) return

    const existed = tabs.value.find(item => item.fullPath === tab.fullPath)
    if (!existed) {
      if (tab.affix) {
        // 固定标签始终排在已有固定标签之后、所有普通标签之前
        const lastAffixIndex = [...tabs.value]
          .map((t, i) => ({ t, i }))
          .filter(({ t }) => t.affix)
          .map(({ i }) => i)
          .pop()
        const insertAt = lastAffixIndex === undefined ? 0 : lastAffixIndex + 1
        tabs.value.splice(insertAt, 0, tab)
      } else {
        tabs.value.push(tab)
      }
    } else {
      // 路由重用时可能 title/icon 发生变化，保持同步
      Object.assign(existed, tab)
    }
    activeFullPath.value = tab.fullPath
    persist()
  }

  /**
   * 预加载标记了 `affix` 的所有路由为固定标签
   *
   * @description
   * 应在路由表就绪后调用，保证固定标签在用户首次导航到非固定页面之前就已置顶。
   * 已存在的同名标签不会重复添加。
   */
  function initAffixTabs(affixItems: Omit<TabItem, 'fullPath'>[]) {
    const incoming: TabItem[] = affixItems.map(it => ({
      ...it,
      fullPath: it.path,
      affix: true
    }))
    const merged: TabItem[] = []
    // 按传入顺序排固定项
    for (const item of incoming) {
      if (!merged.find(t => t.fullPath === item.fullPath)) merged.push(item)
    }
    // 追加当前已有的非固定标签，保留用户操作结果
    for (const item of tabs.value) {
      if (!item.affix && !merged.find(t => t.fullPath === item.fullPath)) {
        merged.push(item)
      }
    }
    tabs.value = merged
    persist()
  }

  /**
   * 关闭指定标签
   *
   * @returns 关闭后应激活的标签，若无标签剩余则返回 `null`
   */
  function closeTab(fullPath: string): TabItem | null {
    const index = tabs.value.findIndex(item => item.fullPath === fullPath)
    if (index === -1) return null
    const target = tabs.value[index]
    if (!target || target.affix) return null

    tabs.value.splice(index, 1)
    persist()

    if (activeFullPath.value !== fullPath) {
      return tabs.value.find(item => item.fullPath === activeFullPath.value) ?? null
    }

    const next = tabs.value[index] ?? tabs.value[index - 1] ?? null
    activeFullPath.value = next?.fullPath ?? ''
    return next
  }

  /**
   * 关闭除 `fullPath` 之外的非固定标签
   */
  function closeOthers(fullPath: string): TabItem | null {
    tabs.value = tabs.value.filter(item => item.affix || item.fullPath === fullPath)
    const target = tabs.value.find(item => item.fullPath === fullPath) ?? tabs.value[0] ?? null
    activeFullPath.value = target?.fullPath ?? ''
    persist()
    return target
  }

  /**
   * 关闭 `fullPath` 左侧非固定标签
   */
  function closeLeft(fullPath: string): TabItem | null {
    const index = tabs.value.findIndex(item => item.fullPath === fullPath)
    if (index <= 0) return null
    tabs.value = tabs.value.filter((item, idx) => item.affix || idx >= index)
    const target = tabs.value.find(item => item.fullPath === fullPath) ?? null
    activeFullPath.value = target?.fullPath ?? activeFullPath.value
    persist()
    return target
  }

  /**
   * 关闭 `fullPath` 右侧非固定标签
   */
  function closeRight(fullPath: string): TabItem | null {
    const index = tabs.value.findIndex(item => item.fullPath === fullPath)
    if (index === -1 || index >= tabs.value.length - 1) return null
    tabs.value = tabs.value.filter((item, idx) => item.affix || idx <= index)
    const target = tabs.value.find(item => item.fullPath === fullPath) ?? null
    activeFullPath.value = target?.fullPath ?? activeFullPath.value
    persist()
    return target
  }

  /**
   * 关闭所有非固定标签
   */
  function closeAll(): TabItem | null {
    tabs.value = tabs.value.filter(item => item.affix)
    const target = tabs.value[0] ?? null
    activeFullPath.value = target?.fullPath ?? ''
    persist()
    return target
  }

  /**
   * 重置标签数据（用户登出时调用）
   */
  function reset() {
    tabs.value = []
    activeFullPath.value = ''
    persist()
  }

  return {
    tabs,
    activeFullPath,
    cachedNames,
    addTab,
    initAffixTabs,
    closeTab,
    closeOthers,
    closeLeft,
    closeRight,
    closeAll,
    reset
  }
})
