import { request } from '@/shared/http'

// 收件箱条目（统一消息中心）
export interface InboxItemDto extends Record<string, unknown> {
  id: string
  sourceType: number
  sourceId: string
  title: string
  summary?: string
  content?: string
  isRead: boolean
  createdTime: string
  senderName?: string
  bizModule?: string
  bizType?: string
  bizId?: string
  linkPath?: string
  resolvedLink?: string
  extraJson?: string
  priority: number
}

// 未读统计
export interface UnreadCountDto {
  total: number
  noticeCount: number
  messageCount: number
  otherCount: number
}

export function getUnreadCount() {
  return request.get<UnreadCountDto>({ url: '/sys/inbox/unread-count' })
}

export function getInboxPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/inbox/page', params })
}

export function getInboxItem(id: string) {
  return request.get<InboxItemDto>({ url: `/sys/inbox/${id}` })
}

export function markInboxRead(id: string) {
  return request.put<void>({ url: `/sys/inbox/${id}/read` })
}

export function markInboxAllRead() {
  return request.put<void>({ url: '/sys/inbox/read-all' })
}
