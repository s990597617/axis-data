import { h, ref } from 'vue'
import { NTag } from 'naive-ui'

import { formatDateTime } from '@/shared/components/axis-crud'
import type { AxisTableAction, AxisTableColumn } from '@/shared/components/axis-table'
import type { DetailField } from '@/shared/components/detail-drawer'
import { dialog, message } from '@/shared/utils/message'
import { tryRequest } from '@/shared/http'

import {
  formatElapsed,
  httpMethodOptions,
  textOrDash,
  withTimeRange
} from './display'

type ClearFn = () => Promise<unknown>
type PageFn = (params: Record<string, unknown>) => Promise<unknown>

export function useLoginLogListPage(options: {
  permissions: { query: string; clear: string }
  pageRequest: PageFn
  clearRequest: ClearFn
}) {
  const tableRef = ref<AxisTableAction | null>(null)
  const detailVisible = ref(false)
  const detailFields = ref<DetailField[]>([])
  const detailSubtitle = ref('')

  const columns: AxisTableColumn[] = [
    {
      key: 'account',
      title: '账号',
      search: { field: 'keyword', placeholder: '账号 / IP', primary: true }
    },
    {
      key: 'success',
      title: '结果',
      width: 90,
      search: {
        field: 'success',
        component: 'select',
        options: [
          { label: '成功', value: true },
          { label: '失败', value: false }
        ]
      },
      render: row =>
        h(
          NTag,
          { size: 'small', type: row.success ? 'success' : 'error', bordered: false },
          { default: () => (row.success ? '成功' : '失败') }
        )
    },
    { key: 'remoteIp', title: 'IP', width: 140, render: row => textOrDash(row.remoteIp as string) },
    {
      key: 'failReason',
      title: '失败原因',
      ellipsis: { tooltip: true },
      render: row => textOrDash(row.failReason as string)
    },
    {
      key: 'createdTime',
      title: '登录时间',
      width: 180,
      search: {
        field: 'timeRange',
        label: '时间范围',
        component: 'dateRange',
        advanced: true,
        span: 2
      },
      render: row => formatDateTime(row.createdTime)
    }
  ]

  function request(params: Record<string, unknown>) {
    return options.pageRequest(withTimeRange(params))
  }

  function showDetail(row: Record<string, unknown>) {
    detailSubtitle.value = String(row.account || '')
    detailFields.value = [
      {
        key: 'account',
        label: '账号',
        value: textOrDash(row.account as string),
        icon: 'ant-design:user-outlined'
      },
      {
        key: 'success',
        label: '结果',
        value: row.success ? '成功' : '失败',
        type: 'tag',
        tagType: row.success ? 'success' : 'error',
        icon: 'ant-design:check-circle-outlined'
      },
      {
        key: 'remoteIp',
        label: 'IP 地址',
        value: textOrDash(row.remoteIp as string),
        icon: 'ant-design:global-outlined'
      },
      {
        key: 'createdTime',
        label: '登录时间',
        value: formatDateTime(row.createdTime),
        icon: 'ant-design:clock-circle-outlined'
      },
      {
        key: 'failReason',
        label: '失败原因',
        value: textOrDash(row.failReason as string),
        fullRow: true,
        icon: 'ant-design:warning-outlined'
      },
      {
        key: 'userAgent',
        label: 'User-Agent',
        value: textOrDash(row.userAgent as string),
        fullRow: true,
        icon: 'ant-design:chrome-outlined'
      }
    ]
    detailVisible.value = true
  }

  function handleClear() {
    dialog.warning({
      title: '清空登录日志',
      content: '确定清空全部登录日志？此操作不可恢复。',
      positiveText: '确认清空',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(options.clearRequest())
        if (error) return
        message.success('已清空登录日志')
        tableRef.value?.reload()
      }
    })
  }

  return {
    columns,
    detailFields,
    detailSubtitle,
    detailVisible,
    handleClear,
    permissions: options.permissions,
    request,
    showDetail,
    tableRef
  }
}

export function useAuditLogListPage(options: {
  title: string
  permissions: { query: string; clear: string }
  pageRequest: PageFn
  clearRequest: ClearFn
  buildDetailFields: (row: Record<string, unknown>) => DetailField[]
}) {
  const tableRef = ref<AxisTableAction | null>(null)
  const detailVisible = ref(false)
  const detailFields = ref<DetailField[]>([])
  const detailSubtitle = ref('')

  const columns: AxisTableColumn[] = [
    {
      key: 'account',
      title: '账号',
      width: 120,
      search: { field: 'keyword', placeholder: '账号 / 功能 / 地址 / IP', primary: true },
      render: row => textOrDash(row.account as string)
    },
    {
      key: 'displayTitle',
      title: '功能描述',
      ellipsis: { tooltip: true },
      render: row => textOrDash(row.displayTitle as string)
    },
    {
      key: 'httpMethod',
      title: '方法',
      width: 90,
      search: {
        field: 'httpMethod',
        component: 'select',
        options: httpMethodOptions
      },
      render: row => textOrDash(row.httpMethod as string)
    },
    {
      key: 'requestUrl',
      title: '请求地址',
      ellipsis: { tooltip: true },
      render: row => textOrDash(row.requestUrl as string)
    },
    {
      key: 'remoteIp',
      title: 'IP',
      width: 130,
      render: row => textOrDash(row.remoteIp as string)
    },
    {
      key: 'elapsed',
      title: '耗时',
      width: 90,
      render: row => formatElapsed(row.elapsed as number)
    },
    {
      key: 'state',
      title: '状态',
      width: 80,
      render: row => textOrDash(row.state as string)
    },
    {
      key: 'createdTime',
      title: '时间',
      width: 180,
      search: {
        field: 'timeRange',
        label: '时间范围',
        component: 'dateRange',
        advanced: true,
        span: 2
      },
      render: row => formatDateTime(row.createdTime)
    }
  ]

  function request(params: Record<string, unknown>) {
    return options.pageRequest(withTimeRange(params))
  }

  function showDetail(row: Record<string, unknown>) {
    detailSubtitle.value = textOrDash((row.displayTitle || row.requestUrl) as string)
    detailFields.value = options.buildDetailFields(row)
    detailVisible.value = true
  }

  function handleClear() {
    dialog.warning({
      title: `清空${options.title}`,
      content: `确定清空全部${options.title}？此操作不可恢复。`,
      positiveText: '确认清空',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(options.clearRequest())
        if (error) return
        message.success(`已清空${options.title}`)
        tableRef.value?.reload()
      }
    })
  }

  return {
    columns,
    detailFields,
    detailSubtitle,
    detailVisible,
    handleClear,
    permissions: options.permissions,
    request,
    showDetail,
    tableRef
  }
}
