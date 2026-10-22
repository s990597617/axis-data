import { request } from '@/shared/http'

/** 物料列表数据。 */
export interface MaterialDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  spec?: string
  categoryId?: string
  categoryName?: string
  unit: string
  sourceType: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 物料保存数据。 */
export interface SaveMaterialInput {
  name: string
  code?: string
  spec?: string
  categoryId?: string | null
  unit: string
  sourceType: string
  orderNo: number
  status: number
  remark?: string
}

/** 物料下一编码预览。 */
export interface MaterialNextCodeDto {
  code: string
}

export function getMaterialPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/mdm/material/page', params })
}

export function createMaterial(data: SaveMaterialInput) {
  return request.post<void>({ url: '/mdm/material', data })
}

export function previewMaterialCode() {
  return request.get<MaterialNextCodeDto>({ url: '/mdm/material/next-code' })
}

export function updateMaterial(id: string, data: SaveMaterialInput) {
  return request.put<void>({ url: `/mdm/material/${id}`, data })
}

export function deleteMaterial(id: string) {
  return request.delete<void>({ url: `/mdm/material/${id}` })
}
