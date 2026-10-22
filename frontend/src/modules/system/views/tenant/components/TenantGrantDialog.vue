<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { NTree, type TreeOption } from 'naive-ui'

  import type { MenuDto } from '@/modules/system/api/menu'
  import type { TenantDto } from '@/modules/system/api/tenant'
  import { getAssignableTenantMenus, getTenantMenuIds, grantTenantMenus } from '@/modules/system/api/tenant'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'

  defineOptions({ name: 'TenantGrantDialog' })

  const visible = ref(false)
  const loading = ref(false)
  const submitting = ref(false)
  const tenant = ref<TenantDto | null>(null)
  const menuRows = ref<MenuDto[]>([])
  const menuTree = ref<TreeOption[]>([])
  const checkedMenuIds = ref<Array<string | number>>([])

  const dialogSubtitle = computed(() => {
    if (!tenant.value) return undefined
    return `租户 @${tenant.value.code || '-'} · ${tenant.value.name || '-'} · 勾选租户可用的菜单与按钮`
  })

  function buildMenuTree(rows: MenuDto[]): TreeOption[] {
    const nodeMap = new Map<string, TreeOption & { children?: TreeOption[] }>()
    rows.forEach(row => {
      nodeMap.set(row.id, {
        key: row.id,
        label: row.type === 2 ? `[按钮] ${row.title}` : row.title,
        isLeaf: row.type === 2,
        children: []
      })
    })

    const roots: TreeOption[] = []
    rows.forEach(row => {
      const node = nodeMap.get(row.id)!
      const parent = nodeMap.get(row.parentId)
      if (parent) parent.children!.push(node)
      else roots.push(node)
    })

    nodeMap.forEach(node => {
      if (!node.children?.length) {
        delete node.children
        node.isLeaf = true
      }
    })
    return roots
  }

  async function open(row: TenantDto) {
    tenant.value = row
    visible.value = true
    loading.value = true
    checkedMenuIds.value = []
    menuTree.value = []

    const [[menus], [ids]] = await Promise.all([
      tryRequest(getAssignableTenantMenus()),
      tryRequest(getTenantMenuIds(row.id))
    ])
    menuRows.value = menus ?? []
    menuTree.value = buildMenuTree(menuRows.value)
    // 仅回显可分配菜单；历史残留的平台专属 Id 不进入勾选，避免保存时报错
    const assignableIds = new Set(menuRows.value.map(menu => String(menu.id)))
    checkedMenuIds.value = (ids ?? []).filter(id => assignableIds.has(String(id)))
    loading.value = false
  }

  async function handleSubmit() {
    if (!tenant.value) return

    const assignableIds = new Set(menuRows.value.map(menu => String(menu.id)))
    const parentMap = new Map(menuRows.value.map(menu => [String(menu.id), String(menu.parentId)]))
    const normalizedIds = new Set(
      checkedMenuIds.value.map(String).filter(id => assignableIds.has(id))
    )
    normalizedIds.forEach(menuId => {
      let parentId = parentMap.get(menuId)
      while (parentId && parentId !== '0' && assignableIds.has(parentId)) {
        normalizedIds.add(parentId)
        parentId = parentMap.get(parentId)
      }
    })

    submitting.value = true
    const [, error] = await tryRequest(grantTenantMenus(tenant.value.id, [...normalizedIds]))
    submitting.value = false
    if (error) return

    message.success('租户菜单已更新，相关用户重新登录后生效')
    visible.value = false
  }

  defineExpose({ open })
</script>

<template>
  <FormDialog
    v-model="visible"
    title="分配菜单"
    :subtitle="dialogSubtitle"
    icon="ant-design:apartment-outlined"
    :width="680"
    :loading="loading"
    :confirm-loading="submitting"
    confirm-text="保存启用"
    :mask-closable="!submitting"
    @confirm="handleSubmit"
  >
    <div class="grant-tree">
      <NTree
        v-model:checked-keys="checkedMenuIds"
        :data="menuTree"
        key-field="key"
        label-field="label"
        children-field="children"
        checkable
        cascade
        block-line
        show-line
        default-expand-all
      />
    </div>
  </FormDialog>
</template>

<style scoped lang="scss">
  .grant-tree {
    max-height: 560px;
    min-height: 280px;
    overflow: auto;
    padding: 8px;
    border: 1px solid var(--axis-border-color);
    border-radius: 6px;
  }
</style>
