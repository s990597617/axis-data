import type { MenuDto } from '@/modules/system/api/menu'

import { MenuType, menuTypeMeta } from './menu-type'

// 菜单图标：优先自定义，否则按类型回退
export function menuIcon(menu: MenuDto) {
  return (
    menu.icon ||
    menuTypeMeta[menu.type as keyof typeof menuTypeMeta]?.icon ||
    menuTypeMeta[MenuType.Menu].icon
  )
}

// 节点类型中文标签
export function menuTypeLabel(menu: MenuDto) {
  return menuTypeMeta[menu.type as keyof typeof menuTypeMeta]?.label ?? '未知'
}

// 空值展示占位
export function displayValue(value: unknown) {
  if (value === undefined || value === null || value === '') return '—'
  return String(value)
}
