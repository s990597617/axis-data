import { request } from '@/shared/http'

/** SKU列表数据。 */
export interface SkuDto extends Record<string, unknown> {
  id: string
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** SKU保存数据。 */
export interface SaveSkuInput {
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
}

export function getSkuPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/demo/sku/page', params })
}

export function createSku(data: SaveSkuInput) {
  return request.post<void>({ url: '/demo/sku', data })
}

export function updateSku(id: string, data: SaveSkuInput) {
  return request.put<void>({ url: `/demo/sku/${id}`, data })
}

export function deleteSku(id: string) {
  return request.delete<void>({ url: `/demo/sku/${id}` })
}
