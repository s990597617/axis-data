import { request } from '@/shared/http'

/** 岗位副本列表数据。 */
export interface PosCloneDto extends Record<string, unknown> {
  id: string
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 岗位副本保存数据。 */
export interface SavePosCloneInput {
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
}

export function getPosClonePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/demo/pos-clone/page', params })
}

export function createPosClone(data: SavePosCloneInput) {
  return request.post<void>({ url: '/demo/pos-clone', data })
}

export function updatePosClone(id: string, data: SavePosCloneInput) {
  return request.put<void>({ url: `/demo/pos-clone/${id}`, data })
}

export function deletePosClone(id: string) {
  return request.delete<void>({ url: `/demo/pos-clone/${id}` })
}
