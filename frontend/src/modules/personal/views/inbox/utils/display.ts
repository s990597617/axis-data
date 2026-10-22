// 收件箱来源类型（与后端 InboxSourceTypeEnum 对齐）
export const inboxSourceTypeOptions = [
  { label: '公告', value: 1 },
  { label: '站内信', value: 2 },
  { label: '系统', value: 9 },
  { label: '报警', value: 10 },
  { label: '工单', value: 20 }
]

// 已读筛选
export const inboxReadOptions = [
  { label: '未读', value: false },
  { label: '已读', value: true }
]

export function inboxSourceTypeLabel(type: number) {
  return inboxSourceTypeOptions.find(item => item.value === type)?.label ?? '消息'
}
