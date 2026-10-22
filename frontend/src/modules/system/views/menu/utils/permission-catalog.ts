import type { MenuDto, PermissionCatalogModule } from '@/modules/system/api/menu'

import { MenuType } from './menu-type'

// 从权限码提取模块键：sys:config:create → config；sys:dict:data:query → dict
export function extractModuleKey(code: string) {
  const parts = code.split(':').filter(Boolean)
  return parts.length >= 2 ? parts[1] : ''
}

export interface PermissionSelectOption {
  label?: string
  value?: string
  type?: 'group'
  key?: string
  disabled?: boolean
  children?: PermissionSelectOption[]
}

// 根据上级菜单推断推荐模块键
export function suggestModuleKey(
  parent: MenuDto | undefined,
  siblingButtons: MenuDto[],
  catalog: PermissionCatalogModule[]
) {
  if (!catalog.length) return undefined

  const moduleKeys = new Set(catalog.map(item => item.module))

  // 1. 上级菜单 Permission 作前缀，如 sys:config
  const parentPermission = parent?.permission?.trim()
  if (parentPermission) {
    const fromPrefix = extractModuleKey(parentPermission)
    if (fromPrefix && moduleKeys.has(fromPrefix)) return fromPrefix

    const byPrefix = catalog.find(module =>
      module.items.some(
        item => item.code === parentPermission || item.code.startsWith(`${parentPermission}:`)
      )
    )
    if (byPrefix) return byPrefix.module
  }

  // 2. 同级已有按钮的多数模块
  const siblingCounts = new Map<string, number>()
  siblingButtons.forEach(button => {
    const key = button.permission ? extractModuleKey(button.permission) : ''
    if (!key || !moduleKeys.has(key)) return
    siblingCounts.set(key, (siblingCounts.get(key) ?? 0) + 1)
  })
  if (siblingCounts.size) {
    return [...siblingCounts.entries()].sort((a, b) => b[1] - a[1])[0]?.[0]
  }

  // 3. 上级路由路径 / 组件路径包含模块键
  const haystack = [parent?.path, parent?.component, parent?.name]
    .filter(Boolean)
    .join('/')
    .toLowerCase()
  if (haystack) {
    const hit = catalog.find(
      module => haystack.includes(`/${module.module}`) || haystack.includes(module.module)
    )
    if (hit) return hit.module
  }

  // 4. 上级标题匹配模块中文名
  const title = parent?.title?.trim()
  if (title) {
    const byName = catalog.find(
      module => title.includes(module.moduleName) || module.moduleName.includes(title)
    )
    if (byName) return byName.module
  }

  return undefined
}

// 推荐模块下的动作列表（单选）
export function buildModuleActionOptions(
  catalog: PermissionCatalogModule[],
  usedCodes: Set<string>,
  moduleKey?: string
): PermissionSelectOption[] {
  const module = catalog.find(item => item.module === moduleKey)
  if (!module) return []
  return module.items.map(item => ({
    label: item.actionName,
    value: item.code,
    disabled: usedCodes.has(item.code)
  }))
}

// 全量分组选项：模块为 group，动作为可选项（单选，无多选框）
export function buildGroupedPermissionOptions(
  catalog: PermissionCatalogModule[],
  usedCodes: Set<string>
): PermissionSelectOption[] {
  return catalog.map(module => ({
    type: 'group',
    label: module.moduleName,
    key: module.module,
    children: module.items.map(item => ({
      label: `${item.actionName}（${item.code}）`,
      value: item.code,
      disabled: usedCodes.has(item.code)
    }))
  }))
}

export function findParentMenu(rows: MenuDto[], parentId: string) {
  return rows.find(row => row.id === parentId && row.type !== MenuType.Button)
}

export function listSiblingButtons(rows: MenuDto[], parentId: string, excludeId?: string) {
  return rows.filter(
    row => row.type === MenuType.Button && row.parentId === parentId && row.id !== excludeId
  )
}
