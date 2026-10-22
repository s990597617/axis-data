import { request } from '@/shared/http'

/** 菜单、目录或按钮节点。 */
export interface MenuDto extends Record<string, unknown> {
  id: string
  parentId: string
  type: number
  title: string
  name?: string
  path?: string
  component?: string
  permission?: string
  icon?: string
  redirect?: string
  isFrame: boolean
  isKeepAlive: boolean
  isHide: boolean
  isPlatform?: boolean
  orderNo: number
  status: number
  remark?: string
  children?: MenuDto[]
}

/** 菜单节点保存数据。 */
export type SaveMenuInput = Omit<MenuDto, 'id' | 'children'>

/** 权限目录中的单条动作。 */
export interface PermissionCatalogItem {
  code: string
  actionName: string
}

/** 权限目录模块分组。 */
export interface PermissionCatalogModule {
  module: string
  moduleName: string
  items: PermissionCatalogItem[]
}

export function getMenuList(params: Record<string, unknown> = {}) {
  return request.get<MenuDto[]>({ url: '/sys/menu/list', params })
}

// 系统权限码目录（按模块分组），配按钮时选用
export function getMenuPermissionCatalog() {
  return request.get<PermissionCatalogModule[]>({ url: '/sys/menu/permission-catalog' })
}

export function createMenu(data: SaveMenuInput) {
  return request.post<void>({ url: '/sys/menu', data })
}

export function updateMenu(id: string, data: SaveMenuInput) {
  return request.put<void>({ url: `/sys/menu/${id}`, data })
}

export function deleteMenu(id: string) {
  return request.delete<void>({ url: `/sys/menu/${id}` })
}
