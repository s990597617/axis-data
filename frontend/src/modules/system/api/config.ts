import { request } from '@/shared/http'

/** 参数配置列表数据。 */
export interface ConfigDto extends Record<string, unknown> {
  id: string
  groupCode?: string
  name: string
  code: string
  value?: string
  type: number
  isBuiltin: boolean
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 参数配置保存数据。 */
export interface SaveConfigInput {
  groupCode?: string
  name: string
  code: string
  value?: string
  type: number
  orderNo: number
  status: number
  remark?: string
}

export function getConfigPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/config/page', params })
}

export function createConfig(data: SaveConfigInput) {
  return request.post<void>({ url: '/sys/config', data })
}

export function updateConfig(id: string, data: SaveConfigInput) {
  return request.put<void>({ url: '/sys/config/' + id, data })
}

export function deleteConfig(id: string) {
  return request.delete<void>({ url: '/sys/config/' + id })
}
