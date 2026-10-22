import { request } from '@/shared/http'

// 通知公告列表/详情
export interface NoticeDto extends Record<string, unknown> {
  id: string
  title: string
  content: string
  noticeType: number
  targetType: number
  targetIds?: string
  publishTime?: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

// 保存通知公告
export interface SaveNoticeInput {
  id?: string
  title: string
  content: string
  noticeType: number
  targetType: number
  targetIds?: string
  orderNo: number
  status: number
  remark?: string
}

export function getNoticePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/notice/page', params })
}

export function createNotice(data: SaveNoticeInput) {
  return request.post<void>({ url: '/sys/notice', data })
}

export function updateNotice(id: string, data: SaveNoticeInput) {
  return request.put<void>({ url: `/sys/notice/${id}`, data })
}

export function deleteNotice(id: string) {
  return request.delete<void>({ url: `/sys/notice/${id}` })
}

export function publishNotice(id: string, published: boolean) {
  return request.put<void>({ url: `/sys/notice/${id}/publish`, data: { published } })
}
