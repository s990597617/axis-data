import { request } from '@/shared/http'

export interface MaterialCategoryTreeNode {
  id: string
  parentId: string
  name: string
  code: string
  orderNo: number
  status: number
  children: MaterialCategoryTreeNode[]
}

/** 物料分类管理列表数据。 */
export interface MaterialCategoryDto extends Record<string, unknown> {
  id: string
  parentId: string
  name: string
  code: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
  children?: MaterialCategoryDto[]
}

/** 物料分类保存数据。 */
export type SaveMaterialCategoryInput = Omit<MaterialCategoryDto, 'id' | 'createdTime' | 'children'>

export function getMaterialCategoryTree() {
  return request.get<MaterialCategoryTreeNode[]>({ url: '/mdm/material-category/tree' })
}

export function getMaterialCategoryList(params: Record<string, unknown> = {}) {
  return request.get<MaterialCategoryDto[]>({ url: '/mdm/material-category/list', params })
}

export function createMaterialCategory(data: SaveMaterialCategoryInput) {
  return request.post<void>({ url: '/mdm/material-category', data })
}

export function updateMaterialCategory(id: string, data: SaveMaterialCategoryInput) {
  return request.put<void>({ url: `/mdm/material-category/${id}`, data })
}

export function deleteMaterialCategory(id: string) {
  return request.delete<void>({ url: `/mdm/material-category/${id}` })
}
