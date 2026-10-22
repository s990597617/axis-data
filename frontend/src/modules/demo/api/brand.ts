import { request } from '@/shared/http'

/** 品牌列表数据。 */
export interface BrandDto extends Record<string, unknown> {
  id: string
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 品牌保存数据。 */
export interface SaveBrandInput {
  name: string
  code?: string
  orderNo?: number
  status: number
  remark?: string
}

export function getBrandPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/demo/brand/page', params })
}

export function createBrand(data: SaveBrandInput) {
  return request.post<void>({ url: '/demo/brand', data })
}

export function updateBrand(id: string, data: SaveBrandInput) {
  return request.put<void>({ url: `/demo/brand/${id}`, data })
}

export function deleteBrand(id: string) {
  return request.delete<void>({ url: `/demo/brand/${id}` })
}
