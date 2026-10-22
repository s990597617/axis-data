import { request } from '@/shared/http'

export interface LoginLogDto extends Record<string, unknown> {
  id: string | number
  userId: string | number
  account: string
  remoteIp?: string | null
  userAgent?: string | null
  success: boolean
  failReason?: string | null
  createdTime: string
}

export interface AuditLogDto extends Record<string, unknown> {
  id: string | number
  userId?: string | number | null
  account?: string | null
  displayTitle?: string | null
  fullName?: string | null
  actionName?: string | null
  httpMethod?: string | null
  requestUrl?: string | null
  requestParam?: string | null
  remoteIp?: string | null
  userAgent?: string | null
  elapsed: number
  state?: string | null
  traceId?: string | null
  createdTime: string
}

export interface ErrorLogDto extends Record<string, unknown> {
  id: string | number
  displayTitle?: string | null
  fullName?: string | null
  actionName?: string | null
  httpMethod?: string | null
  requestUrl?: string | null
  requestParam?: string | null
  remoteIp?: string | null
  userAgent?: string | null
  elapsed: number
  exception?: string | null
  traceId?: string | null
  threadId: number
  createdTime: string
}

export function getLoginLogPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/log/login/page', params })
}

export function clearLoginLogs() {
  return request.delete<void>({ url: '/sys/log/login' })
}

export function getVisitLogPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/log/visit/page', params })
}

export function clearVisitLogs() {
  return request.delete<void>({ url: '/sys/log/visit' })
}

export function getOperateLogPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/log/operate/page', params })
}

export function clearOperateLogs() {
  return request.delete<void>({ url: '/sys/log/operate' })
}

export function getErrorLogPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/log/error/page', params })
}

export function clearErrorLogs() {
  return request.delete<void>({ url: '/sys/log/error' })
}
