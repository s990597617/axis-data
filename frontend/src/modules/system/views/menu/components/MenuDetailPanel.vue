<script setup lang="ts">
  import { NButton, NTag } from 'naive-ui'

  import type { MenuDto } from '@/modules/system/api/menu'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { renderStatus } from '@/shared/components/axis-crud'
  import { AxisTable } from '@/shared/components/axis-table'
  import type { AxisTableColumn } from '@/shared/components/axis-table'
  import { Icon } from '@/shared/icons'

  import { displayValue, menuIcon, menuTypeLabel } from '../utils/display'
  import { MenuType, menuTypeMeta } from '../utils/menu-type'

  defineOptions({ name: 'MenuDetailPanel' })

  defineProps<{
    loading: boolean
    selectedMenu: MenuDto | null
    selectedButtons: MenuDto[]
  }>()

  const emit = defineEmits<{
    edit: [row: MenuDto]
    'create-button': []
    delete: [row: MenuDto]
  }>()

  /* ============ 常量 ============ */

  const buttonColumns: AxisTableColumn<MenuDto>[] = [
    { key: 'title', title: '名称', minWidth: 150 },
    { key: 'permission', title: '权限标识', minWidth: 220, ellipsis: { tooltip: true } },
    {
      key: 'status',
      title: '状态',
      width: 100,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 90 }
  ]
</script>

<template>
  <main class="menu-page__content">
    <n-spin :show="loading" class="menu-page__content-spin">
      <template v-if="selectedMenu">
        <section class="menu-page__detail">
          <header class="menu-page__detail-header">
            <div class="menu-page__detail-heading">
              <span class="menu-page__detail-icon">
                <Icon :icon="menuIcon(selectedMenu)" />
              </span>
              <div>
                <div class="menu-page__detail-title-row">
                  <h2>{{ selectedMenu.title }}</h2>
                  <NTag
                    size="small"
                    :type="menuTypeMeta[selectedMenu.type as keyof typeof menuTypeMeta]?.tagType"
                  >
                    {{ menuTypeLabel(selectedMenu) }}
                  </NTag>
                </div>
                <p>菜单节点详情</p>
              </div>
            </div>

            <NButton
              v-permission="systemPermissionCodes.menu.edit"
              type="primary"
              size="small"
              @click="emit('edit', selectedMenu)"
            >
              <template #icon>
                <Icon icon="ant-design:edit-outlined" />
              </template>
              编辑
            </NButton>
          </header>

          <div class="menu-page__detail-grid">
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">路由名称</span>
              <span class="menu-page__detail-value">{{ displayValue(selectedMenu.name) }}</span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">显示名称</span>
              <span class="menu-page__detail-value">{{ selectedMenu.title }}</span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">路由路径</span>
              <span class="menu-page__detail-value is-code">
                {{ displayValue(selectedMenu.path) }}
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">组件路径</span>
              <span class="menu-page__detail-value is-code">
                {{ displayValue(selectedMenu.component) }}
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">菜单图标</span>
              <span class="menu-page__detail-value menu-page__icon-value">
                <Icon :icon="menuIcon(selectedMenu)" />
                {{ displayValue(selectedMenu.icon) }}
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">重定向地址</span>
              <span class="menu-page__detail-value is-code">
                {{ displayValue(selectedMenu.redirect) }}
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">是否显示</span>
              <span class="menu-page__detail-value">
                <NTag size="small" :type="selectedMenu.isHide ? 'default' : 'success'">
                  {{ selectedMenu.isHide ? '隐藏' : '显示' }}
                </NTag>
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">状态</span>
              <span class="menu-page__detail-value">
                <NTag size="small" :type="selectedMenu.status === 1 ? 'success' : 'error'">
                  {{ selectedMenu.status === 1 ? '启用' : '禁用' }}
                </NTag>
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">页面缓存</span>
              <span class="menu-page__detail-value">
                {{
                  selectedMenu.type === MenuType.Menu
                    ? selectedMenu.isKeepAlive
                      ? '是'
                      : '否'
                    : '—'
                }}
              </span>
            </div>
            <div class="menu-page__detail-item">
              <span class="menu-page__detail-label">排序</span>
              <span class="menu-page__detail-value">{{ selectedMenu.orderNo }}</span>
            </div>
          </div>
        </section>

        <section class="menu-page__buttons">
          <AxisTable
            :columns="buttonColumns"
            :data="selectedButtons"
            row-key="id"
            title="按钮"
            :pagination="false"
            :remote="false"
            :show-search="false"
            :show-refresh="false"
            :show-density="false"
            :show-column-setting="false"
            :show-skeleton="false"
            empty-description="该菜单下暂无按钮"
            :table-props="{ minHeight: 260 }"
            :action-column="{
              width: 130,
              maxInline: 2,
              actions: [
                {
                  key: 'edit',
                  label: '编辑',
                  type: 'primary',
                  permissions: [systemPermissionCodes.menu.edit],
                  onClick: row => emit('edit', row)
                },
                {
                  key: 'delete',
                  label: '删除',
                  type: 'error',
                  permissions: [systemPermissionCodes.menu.delete],
                  confirm: row => `确认删除按钮“${row.title}”吗？`,
                  onClick: row => emit('delete', row)
                }
              ]
            }"
          >
            <template #title>
              <div class="menu-page__button-title">
                <span>按钮</span>
                <span class="menu-page__button-count">{{ selectedButtons.length }}</span>
              </div>
            </template>
            <template #toolbar-extra>
              <NButton
                v-permission="systemPermissionCodes.menu.create"
                type="primary"
                size="small"
                @click="emit('create-button')"
              >
                <template #icon>
                  <Icon icon="ant-design:plus-outlined" />
                </template>
                新增按钮
              </NButton>
            </template>
          </AxisTable>
        </section>
      </template>

      <div v-else class="menu-page__empty-detail">
        <n-empty :description="loading ? '正在加载菜单...' : '请先选择一个菜单节点'" />
      </div>
    </n-spin>
  </main>
</template>
