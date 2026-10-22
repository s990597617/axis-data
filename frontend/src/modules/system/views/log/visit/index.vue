<script setup lang="ts">
  import { NButton } from 'naive-ui'

  import { clearVisitLogs, getVisitLogPage } from '@/modules/system/api/log'
  import { AxisTable } from '@/shared/components/axis-table'
  import { DetailDrawer } from '@/shared/components/detail-drawer'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { Icon } from '@/shared/icons'

  import { buildAuditDetailFields } from '../shared/display'
  import { useAuditLogListPage } from '../shared/useLogListPage'

  defineOptions({ name: 'SysLogVisit' })

  const {
    columns,
    detailFields,
    detailSubtitle,
    detailVisible,
    handleClear,
    permissions,
    request,
    showDetail,
    tableRef
  } = useAuditLogListPage({
    title: '访问日志',
    permissions: systemPermissionCodes.logVisit,
    pageRequest: getVisitLogPage,
    clearRequest: clearVisitLogs,
    buildDetailFields: row => buildAuditDetailFields(row as never)
  })
</script>

<template>
  <div class="log-page">
    <AxisTable
      ref="tableRef"
      :columns="columns"
      :request="request"
      :search-config="{ minItemWidth: 220, maxColumns: 3 }"
      row-key="id"
      :pagination="{ pageSize: 20 }"
      :action-column="{
        width: 88,
        maxInline: 1,
        actions: [
          {
            key: 'detail',
            label: '详情',
            icon: 'ant-design:eye-outlined',
            type: 'primary',
            permissions: [permissions.query],
            onClick: row => showDetail(row)
          }
        ]
      }"
    >
      <template #toolbar>
        <NButton
          v-permission="permissions.clear"
          type="error"
          size="small"
          secondary
          @click="handleClear"
        >
          <template #icon>
            <Icon icon="ant-design:delete-outlined" />
          </template>
          清空日志
        </NButton>
      </template>
    </AxisTable>

    <DetailDrawer
      v-model:show="detailVisible"
      title="访问日志详情"
      :subtitle="detailSubtitle"
      icon="ant-design:eye-outlined"
      :fields="detailFields"
      section-title="访问信息"
      :width="760"
    />
  </div>
</template>

<style scoped lang="scss">
  @use '../shared/styles.scss';
</style>
