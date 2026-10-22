import { request } from '@/shared/http'

export interface OrgTreeNode {
  id: string
  parentId: string
  name: string
  code?: string
  type: number
  orderNo: number
  status: number
  children: OrgTreeNode[]
}

/** 机构管理列表数据。 */
export interface OrgDto extends Record<string, unknown> {
  id: string
  parentId: string
  name: string
  code?: string
  type: number
  orderNo: number
  status: number
  remark?: string
  createdTime: string
  children?: OrgDto[]
}

/** 机构保存数据。 */
export type SaveOrgInput = Omit<OrgDto, 'id' | 'createdTime' | 'children'>

export function getOrgTree() {
  return request.get<OrgTreeNode[]>({ url: '/sys/org/tree' })
}

export function getOrgList(params: Record<string, unknown> = {}) {
  return request.get<OrgDto[]>({ url: '/sys/org/list', params })
}

export function createOrg(data: SaveOrgInput) {
  return request.post<void>({ url: '/sys/org', data })
}

export function updateOrg(id: string, data: SaveOrgInput) {
  return request.put<void>({ url: `/sys/org/${id}`, data })
}

export function deleteOrg(id: string) {
  return request.delete<void>({ url: `/sys/org/${id}` })
}
