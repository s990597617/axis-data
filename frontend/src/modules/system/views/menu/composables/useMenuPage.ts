import { computed, onMounted, ref, watch } from 'vue'

import type { MenuDto } from '@/modules/system/api/menu'
import { deleteMenu, getMenuList } from '@/modules/system/api/menu'
import { hasAnyPermission } from '@/shared/access'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'
import { useUserStore } from '@/store/modules/user.store'

import type MenuFormDialog from '../components/MenuFormDialog.vue'
import { MenuType } from '../utils/menu-type'
import {
  buildMenuTree,
  collectTreeKeys,
  compareMenu,
  filterMenuTree,
  flattenMenuRows
} from '../utils/tree'

export function useMenuPage() {
  /* ============ 状态 & 引用 ============ */

  const userStore = useUserStore()
  const formDialogRef = ref<InstanceType<typeof MenuFormDialog> | null>(null)
  const loading = ref(false)
  const keyword = ref('')
  const rows = ref<MenuDto[]>([])
  const selectedId = ref<string>()
  const expandedKeys = ref<Array<string | number>>([])

  /* ============ 视图模型 ============ */

  const accessSubject = computed(() => ({
    roles: userStore.roles,
    permissions: userStore.permissions
  }))
  const canCreate = computed(() =>
    hasAnyPermission(accessSubject.value, [systemPermissionCodes.menu.create])
  )
  const canDelete = computed(() =>
    hasAnyPermission(accessSubject.value, [systemPermissionCodes.menu.delete])
  )

  // 左侧树只展示目录与菜单，按钮落在右侧表格
  const navigationRows = computed(() => rows.value.filter(row => row.type !== MenuType.Button))
  const navCount = computed(() => navigationRows.value.length)
  const menuTree = computed(() => buildMenuTree(navigationRows.value))
  const filteredMenuTree = computed(() => filterMenuTree(menuTree.value, keyword.value.trim()))
  const selectedMenu = computed(
    () =>
      rows.value.find(row => row.id === selectedId.value && row.type !== MenuType.Button) ?? null
  )
  const selectedButtons = computed(() => {
    if (!selectedMenu.value) return []
    return rows.value
      .filter(row => row.type === MenuType.Button && row.parentId === selectedMenu.value?.id)
      .sort(compareMenu)
  })

  /* ============ 交互操作 ============ */

  async function loadMenus(preferredId = selectedId.value) {
    loading.value = true
    const [data, error] = await tryRequest(getMenuList())
    loading.value = false
    if (error) return

    rows.value = flattenMenuRows(data ?? [])
    const nextSelected =
      navigationRows.value.find(row => row.id === preferredId) ?? navigationRows.value[0]
    selectedId.value = nextSelected?.id
    // 默认折叠；仅搜索时再展开匹配节点
  }

  function openCreateRoot() {
    formDialogRef.value?.openCreateRoot()
  }

  function openCreateChild(parent: MenuDto) {
    formDialogRef.value?.openCreateChild(parent)
  }

  function openCreateButton() {
    if (!selectedMenu.value) return
    formDialogRef.value?.openCreateButton(selectedMenu.value)
  }

  function openEdit(row: MenuDto) {
    formDialogRef.value?.openEdit(row)
  }

  function confirmDelete(row: MenuDto) {
    const directChildCount = rows.value.filter(item => item.parentId === row.id).length
    dialog.warning({
      title: '删除确认',
      content: directChildCount
        ? `“${row.title}”下还有 ${directChildCount} 个直接子节点，确认继续删除吗？`
        : `确认删除“${row.title}”吗？`,
      positiveText: '确认删除',
      negativeText: '取消',
      onPositiveClick: () => handleDelete(row)
    })
  }

  async function handleDelete(row: MenuDto) {
    const [, error] = await tryRequest(deleteMenu(row.id))
    if (error) return

    message.success('删除成功')
    const preferredId = selectedId.value === row.id ? row.parentId : selectedId.value
    await loadMenus(preferredId)
  }

  function handleSaved(preferredId?: string) {
    void loadMenus(preferredId ?? selectedId.value)
  }

  // 搜索时展开匹配结果，便于定位
  watch(keyword, value => {
    if (value.trim()) expandedKeys.value = collectTreeKeys(filteredMenuTree.value)
  })

  onMounted(() => {
    void loadMenus()
  })

  return {
    formDialogRef,
    loading,
    keyword,
    rows,
    selectedId,
    expandedKeys,
    canCreate,
    canDelete,
    navCount,
    menuTree,
    filteredMenuTree,
    selectedMenu,
    selectedButtons,
    openCreateRoot,
    openCreateChild,
    openCreateButton,
    openEdit,
    confirmDelete,
    handleDelete,
    handleSaved
  }
}
