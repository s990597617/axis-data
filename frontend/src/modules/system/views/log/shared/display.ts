import dayjs from 'dayjs'

import { formatDateTime } from '@/shared/components/axis-crud'
import type { DetailField } from '@/shared/components/detail-drawer'

/** HTTP 方法筛选项。 */
export const httpMethodOptions = [
  { label: 'GET', value: 'GET' },
  { label: 'POST', value: 'POST' },
  { label: 'PUT', value: 'PUT' },
  { label: 'DELETE', value: 'DELETE' },
  { label: 'PATCH', value: 'PATCH' }
]

/** 登录结果筛选项。 */
export const loginSuccessOptions = [
  { label: '成功', value: true },
  { label: '失败', value: false }
]

/**
 * 把 AxisTable 的 dateRange（毫秒时间戳数组）拆成 StartTime / EndTime。
 * 结束日取当天最后一刻，避免只筛到 00:00:00。
 */
export function withTimeRange(params: Record<string, unknown>) {
  const next = { ...params }
  const range = next.timeRange
  delete next.timeRange

  if (!Array.isArray(range) || range.length !== 2) return next

  const [start, end] = range
  if (start != null && start !== '') {
    next.startTime = dayjs(Number(start)).format('YYYY-MM-DD HH:mm:ss')
  }
  if (end != null && end !== '') {
    next.endTime = dayjs(Number(end)).endOf('day').format('YYYY-MM-DD HH:mm:ss')
  }
  return next
}

export function formatElapsed(ms?: number | null) {
  if (ms == null || Number.isNaN(Number(ms))) return '-'
  return `${ms} ms`
}

export function textOrDash(value?: string | null) {
  const text = (value ?? '').trim()
  return text || '-'
}

export function buildAuditDetailFields(row: {
  account?: string | null
  displayTitle?: string | null
  httpMethod?: string | null
  requestUrl?: string | null
  remoteIp?: string | null
  elapsed?: number | null
  state?: string | null
  traceId?: string | null
  userAgent?: string | null
  fullName?: string | null
  actionName?: string | null
  requestParam?: string | null
  createdTime?: string | null
}): DetailField[] {
  return [
    {
      key: 'account',
      label: '操作账号',
      value: textOrDash(row.account),
      icon: 'ant-design:user-outlined'
    },
    {
      key: 'displayTitle',
      label: '功能描述',
      value: textOrDash(row.displayTitle),
      icon: 'ant-design:profile-outlined'
    },
    {
      key: 'httpMethod',
      label: '请求方式',
      value: textOrDash(row.httpMethod),
      type: 'tag',
      tagType: 'info',
      icon: 'ant-design:api-outlined'
    },
    {
      key: 'state',
      label: '状态码',
      value: textOrDash(row.state),
      type: 'tag',
      tagType: String(row.state || '').startsWith('2') ? 'success' : 'warning',
      icon: 'ant-design:check-circle-outlined'
    },
    {
      key: 'requestUrl',
      label: '请求地址',
      value: textOrDash(row.requestUrl),
      fullRow: true,
      type: 'code',
      icon: 'ant-design:link-outlined'
    },
    {
      key: 'remoteIp',
      label: 'IP 地址',
      value: textOrDash(row.remoteIp),
      icon: 'ant-design:global-outlined'
    },
    {
      key: 'elapsed',
      label: '耗时',
      value: formatElapsed(row.elapsed),
      icon: 'ant-design:dashboard-outlined'
    },
    {
      key: 'traceId',
      label: '跟踪 ID',
      value: textOrDash(row.traceId),
      type: 'code',
      icon: 'ant-design:number-outlined'
    },
    {
      key: 'createdTime',
      label: '发生时间',
      value: formatDateTime(row.createdTime),
      icon: 'ant-design:clock-circle-outlined'
    },
    {
      key: 'actionName',
      label: '方法名称',
      value: textOrDash(row.actionName),
      icon: 'ant-design:code-outlined'
    },
    {
      key: 'fullName',
      label: '方法全名',
      value: textOrDash(row.fullName),
      fullRow: true,
      type: 'code',
      icon: 'ant-design:file-text-outlined'
    },
    {
      key: 'userAgent',
      label: 'User-Agent',
      value: textOrDash(row.userAgent),
      fullRow: true,
      icon: 'ant-design:chrome-outlined'
    },
    {
      key: 'requestParam',
      label: '请求参数',
      value: textOrDash(row.requestParam),
      fullRow: true,
      type: 'pre',
      icon: 'ant-design:form-outlined'
    }
  ]
}
