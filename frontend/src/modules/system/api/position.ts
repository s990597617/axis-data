import { request } from '@/shared/http'

/** 岗位列表数据。 */
export interface PositionDto extends Record<string, unknown> {
  id: string
  name: string
  code?: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 岗位保存数据。 */
export interface SavePositionInput {
  name: string
  code?: string
  orderNo: number
  status: number
  remark?: string
}

export function getPositionPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/pos/page', params })
}

export function createPosition(data: SavePositionInput) {
  return request.post<void>({ url: '/sys/pos', data })
}

export function updatePosition(id: string, data: SavePositionInput) {
  return request.put<void>({ url: `/sys/pos/${id}`, data })
}

export function deletePosition(id: string) {
  return request.delete<void>({ url: `/sys/pos/${id}` })
}
