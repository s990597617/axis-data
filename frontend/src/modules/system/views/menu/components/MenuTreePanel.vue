<script setup lang="ts">
  import { h } from 'vue'
  import { NButton, type TreeOption } from 'naive-ui'

  import type { MenuDto } from '@/modules/system/api/menu'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { Icon } from '@/shared/icons'

  import { MenuType } from '../utils/menu-type'
  import { menuIcon } from '../utils/display'
  import type { MenuTreeOption } from '../utils/tree'

  defineOptions({ name: 'MenuTreePanel' })

  const props = defineProps<{
    loading: boolean
    navCount: number
    treeData: MenuTreeOption[]
    selectedId?: string
    expandedKeys: Array<string | number>
    keyword: string
    canCreate: boolean
    canDelete: boolean
  }>()

  const emit = defineEmits<{
    'update:keyword': [value: string]
    'update:selectedId': [value: string]
    'update:expandedKeys': [value: Array<string | number>]
    'create-root': []
    'create-child': [parent: MenuDto]
    delete: [row: MenuDto]
  }>()

  /* ============ 工具函数 ============ */

  // 树节点自定义标签：图标 + 标题 + 悬停操作
  function renderTreeLabel({ option, selected }: { option: TreeOption; selected: boolean }) {
    const node = option as MenuTreeOption
    const actionNodes = []

    if (props.canCreate) {
      actionNodes.push(
        h(
          'button',
          {
            type: 'button',
            class: 'menu-tree-node__action is-create',
            title: `在“${node.menu.title}”下新增`,
            'aria-label': `在“${node.menu.title}”下新增`,
            onClick: (event: MouseEvent) => {
              event.stopPropagation()
              emit('create-child', node.menu)
            }
          },
          [h(Icon, { icon: 'ant-design:plus-outlined' })]
        )
      )
    }

    if (props.canDelete) {
      actionNodes.push(
        h(
          'button',
          {
            type: 'button',
            class: 'menu-tree-node__action is-delete',
            title: `删除“${node.menu.title}”`,
            'aria-label': `删除“${node.menu.title}”`,
            onClick: (event: MouseEvent) => {
              event.stopPropagation()
              emit('delete', node.menu)
            }
          },
          [h(Icon, { icon: 'ant-design:delete-outlined' })]
        )
      )
    }

    return h('div', { class: ['menu-tree-node', { 'is-selected': selected }] }, [
      h('span', { class: 'menu-tree-node__main' }, [
        h(
          'span',
          {
            class: [
              'menu-tree-node__icon-wrap',
              node.menu.type === MenuType.Directory ? 'is-directory' : 'is-menu'
            ]
          },
          [h(Icon, { icon: menuIcon(node.menu), class: 'menu-tree-node__icon' })]
        ),
        h('span', { class: 'menu-tree-node__title', title: node.menu.title }, node.menu.title),
        node.menu.isHide
          ? h(Icon, {
              icon: 'ant-design:eye-invisible-outlined',
              class: 'menu-tree-node__state-icon',
              title: '已隐藏'
            })
          : null,
        node.menu.status !== 1
          ? h('span', { class: 'menu-tree-node__disabled-dot', title: '已停用' })
          : null
      ]),
      actionNodes.length ? h('span', { class: 'menu-tree-node__actions' }, actionNodes) : null
    ])
  }

  function handleSelect(keys: Array<string | number>) {
    const key = keys[0]
    if (key !== undefined) emit('update:selectedId', String(key))
  }
</script>

<template>
  <aside class="menu-page__tree-panel">
    <div class="menu-page__tree-header">
      <div class="menu-page__tree-title">
        <Icon icon="ant-design:appstore-outlined" />
        <span>菜单结构</span>
      </div>
      <span class="menu-page__tree-count">{{ navCount }}</span>
    </div>

    <div class="menu-page__tree-tools">
      <n-input
        :value="keyword"
        clearable
        size="small"
        placeholder="搜索菜单"
        @update:value="emit('update:keyword', $event)"
      >
        <template #prefix>
          <Icon icon="ant-design:search-outlined" />
        </template>
      </n-input>
      <NButton
        v-permission="systemPermissionCodes.menu.create"
        type="primary"
        size="small"
        circle
        title="新增顶级菜单"
        aria-label="新增顶级菜单"
        @click="emit('create-root')"
      >
        <template #icon>
          <Icon icon="ant-design:plus-outlined" />
        </template>
      </NButton>
    </div>

    <n-spin :show="loading" class="menu-page__tree-spin">
      <n-scrollbar class="menu-page__tree-scroll">
        <n-tree
          v-if="treeData.length"
          class="menu-page__tree"
          :data="treeData"
          :selected-keys="selectedId ? [selectedId] : []"
          :expanded-keys="expandedKeys"
          :render-label="renderTreeLabel"
          key-field="key"
          label-field="label"
          children-field="children"
          block-line
          selectable
          :expand-on-click="false"
          :show-line="false"
          :cancelable="false"
          @update:selected-keys="handleSelect"
          @update:expanded-keys="emit('update:expandedKeys', $event)"
        />
        <n-empty
          v-else-if="!loading"
          size="small"
          :description="keyword ? '没有匹配的菜单' : '暂无菜单数据'"
        />
      </n-scrollbar>
    </n-spin>
  </aside>
</template>
