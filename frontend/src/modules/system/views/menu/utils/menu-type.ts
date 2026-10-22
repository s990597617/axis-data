// 与后端菜单节点类型枚举对齐：0 目录 / 1 菜单 / 2 按钮
export const MenuType = {
  Directory: 0,
  Menu: 1,
  Button: 2
} as const

export type MenuTypeValue = (typeof MenuType)[keyof typeof MenuType]

export const menuTypeOptions = [
  { label: '目录', value: MenuType.Directory },
  { label: '菜单', value: MenuType.Menu },
  { label: '按钮', value: MenuType.Button }
]

export const menuTypeMeta = {
  [MenuType.Directory]: {
    label: '目录',
    tagType: 'info' as const,
    icon: 'ant-design:folder-outlined'
  },
  [MenuType.Menu]: {
    label: '菜单',
    tagType: 'success' as const,
    icon: 'ant-design:menu-outlined'
  },
  [MenuType.Button]: {
    label: '按钮',
    tagType: 'warning' as const,
    icon: 'ant-design:control-outlined'
  }
}

// 树节点「+」：目录下默认建菜单，菜单下默认建按钮
export function defaultChildType(parentType: number): MenuTypeValue {
  if (parentType === MenuType.Directory) return MenuType.Menu
  if (parentType === MenuType.Menu) return MenuType.Button
  return MenuType.Menu
}
