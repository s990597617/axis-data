<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { NTree, type TreeOption } from 'naive-ui'

  import { getGrantableRoleMenus, getRoleMenuIds, grantRoleMenus } from '@/modules/system/api/role'
  import type { MenuDto } from '@/modules/system/api/menu'
  import type { RoleDto } from '@/modules/system/api/role'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'

  defineOptions({ name: 'RoleGrantDialog' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const loading = ref(false)
  const submitting = ref(false)
  const role = ref<RoleDto | null>(null)
  const menuRows = ref<MenuDto[]>([])
  const menuTree = ref<TreeOption[]>([])
  const checkedMenuIds = ref<Array<string | number>>([])

  const dialogSubtitle = computed(() => {
    if (!role.value) return undefined
    return `角色 @${role.value.code || '-'} · ${role.value.name || '-'} · 勾选可访问的菜单与按钮`
  })

  /* ============ 工具函数 ============ */

  // 把菜单平铺数据转换为授权树；叶子节点不挂空 children，避免出现展开箭头
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

  /* ============ 交互操作 ============ */

  async function open(row: RoleDto) {
    role.value = row
    visible.value = true
    loading.value = true
    checkedMenuIds.value = []
    menuTree.value = []

    const [[menus], [ids]] = await Promise.all([
      tryRequest(getGrantableRoleMenus()),
      tryRequest(getRoleMenuIds(row.id))
    ])
    menuRows.value = menus ?? []
    menuTree.value = buildMenuTree(menuRows.value)
    checkedMenuIds.value = ids ?? []
    loading.value = false
  }

  async function handleSubmit() {
    if (!role.value) return

    const parentMap = new Map(menuRows.value.map(menu => [String(menu.id), String(menu.parentId)]))
    const normalizedIds = new Set(checkedMenuIds.value.map(String))
    normalizedIds.forEach(menuId => {
      let parentId = parentMap.get(menuId)
      while (parentId && parentId !== '0') {
        normalizedIds.add(parentId)
        parentId = parentMap.get(parentId)
      }
    })

    submitting.value = true
    const [, error] = await tryRequest(grantRoleMenus(role.value.id, [...normalizedIds]))
    submitting.value = false
    if (error) return

    message.success('角色权限已更新，相关用户重新登录后生效')
    visible.value = false
  }

  defineExpose({ open })
</script>

<template>
  <FormDialog
    v-model="visible"
    title="分配权限"
    :subtitle="dialogSubtitle"
    icon="ant-design:safety-certificate-outlined"
    :width="680"
    :loading="loading"
    :confirm-loading="submitting"
    confirm-text="保存授权"
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
