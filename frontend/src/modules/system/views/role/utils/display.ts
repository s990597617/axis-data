import type { RoleGrantedMenuDto } from '@/modules/system/api/role'
import type { TreeOption } from 'naive-ui'

export const dataScopeOptions = [
  { label: '全部数据', value: 1 },
  { label: '自定义机构', value: 2 },
  { label: '本部门及以下', value: 3 },
  { label: '本部门', value: 4 },
  { label: '仅本人', value: 5 }
]

const menuTypePrefix: Record<number, string> = {
  0: '[目录]',
  1: '[菜单]',
  2: '[按钮]'
}

// 数据范围枚举值 → 文案
export function formatDataScope(value: number) {
  return dataScopeOptions.find(item => item.value === value)?.label ?? String(value)
}

// 授权菜单树 → NTree 只读选项
export function toGrantedMenuTreeOptions(menus: RoleGrantedMenuDto[]): TreeOption[] {
  return menus.map(menu => ({
    key: String(menu.id),
    label: formatGrantedMenuLabel(menu),
    children: menu.children?.length ? toGrantedMenuTreeOptions(menu.children) : undefined
  }))
}

function formatGrantedMenuLabel(menu: RoleGrantedMenuDto) {
  const prefix = menuTypePrefix[menu.type] ?? ''
  const title = menu.title || '未命名'
  if (menu.type === 2 && menu.permission) {
    return `${prefix} ${title}（${menu.permission}）`
  }
  return `${prefix} ${title}`.trim()
}

// 统计授权节点数（含目录/菜单/按钮）
export function countGrantedMenus(menus: RoleGrantedMenuDto[]): number {
  return menus.reduce((total, menu) => {
    return total + 1 + countGrantedMenus(menu.children ?? [])
  }, 0)
}
