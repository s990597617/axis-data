import { ref } from 'vue'

import {
  clearLoginLogs,
  getLoginLogPage,
  type LoginLogDto
} from '@/modules/system/api/log'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import type {
  AxisTableAction,
  AxisTableActionColumn,
  AxisTableColumn,
  AxisTableQuery
} from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

import type LoginLogDetailDrawer from '../components/LoginLogDetailDrawer.vue'
import {
  formatLogTime,
  loginSuccessOptions,
  renderLoginSuccess,
  withTimeRange
} from '../../shared/display'

export function useLoginLogPage() {
  /* ============ 状态 & 引用 ============ */

  const tableRef = ref<AxisTableAction<LoginLogDto> | null>(null)
  const detailRef = ref<InstanceType<typeof LoginLogDetailDrawer> | null>(null)
  const clearing = ref(false)

  /* ============ 表格配置 ============ */

  const columns: AxisTableColumn<LoginLogDto>[] = [
    {
      key: 'account',
      title: '账号',
      width: 140,
      search: { field: 'keyword', placeholder: '账号或 IP', primary: true }
    },
    {
      key: 'success',
      title: '结果',
      width: 90,
      search: true,
      dict: loginSuccessOptions,
      render: row => renderLoginSuccess(row.success)
    },
    { key: 'remoteIp', title: 'IP 地址', width: 140 },
    {
      key: 'failReason',
      title: '失败原因',
      ellipsis: { tooltip: true },
      render: row => row.failReason || '-'
    },
    {
      key: 'userAgent',
      title: 'User-Agent',
      ellipsis: { tooltip: true },
      render: row => row.userAgent || '-'
    },
    {
      key: 'createdTime',
      title: '登录时间',
      width: 180,
      search: {
        field: 'createdTimeRange',
        component: 'dateRange',
        advanced: true,
        span: 2
      },
      render: row => formatLogTime(row.createdTime)
    }
  ]

  const actionColumn: AxisTableActionColumn<LoginLogDto> = {
    width: 90,
    actions: [
      {
        key: 'detail',
        label: '详情',
        type: 'primary',
        onClick: row => detailRef.value?.open(row)
      }
    ]
  }

  /* ============ 交互操作 ============ */

  function request(params: AxisTableQuery) {
    return getLoginLogPage(withTimeRange(params))
  }

  function handleClear() {
    dialog.warning({
      title: '清空登录日志',
      content: '将删除全部登录日志，且不可恢复，确认继续？',
      positiveText: '清空',
      negativeText: '取消',
      onPositiveClick: async () => {
        clearing.value = true
        const [, error] = await tryRequest(clearLoginLogs(), {
          fallbackMessage: '清空登录日志失败'
        })
        clearing.value = false
        if (error) return
        message.success('登录日志已清空')
        await tableRef.value?.reload()
      }
    })
  }

  return {
    tableRef,
    detailRef,
    clearing,
    columns,
    actionColumn,
    request,
    handleClear,
    clearPermission: systemPermissionCodes.logLogin.clear
  }
}
