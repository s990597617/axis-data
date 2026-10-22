import { request } from '@/shared/http'

import type { MenuDto } from '@/modules/system/api/menu'

/** 角色列表数据。 */
export interface RoleDto extends Record<string, unknown> {
  id: string
  version: string
  name: string
  code: string
  dataScope: number
  orgIds?: string[]
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 角色保存数据。 */
export interface SaveRoleInput {
  version?: string
  name: string
  code: string
  dataScope: number
  orgIds?: string[]
  orderNo: number
  status: number
  remark?: string
}

/** 角色已授权菜单节点。 */
export interface RoleGrantedMenuDto {
  id: string
  parentId: string
  type: number
  title: string
  permission?: string
  children?: RoleGrantedMenuDto[]
}

/** 角色详情（含已授权菜单树）。 */
export interface RoleDetailDto extends RoleDto {
  ownAllPermissions: boolean
  orgNames?: string[]
  menus: RoleGrantedMenuDto[]
}

export function getRolePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/role/page', params })
}

export function getRoleDetail(id: string) {
  return request.get<RoleDetailDto>({ url: `/sys/role/${id}` })
}

export function createRole(data: SaveRoleInput) {
  return request.post<void>({ url: '/sys/role', data })
}

export function updateRole(id: string, data: SaveRoleInput) {
  return request.put<void>({ url: `/sys/role/${id}`, data })
}

export function deleteRole(id: string) {
  return request.delete<void>({ url: `/sys/role/${id}` })
}

export function getRoleMenuIds(id: string) {
  return request.get<string[]>({ url: `/sys/role/${id}/menus` })
}

export function getGrantableRoleMenus() {
  return request.get<MenuDto[]>({ url: '/sys/role/menus/grantable' })
}

export function grantRoleMenus(id: string, menuIds: string[]) {
  return request.put<void>({ url: `/sys/role/${id}/menus`, data: { menuIds } })
}
