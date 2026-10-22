import { computed, onMounted, ref } from 'vue'

import {
  flushCache,
  getCacheOverview,
  listCacheKeys,
  removeCacheByPrefix,
  removeCacheKey,
  type CacheKeyItemDto,
  type CacheOverviewDto,
  type CachePrefixGroupDto
} from '@/modules/system/api/cache'
import type { AxisTableAction, AxisTableColumn } from '@/shared/components/axis-table'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

import CacheDetailDrawer from '../components/CacheDetailDrawer.vue'
import { formatTtl } from '../utils/display'

export function useCachePage() {
  const permissions = systemPermissionCodes.cache
  const tableRef = ref<AxisTableAction | null>(null)
  const detailDrawerRef = ref<InstanceType<typeof CacheDetailDrawer> | null>(null)
  const overview = ref<CacheOverviewDto | null>(null)
  const loadingOverview = ref(false)
  const selectedCode = ref('all')
  const prefix = ref('*')
  const truncated = ref(false)
  const matchedTotal = ref(0)

  const groups = computed(() => overview.value?.groups ?? [])

  const selectedGroup = computed<CachePrefixGroupDto | null>(
    () => groups.value.find(item => item.code === selectedCode.value) ?? groups.value[0] ?? null
  )

  const canClearSelected = computed(() => !!selectedGroup.value?.allowClear)

  const prefixHint = computed(() => {
    const group = selectedGroup.value
    if (!group) return '选择上方缓存分组后查看键列表'
    if (group.prefix === '*') return '当前：全部已知分组汇总'
    return `当前前缀：${group.prefix}`
  })

  const columns: AxisTableColumn[] = [
    {
      key: 'key',
      title: '缓存键',
      ellipsis: { tooltip: true },
      search: {
        field: 'keyword',
        label: '过滤键名',
        placeholder: '在当前分组内过滤',
        primary: true
      }
    },
    {
      key: 'expirationSeconds',
      title: '剩余 TTL',
      width: 140,
      render: row => formatTtl(row.expirationSeconds as number)
    }
  ]

  async function loadOverview(keepSelection = true) {
    loadingOverview.value = true
    const [result] = await tryRequest(getCacheOverview())
    loadingOverview.value = false
    if (!result) return
    overview.value = result

    const stillValid =
      keepSelection && result.groups.some(item => item.code === selectedCode.value)
    if (!stillValid) {
      selectedCode.value = result.groups[0]?.code || 'all'
      prefix.value = result.groups[0]?.prefix || '*'
    } else {
      const current = result.groups.find(item => item.code === selectedCode.value)
      if (current) prefix.value = current.prefix
    }

    await tableRef.value?.reload({ keyword: '' })
  }

  async function selectGroup(group: CachePrefixGroupDto) {
    selectedCode.value = group.code
    prefix.value = group.prefix
    await tableRef.value?.reload({ keyword: '' })
  }

  async function request(params: Record<string, unknown>) {
    const currentPrefix = (selectedGroup.value?.prefix || prefix.value || '*').trim()
    prefix.value = currentPrefix
    const take = overview.value?.maxKeysPerQuery ?? 200
    const [result] = await tryRequest(listCacheKeys({ prefix: currentPrefix, take }))
    if (!result) {
      truncated.value = false
      matchedTotal.value = 0
      return { items: [], total: 0 }
    }

    truncated.value = result.truncated
    matchedTotal.value = result.total

    const keyword = String(params.keyword ?? '')
      .trim()
      .toLowerCase()
    const items = keyword
      ? result.items.filter(item => item.key.toLowerCase().includes(keyword))
      : result.items

    return {
      items,
      total: items.length
    }
  }

  function showDetail(row: CacheKeyItemDto) {
    detailDrawerRef.value?.open(row.key)
  }

  function handleRemoveKey(row: CacheKeyItemDto) {
    dialog.warning({
      title: '删除缓存键',
      content: `确定删除「${row.key}」？`,
      positiveText: '删除',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(removeCacheKey({ key: row.key }))
        if (error) return
        message.success('已删除')
        await loadOverview()
      }
    })
  }

  function handleClearPrefix() {
    const group = selectedGroup.value
    if (!group?.allowClear) {
      message.warning('请先选择具体缓存分组；全部请使用「清空全部」')
      return
    }

    dialog.warning({
      title: `清理「${group.name}」`,
      content: `确定清理前缀「${group.prefix}」下的全部缓存？此操作不可恢复。`,
      positiveText: '确认清理',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [result, error] = await tryRequest(removeCacheByPrefix({ prefix: group.prefix }))
        if (error || !result) return
        message.success(result.message)
        await loadOverview()
      }
    })
  }

  function handleFlushAll() {
    if (!overview.value?.allowFlushAll) {
      message.warning('当前环境不允许清空全部缓存')
      return
    }

    dialog.error({
      title: '清空全部缓存',
      content: '将清空当前缓存提供者中的全部键，仅开发/测试环境可用。确定继续？',
      positiveText: '确认清空',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [result, error] = await tryRequest(flushCache())
        if (error || !result) return
        message.success(result.message)
        await loadOverview()
      }
    })
  }

  onMounted(() => {
    void loadOverview(false)
  })

  return {
    canClearSelected,
    columns,
    detailDrawerRef,
    groups,
    handleClearPrefix,
    handleFlushAll,
    handleRemoveKey,
    loadOverview,
    loadingOverview,
    matchedTotal,
    overview,
    permissions,
    prefix,
    prefixHint,
    request,
    selectGroup,
    selectedCode,
    selectedGroup,
    showDetail,
    tableRef,
    truncated
  }
}
