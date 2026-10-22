// 公告类型（与后端 NoticeTypeEnum 对齐）
export const noticeTypeOptions = [
  { label: '通知', value: 1 },
  { label: '公告', value: 2 }
]

// 发布范围（与后端 NoticeTargetTypeEnum 对齐）
export const noticeTargetTypeOptions = [
  { label: '全部用户', value: 0 },
  { label: '指定角色', value: 1 },
  { label: '指定机构', value: 2 },
  { label: '指定用户', value: 3 }
]
