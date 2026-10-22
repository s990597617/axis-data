<script setup lang="ts">
  import { NButton } from 'naive-ui'

  import {
    clearLoginLogs,
    getLoginLogPage
  } from '@/modules/system/api/log'
  import { AxisTable } from '@/shared/components/axis-table'
  import { DetailDrawer } from '@/shared/components/detail-drawer'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { Icon } from '@/shared/icons'

  import { useLoginLogListPage } from '../shared/useLogListPage'

  defineOptions({ name: 'SysLogLogin' })

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
  } = useLoginLogListPage({
    permissions: systemPermissionCodes.logLogin,
    pageRequest: getLoginLogPage,
    clearRequest: clearLoginLogs
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
      title="登录日志详情"
      :subtitle="detailSubtitle"
      icon="ant-design:login-outlined"
      :fields="detailFields"
      section-title="登录信息"
      :width="680"
    />
  </div>
</template>

<style scoped lang="scss">
  @use '../shared/styles.scss';
</style>
