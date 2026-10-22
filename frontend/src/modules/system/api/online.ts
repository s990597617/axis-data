import { request } from '@/shared/http'

/** 在线会话。 */
export interface OnlineSessionDto extends Record<string, unknown> {
  id: string
  userId: string
  account: string
  realName?: string
  accountType: number
  clientIp?: string
  userAgent?: string
  createdTime: string
  expiryDate: string
  rememberMe: boolean
  isCurrent: boolean
}

export function getOnlineSessionPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/online/page', params })
}

export function forceLogoutSession(sessionId: string) {
  return request.post<void>({ url: `/sys/online/${sessionId}/kick` })
}

export function forceLogoutUser(userId: string) {
  return request.post<void>({ url: `/sys/online/user/${userId}/kick` })
}
