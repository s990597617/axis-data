import type { TagProps } from 'naive-ui'

import { statusOptions } from '@/shared/components/axis-crud'

export const yesNoOptions = [
  { label: '是', value: true },
  { label: '否', value: false }
]

export type DictTagStyle = NonNullable<TagProps['type']>

// 标签样式选项：写入 tagStyle，列表与表单按 Naive Tag type 渲染
export const tagStyleOptions: Array<{
  label: string
  value: DictTagStyle
  color: string
}> = [
  { label: '默认', value: 'default', color: '#c2c2c8' },
  { label: '主色', value: 'primary', color: 'var(--axis-primary-color, #18a058)' },
  { label: '成功', value: 'success', color: '#18a058' },
  { label: '警告', value: 'warning', color: '#f0a020' },
  { label: '错误', value: 'error', color: '#d03050' },
  { label: '信息', value: 'info', color: '#2080f0' }
]

export function resolveTagStyleOption(value?: string | null) {
  return tagStyleOptions.find(item => item.value === value) ?? tagStyleOptions[0]
}

export { statusOptions }
