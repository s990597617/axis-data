import { computed, type ComputedRef } from 'vue'
import { useRoute } from 'vue-router'

import type { AppRouteMeta } from '@/shared/types/app-route'

/**
 * 面包屑项
 */
export interface BreadcrumbItem {
  /** 路由名称，作为唯一 key */
  name: string
  /** 标题 */
  title: string
  /** 图标（iconify 名） */
  icon?: string
  /** 跳转路径，若为 undefined 表示当前项不可跳转 */
  path?: string
  /** 是否为当前激活项（链路末尾） */
  active: boolean
}

/**
 * 面包屑消费 Hook
 *
 * @description
 * 基于 `route.matched` 生成面包屑链路，自动忽略：
 * - 不存在 `meta.title` 的节点
 * - `meta.hidden` 为 true 且未命中当前激活项的节点
 * - `meta.breadcrumb === false` 的节点
 */
export function useLayoutBreadcrumb(): { items: ComputedRef<BreadcrumbItem[]> } {
  const route = useRoute()

  const items = computed<BreadcrumbItem[]>(() => {
    const matched = route.matched.filter((record) => {
      const meta = record.meta as AppRouteMeta | undefined
      if (!meta?.title) return false
      if (meta.hidden) return false
      if ((meta as AppRouteMeta & { breadcrumb?: boolean }).breadcrumb === false) return false
      return true
    })

    return matched.map((record, index) => {
      const meta = record.meta as AppRouteMeta
      const isLast = index === matched.length - 1
      return {
        name: typeof record.name === 'string' ? record.name : record.path,
        title: meta.title,
        icon: meta.icon,
        path: isLast ? undefined : record.path,
        active: isLast,
      }
    })
  })

  return { items }
}
