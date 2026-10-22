import { request } from '@/shared/http'

import type { MenuDto } from '@/modules/system/api/menu'

/** 租户列表数据。 */
export interface TenantDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  type: number
  dbConfigId?: string
  adminUserId?: string
  provisionStatus?: number
  provisionMessage?: string
  expireTime?: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 租户保存数据。 */
export interface SaveTenantInput {
  name: string
  code: string
  type: number
  dbConfigId?: string
  adminUserId?: string
  expireTime?: string
  orderNo: number
  status: number
  remark?: string
}

export function getTenantPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/tenant/page', params })
}

export function createTenant(data: SaveTenantInput) {
  return request.post<void>({ url: '/sys/tenant', data })
}

export function updateTenant(id: string, data: SaveTenantInput) {
  return request.put<void>({ url: `/sys/tenant/${id}`, data })
}

export function deleteTenant(id: string) {
  return request.delete<void>({ url: `/sys/tenant/${id}` })
}

/** 查询租户已启用菜单 Id。 */
export function getTenantMenuIds(id: string) {
  return request.get<string[]>({ url: `/sys/tenant/${id}/menus` })
}

/** 覆盖保存租户菜单启用集。 */
export function grantTenantMenus(id: string, menuIds: string[]) {
  return request.put<void>({ url: `/sys/tenant/${id}/menus`, data: { menuIds } })
}

/** 可分配给租户的菜单平铺列表（排除平台专属）。 */
export function getAssignableTenantMenus() {
  return request.get<MenuDto[]>({ url: '/sys/tenant/menus/assignable' })
}
