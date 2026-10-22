import { computed, nextTick, onMounted, onUnmounted, ref, watch, type Ref } from 'vue'
import { useRouter } from 'vue-router'
import { pinyin } from 'pinyin-pro'

import { buildRouteTree, collectLeafNodes, type RouteNode } from '@/router/helpers/route-tree'
import { staticRoutes } from '@/router/routes/static-routes'
import type { AppRouteRecord } from '@/shared/types/app-route'
import { usePermissionStore } from '@/store/modules/permission.store'

export interface LayoutSearchItem {
  title: string
  path: string
  icon?: string
  breadcrumb: string
  pinyinFull: string
  pinyinFirst: string
}

/**
 * 将路由 IR 叶子节点转为搜索索引项
 *
 * @description
 * 拼音检索所需的两类字符串在构建期一次性计算好，搜索时只做字符串 includes，
 * 避免在每次 keyword 变化时重复跑 pinyin-pro 的 segmentation。
 */
function toSearchItems(tree: readonly RouteNode[]): LayoutSearchItem[] {
  return collectLeafNodes(tree).map(node => ({
    title: node.title,
    path: node.path,
    icon: node.icon,
    breadcrumb: [...node.parents, node.title].join(' > '),
    pinyinFull: pinyin(node.title, { toneType: 'none', type: 'array' }).join('').toLowerCase(),
    pinyinFirst: pinyin(node.title, { pattern: 'first', toneType: 'none', type: 'array' })
      .join('')
      .toLowerCase()
  }))
}

export function useLayoutSearch(visible: Ref<boolean>) {
  const router = useRouter()
  const permissionStore = usePermissionStore()
  const keyword = ref('')
  const activeIndex = ref(0)
  const inputRef = ref<HTMLInputElement>()
  const listRef = ref<HTMLDivElement>()

  const appRoutes = computed<AppRouteRecord[]>(() => [
    ...(staticRoutes as AppRouteRecord[]),
    ...permissionStore.accessRoutes
  ])

  const allItems = computed(() => toSearchItems(buildRouteTree(appRoutes.value)))
  const filteredItems = computed(() => {
    const query = keyword.value.trim().toLowerCase()
    if (!query) return []
    return allItems.value.filter(
      item =>
        item.title.toLowerCase().includes(query) ||
        item.pinyinFull.includes(query) ||
        item.pinyinFirst.includes(query) ||
        item.path.toLowerCase().includes(query)
    )
  })

  watch(keyword, () => {
    activeIndex.value = 0
  })

  watch(visible, value => {
    if (!value) return
    keyword.value = ''
    activeIndex.value = 0
    nextTick(() => inputRef.value?.focus())
  })

  function clearKeyword() {
    keyword.value = ''
    inputRef.value?.focus()
  }

  function navigate(item: LayoutSearchItem, newWindow = false) {
    if (newWindow) {
      const url = router.resolve(item.path).href
      window.open(url, '_blank')
    } else {
      router.push(item.path)
    }
    visible.value = false
  }

  function scrollToActive() {
    nextTick(() => {
      const active = listRef.value?.querySelector('.search-item.active') as HTMLElement | null
      active?.scrollIntoView({ block: 'nearest' })
    })
  }

  function handleKeydown(event: KeyboardEvent) {
    const total = filteredItems.value.length
    if (!total) return

    if (event.key === 'ArrowDown') {
      event.preventDefault()
      activeIndex.value = (activeIndex.value + 1) % total
      scrollToActive()
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      activeIndex.value = (activeIndex.value - 1 + total) % total
      scrollToActive()
    } else if (event.key === 'Enter') {
      event.preventDefault()
      const item = filteredItems.value[activeIndex.value]
      if (item) navigate(item, event.ctrlKey || event.metaKey)
    }
  }

  function handleGlobalKeydown(event: KeyboardEvent) {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
      event.preventDefault()
      visible.value = !visible.value
    }
    if (event.key === 'Escape' && visible.value) {
      visible.value = false
    }
  }

  onMounted(() => document.addEventListener('keydown', handleGlobalKeydown))
  onUnmounted(() => document.removeEventListener('keydown', handleGlobalKeydown))

  return {
    activeIndex,
    clearKeyword,
    filteredItems,
    handleKeydown,
    inputRef,
    keyword,
    listRef,
    navigate
  }
}
