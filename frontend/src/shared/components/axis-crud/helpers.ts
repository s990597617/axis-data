import { h } from 'vue'
import { NTag } from 'naive-ui'

import { formatLocalDateTime } from '@/shared/utils/date'

// 通用启用状态选项（与字典 common_status / StatusEnum 对齐）
export const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 }
]

// 渲染统一的启用状态标签
export function renderStatus(status: unknown) {
  const enabled = status === 1
  return h(
    NTag,
    { type: enabled ? 'success' : 'error', size: 'small', round: true },
    { default: () => (enabled ? '启用' : '禁用') }
  )
}

// 列表时间列展示：统一走 dayjs 封装
export function formatDateTime(value: unknown) {
  if (typeof value !== 'string' && !(value instanceof Date) && typeof value !== 'number') {
    return '-'
  }
  return formatLocalDateTime(value) || '-'
}
