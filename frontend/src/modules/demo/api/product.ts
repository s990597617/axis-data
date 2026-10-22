import { request } from '@/shared/http'

/** 产品列表数据。 */
export interface ProductDto extends Record<string, unknown> {
  id: string
  name: string
  code?: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 产品保存数据。 */
export interface SaveProductInput {
  name: string
  code?: string
  orderNo: number
  status: number
  remark?: string
}

export function getProductPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/demo/product/page', params })
}

export function createProduct(data: SaveProductInput) {
  return request.post<void>({ url: '/demo/product', data })
}

export function updateProduct(id: string, data: SaveProductInput) {
  return request.put<void>({ url: `/demo/product/${id}`, data })
}

export function deleteProduct(id: string) {
  return request.delete<void>({ url: `/demo/product/${id}` })
}
