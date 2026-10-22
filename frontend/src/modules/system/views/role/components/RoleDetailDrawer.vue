<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { NEmpty, NTree } from 'naive-ui'

  import { getRoleDetail, type RoleDetailDto } from '@/modules/system/api/role'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'
  import { tryRequest } from '@/shared/http'
  import { formatLocalDateTime } from '@/shared/utils/date'

  import {
    countGrantedMenus,
    formatDataScope,
    toGrantedMenuTreeOptions
  } from '../utils/display'

  defineOptions({ name: 'RoleDetailDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const loading = ref(false)
  const detail = ref<RoleDetailDto | null>(null)

  /* ============ 视图模型 ============ */

  const detailFields = computed<DetailField[]>(() => {
    const row = detail.value
    if (!row) return []
    return [
      {
        key: 'code',
        label: '角色编码',
        value: row.code,
        type: 'code',
        icon: 'ant-design:code-outlined'
      },
      {
        key: 'name',
        label: '角色名称',
        value: row.name,
        icon: 'ant-design:idcard-outlined'
      },
      {
        key: 'status',
        label: '状态',
        value: row.status === 1 ? '启用' : '禁用',
        type: 'tag',
        tagType: row.status === 1 ? 'success' : 'error',
        icon: 'ant-design:check-circle-outlined'
      },
      {
        key: 'dataScope',
        label: '数据范围',
        value: formatDataScope(row.dataScope),
        icon: 'ant-design:database-outlined'
      },
      ...(row.dataScope === 2
        ? [
            {
              key: 'orgNames',
              label: '自定义机构',
              value: row.orgNames?.length ? row.orgNames.join('、') : '-',
              fullRow: true,
              icon: 'ant-design:apartment-outlined'
            } as DetailField
          ]
        : []),
      {
        key: 'orderNo',
        label: '排序',
        value: row.orderNo,
        icon: 'ant-design:ordered-list-outlined'
      },
      {
        key: 'createdTime',
        label: '创建时间',
        value: row.createdTime ? formatLocalDateTime(row.createdTime) : '-',
        icon: 'ant-design:clock-circle-outlined'
      },
      {
        key: 'remark',
        label: '备注',
        value: row.remark || '-',
        fullRow: true,
        icon: 'ant-design:file-text-outlined'
      }
    ]
  })

  const statusText = computed(() =>
    detail.value ? (detail.value.status === 1 ? '启用' : '禁用') : undefined
  )

  const statusType = computed(() => {
    if (!detail.value) return undefined
    return detail.value.status === 1 ? ('success' as const) : ('error' as const)
  })

  const menuTree = computed(() => toGrantedMenuTreeOptions(detail.value?.menus ?? []))

  const menuCount = computed(() => countGrantedMenus(detail.value?.menus ?? []))

  const permissionSectionTitle = computed(() => {
    if (detail.value?.ownAllPermissions) return '已授权权限'
    return `已授权权限（${menuCount.value}）`
  })

  /* ============ 交互操作 ============ */

  async function open(roleId: string) {
    visible.value = true
    loading.value = true
    detail.value = null

    const [data, error] = await tryRequest(getRoleDetail(roleId))
    loading.value = false
    if (error) {
      visible.value = false
      return
    }
    detail.value = data ?? null
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="角色详情"
    :subtitle="detail ? detail.code : undefined"
    icon="ant-design:team-outlined"
    :status-text="statusText"
    :status-type="statusType"
    :loading="loading"
    :width="760"
    :fields="detailFields"
    section-title="基本信息"
  >
    <section v-if="detail" class="role-detail-perm">
      <div class="role-detail-perm__header">
        <span class="role-detail-perm__title">{{ permissionSectionTitle }}</span>
      </div>

      <div v-if="detail.ownAllPermissions" class="role-detail-perm__tip">
        超级管理员固定拥有全部菜单与按钮权限，无需单独分配。
      </div>

      <NEmpty
        v-else-if="!menuTree.length"
        description="尚未分配任何菜单权限"
        class="role-detail-perm__empty"
      />

      <div v-else class="role-detail-perm__tree">
        <NTree
          :data="menuTree"
          key-field="key"
          label-field="label"
          children-field="children"
          block-line
          show-line
          default-expand-all
          expand-on-click
          selectable
        />
      </div>
    </section>
  </DetailDrawer>
</template>

<style scoped lang="scss">
  .role-detail-perm {
    padding: 14px 16px 12px;
    border: 1px solid color-mix(in srgb, var(--axis-border-color) 80%, transparent);
    border-radius: var(--axis-border-radius, 6px);
    background: var(--axis-card-color);

    &__header {
      margin-bottom: 8px;
      padding-bottom: 10px;
      border-bottom: 1px dashed color-mix(in srgb, var(--axis-border-color) 85%, transparent);
    }

    &__title {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      color: var(--axis-text-color-1);
      font-size: 14px;
      font-weight: 600;

      &::before {
        width: 3px;
        height: 14px;
        border-radius: 999px;
        background: linear-gradient(
          180deg,
          var(--axis-primary-color),
          color-mix(in srgb, var(--axis-primary-color) 45%, transparent)
        );
        content: '';
      }
    }

    &__tip {
      padding: 14px 16px;
      border: 1px solid color-mix(in srgb, var(--axis-primary-color) 18%, transparent);
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-primary-color) 8%, var(--axis-card-color));
      color: var(--axis-text-color-2);
      font-size: 13px;
      line-height: 21px;
    }

    &__empty {
      padding: 28px 0;
    }

    &__tree {
      max-height: min(52vh, 480px);
      overflow: auto;
      padding: 8px 10px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 72%, transparent);
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-bg-color) 45%, var(--axis-card-color));
    }
  }
</style>
