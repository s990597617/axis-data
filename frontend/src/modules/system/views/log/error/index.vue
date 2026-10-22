<script setup lang="ts">
  import { NButton } from 'naive-ui'

  import { clearErrorLogs, getErrorLogPage } from '@/modules/system/api/log'
  import { formatDateTime } from '@/shared/components/axis-crud'
  import { AxisTable } from '@/shared/components/axis-table'
  import { DetailDrawer } from '@/shared/components/detail-drawer'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { Icon } from '@/shared/icons'

  import {
    formatElapsed,
    textOrDash
  } from '../shared/display'
  import { useAuditLogListPage } from '../shared/useLogListPage'

  defineOptions({ name: 'SysLogError' })

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
    title: '异常日志',
    permissions: systemPermissionCodes.logError,
    pageRequest: getErrorLogPage,
    clearRequest: clearErrorLogs,
    buildDetailFields: row => [
      {
        key: 'displayTitle',
        label: '功能描述',
        value: textOrDash(row.displayTitle as string),
        icon: 'ant-design:profile-outlined'
      },
      {
        key: 'httpMethod',
        label: '请求方式',
        value: textOrDash(row.httpMethod as string),
        type: 'tag',
        tagType: 'error',
        icon: 'ant-design:api-outlined'
      },
      {
        key: 'requestUrl',
        label: '请求地址',
        value: textOrDash(row.requestUrl as string),
        fullRow: true,
        type: 'code',
        icon: 'ant-design:link-outlined'
      },
      {
        key: 'remoteIp',
        label: 'IP 地址',
        value: textOrDash(row.remoteIp as string),
        icon: 'ant-design:global-outlined'
      },
      {
        key: 'elapsed',
        label: '耗时',
        value: formatElapsed(row.elapsed as number),
        icon: 'ant-design:dashboard-outlined'
      },
      {
        key: 'traceId',
        label: '跟踪 ID',
        value: textOrDash(row.traceId as string),
        type: 'code',
        icon: 'ant-design:number-outlined'
      },
      {
        key: 'threadId',
        label: '线程 ID',
        value: String(row.threadId ?? '-'),
        icon: 'ant-design:cluster-outlined'
      },
      {
        key: 'createdTime',
        label: '发生时间',
        value: formatDateTime(row.createdTime),
        icon: 'ant-design:clock-circle-outlined'
      },
      {
        key: 'fullName',
        label: '方法全名',
        value: textOrDash(row.fullName as string),
        fullRow: true,
        type: 'code',
        icon: 'ant-design:file-text-outlined'
      },
      {
        key: 'userAgent',
        label: 'User-Agent',
        value: textOrDash(row.userAgent as string),
        fullRow: true,
        icon: 'ant-design:chrome-outlined'
      },
      {
        key: 'requestParam',
        label: '请求参数',
        value: textOrDash(row.requestParam as string),
        fullRow: true,
        type: 'pre',
        icon: 'ant-design:form-outlined'
      },
      {
        key: 'exception',
        label: '异常信息',
        value: textOrDash(row.exception as string),
        fullRow: true,
        type: 'pre',
        icon: 'ant-design:bug-outlined'
      }
    ]
  })

  // 异常页无账号列：用功能描述作为主列并承接关键字搜索
  columns.splice(0, 2, {
    key: 'displayTitle',
    title: '功能描述',
    search: { field: 'keyword', placeholder: '功能 / 地址 / 异常 / IP', primary: true },
    ellipsis: { tooltip: true },
    render: row => textOrDash(row.displayTitle as string)
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
      title="异常日志详情"
      :subtitle="detailSubtitle"
      icon="ant-design:bug-outlined"
      icon-theme="error"
      :fields="detailFields"
      section-title="异常信息"
      :width="820"
    />
  </div>
</template>

<style scoped lang="scss">
  @use '../shared/styles.scss';
</style>
