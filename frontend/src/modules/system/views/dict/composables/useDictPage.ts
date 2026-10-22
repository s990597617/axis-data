import { onMounted, ref, watch } from 'vue'

import type { DictTypeDto } from '@/modules/system/api/dict'
import { deleteDictType, getDictTypePage } from '@/modules/system/api/dict'
import { isSuperAdmin } from '@/shared/access/access.service'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { clearDictCache } from '@/shared/composables/use-dict'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'
import { useUserStore } from '@/store/modules/user.store'

import type DictDataPanel from '../components/DictDataPanel.vue'
import type DictTypeFormDialog from '../components/DictTypeFormDialog.vue'

function normalizeTypePage(data: unknown): { items: DictTypeDto[]; total: number } {
  if (Array.isArray(data)) {
    return { items: data as DictTypeDto[], total: data.length }
  }
  if (!data || typeof data !== 'object') return { items: [], total: 0 }
  const source = data as Record<string, unknown>
  const rawItems = source.items ?? source.Items ?? source.list ?? source.List
  const items = Array.isArray(rawItems) ? (rawItems as DictTypeDto[]) : []
  const total = Number(source.total ?? source.Total ?? source.itemCount ?? items.length) || 0
  return { items, total }
}

export function useDictPage() {
  /* ============ 状态 & 引用 ============ */

  const userStore = useUserStore()
  const typeLoading = ref(false)
  const types = ref<DictTypeDto[]>([])
  const keyword = ref('')
  const page = ref(1)
  const pageSize = ref(12)
  const total = ref(0)
  const selectedId = ref<string>()
  // 跨页保留当前选中类型，避免翻页后右侧空白
  const selectedType = ref<DictTypeDto | null>(null)
  const typeFormRef = ref<InstanceType<typeof DictTypeFormDialog> | null>(null)
  const dataPanelRef = ref<InstanceType<typeof DictDataPanel> | null>(null)
  let keywordTimer: ReturnType<typeof setTimeout> | undefined

  /* ============ 交互操作 ============ */

  async function loadTypes(options?: {
    preferId?: string
    preferCode?: string
    keepPage?: boolean
  }) {
    typeLoading.value = true
    const [data, error] = await tryRequest(
      getDictTypePage({
        page: page.value,
        pageSize: pageSize.value,
        keyword: keyword.value.trim() || undefined
      })
    )
    typeLoading.value = false
    if (error) return

    const result = normalizeTypePage(data)
    types.value = result.items
    total.value = result.total

    // 当前页删空时回退上一页
    if (!options?.keepPage && result.items.length === 0 && page.value > 1 && result.total > 0) {
      page.value -= 1
      await loadTypes(options)
      return
    }

    const byPreferId = options?.preferId
      ? result.items.find(item => item.id === options.preferId)
      : undefined
    const byPreferCode = options?.preferCode
      ? result.items.find(item => item.code === options.preferCode)
      : undefined
    const byCurrent = selectedId.value
      ? result.items.find(item => item.id === selectedId.value)
      : undefined

    const next = byPreferId ?? byPreferCode ?? byCurrent ?? result.items[0] ?? null
    if (next) {
      selectedId.value = next.id
      selectedType.value = next
    } else if (
      selectedId.value &&
      selectedType.value?.id === selectedId.value &&
      !options?.preferId &&
      !options?.preferCode
    ) {
      // 选中项不在本页：保留右侧已选，不清空
    } else if (!result.items.length) {
      selectedId.value = undefined
      selectedType.value = null
    }
  }

  function selectType(row: DictTypeDto) {
    selectedId.value = row.id
    selectedType.value = row
  }

  function handleKeywordUpdate(value: string) {
    keyword.value = value
    if (keywordTimer) clearTimeout(keywordTimer)
    keywordTimer = setTimeout(() => {
      page.value = 1
      void loadTypes()
    }, 280)
  }

  function handlePageChange(nextPage: number) {
    page.value = nextPage
    void loadTypes({ keepPage: true })
  }

  function handlePageSizeChange(nextSize: number) {
    pageSize.value = nextSize
    page.value = 1
    void loadTypes()
  }

  function canMutateBuiltin(row: DictTypeDto) {
    if (!row.isBuiltin) return true
    return isSuperAdmin({
      roles: userStore.roles,
      permissions: userStore.permissions,
      accountType: userStore.userInfo?.accountType
    })
  }

  function openCreateType() {
    typeFormRef.value?.openCreate()
  }

  function openEditType(row?: DictTypeDto | null) {
    const target = row ?? selectedType.value
    if (!target) return
    if (!canMutateBuiltin(target)) {
      message.warning('内置字典仅超级管理员可修改')
      return
    }
    typeFormRef.value?.openEdit(target)
  }

  function confirmDeleteType(row: DictTypeDto) {
    if (row.isBuiltin) {
      message.warning('内置字典不可删除')
      return
    }
    dialog.warning({
      title: '删除确认',
      content: `确认删除字典「${row.name}」？其下数据项将一并删除。`,
      positiveText: '删除',
      negativeText: '取消',
      onPositiveClick: () => handleDeleteType(row)
    })
  }

  async function handleDeleteType(row: DictTypeDto) {
    const [, error] = await tryRequest(deleteDictType(row.id))
    if (error) return
    message.success('删除成功')
    clearDictCache(row.code)
    if (selectedId.value === row.id) {
      selectedId.value = undefined
      selectedType.value = null
    }
    await loadTypes()
  }

  async function handleTypeSaved(payload?: { id?: string; code?: string }) {
    if (payload?.code) clearDictCache(payload.code)
    await loadTypes({ preferId: payload?.id, preferCode: payload?.code })
  }

  watch(selectedId, () => {
    dataPanelRef.value?.reload()
  })

  onMounted(() => {
    void loadTypes()
  })

  return {
    systemPermissionCodes,
    typeLoading,
    types,
    keyword,
    page,
    pageSize,
    total,
    selectedId,
    selectedType,
    typeFormRef,
    dataPanelRef,
    selectType,
    handleKeywordUpdate,
    handlePageChange,
    handlePageSizeChange,
    openCreateType,
    openEditType,
    confirmDeleteType,
    handleTypeSaved,
    loadTypes
  }
}
