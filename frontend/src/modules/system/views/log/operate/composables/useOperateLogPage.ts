import { ref } from 'vue'

import {
  clearOperateLogs,
  getOperateLogPage,
  type OperateLogDto
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

import type OperateLogDetailDrawer from '../components/OperateLogDetailDrawer.vue'
import {
  formatLogTime,
  httpMethodOptions,
  withTimeRange
} from '../../shared/display'

export function useOperateLogPage() {
  /* ============ 状态 & 引用 ============ */

  const tableRef = ref<AxisTableAction<OperateLogDto> | null>(null)
  const detailRef = ref<InstanceType<typeof OperateLogDetailDrawer> | null>(null)
  const clearing = ref(false)

  /* ============ 表格配置 ============ */

  const columns: AxisTableColumn<OperateLogDto>[] = [
    {
      key: 'account',
      title: '账号',
      width: 120,
      search: { field: 'keyword', placeholder: '账号 / 地址 / 功能', primary: true }
    },
    {
      key: 'displayTitle',
      title: '功能描述',
      width: 160,
      ellipsis: { tooltip: true },
      render: row => row.displayTitle || '-'
    },
    {
      key: 'httpMethod',
      title: '方法',
      width: 90,
      search: true,
      dict: httpMethodOptions
    },
    {
      key: 'requestUrl',
      title: '请求地址',
      ellipsis: { tooltip: true },
      render: row => row.requestUrl || '-'
    },
    {
      key: 'remoteIp',
      title: 'IP',
      width: 130,
      render: row => row.remoteIp || '-'
    },
    {
      key: 'elapsed',
      title: '耗时(ms)',
      width: 100
    },
    {
      key: 'state',
      title: '状态',
      width: 90,
      render: row => row.state || '-'
    },
    {
      key: 'createdTime',
      title: '操作时间',
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

  const actionColumn: AxisTableActionColumn<OperateLogDto> = {
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
    return getOperateLogPage(withTimeRange(params))
  }

  function handleClear() {
    dialog.warning({
      title: '清空操作日志',
      content: '将删除全部操作日志，且不可恢复，确认继续？',
      positiveText: '清空',
      negativeText: '取消',
      onPositiveClick: async () => {
        clearing.value = true
        const [, error] = await tryRequest(clearOperateLogs(), {
          fallbackMessage: '清空操作日志失败'
        })
        clearing.value = false
        if (error) return
        message.success('操作日志已清空')
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
    clearPermission: systemPermissionCodes.logOperate.clear
  }
}
