import type { TreeOption } from 'naive-ui'

import type { MenuDto } from '@/modules/system/api/menu'

export interface MenuTreeOption extends TreeOption {
  key: string
  label: string
  menu: MenuDto
  children?: MenuTreeOption[]
}

// 按排序号、标题排序菜单节点
export function compareMenu(left: MenuDto, right: MenuDto) {
  return left.orderNo - right.orderNo || left.title.localeCompare(right.title, 'zh-CN')
}

// 接口可能返回嵌套 children，展平为带 parentId 的列表
export function flattenMenuRows(source: MenuDto[]) {
  const result: MenuDto[] = []
  const visited = new Set<string>()

  function walk(row: MenuDto, fallbackParentId = '0') {
    if (visited.has(row.id)) return
    visited.add(row.id)
    const children = Array.isArray(row.children) ? row.children : []
    result.push({ ...row, parentId: row.parentId || fallbackParentId, children: undefined })
    children.forEach(child => walk(child, row.id))
  }

  source.forEach(row => walk(row))
  return result
}

// 平铺菜单行组装为左侧导航树（不含按钮）
export function buildMenuTree(source: MenuDto[]): MenuTreeOption[] {
  const nodeMap = new Map<string, MenuTreeOption>()
  source.forEach(menu => {
    nodeMap.set(menu.id, {
      key: menu.id,
      label: menu.title,
      menu,
      children: []
    })
  })

  const roots: MenuTreeOption[] = []
  nodeMap.forEach(node => {
    const parent = nodeMap.get(node.menu.parentId)
    if (parent && parent.key !== node.key) parent.children?.push(node)
    else roots.push(node)
  })

  function sortTree(nodes: MenuTreeOption[]) {
    nodes.sort((left, right) => compareMenu(left.menu, right.menu))
    nodes.forEach(node => {
      node.children = node.children?.length ? sortTree(node.children) : undefined
    })
    return nodes
  }

  return sortTree(roots)
}

// 关键字过滤树：命中节点保留完整子树，仅子树命中则只保留匹配分支
export function filterMenuTree(nodes: MenuTreeOption[], searchText: string): MenuTreeOption[] {
  if (!searchText) return nodes
  const normalizedKeyword = searchText.toLocaleLowerCase()

  return nodes.flatMap(node => {
    const matched = [node.menu.title, node.menu.name, node.menu.path]
      .filter(Boolean)
      .some(value => String(value).toLocaleLowerCase().includes(normalizedKeyword))
    const matchedChildren = filterMenuTree(node.children ?? [], searchText)
    if (!matched && !matchedChildren.length) return []
    return [{ ...node, children: matched ? node.children : matchedChildren }]
  })
}

// 上级节点树选择：禁选自身及全部子孙，避免环
export function toParentOptions(nodes: MenuTreeOption[], disabledIds: Set<string>): TreeOption[] {
  return nodes.map(node => ({
    key: node.key,
    label: node.label,
    disabled: disabledIds.has(node.key),
    children: node.children?.length ? toParentOptions(node.children, disabledIds) : undefined
  }))
}

// 收集树全部 key，用于搜索后展开
export function collectTreeKeys(nodes: MenuTreeOption[]): string[] {
  return nodes.flatMap(node => [node.key, ...collectTreeKeys(node.children ?? [])])
}

// 编辑时收集自身及全部子孙 Id，禁止选为上级
export function collectDescendantIds(rows: MenuDto[], rootId: string) {
  const result = new Set<string>([rootId])
  let changed = true
  while (changed) {
    changed = false
    rows.forEach(row => {
      if (result.has(row.parentId) && !result.has(row.id)) {
        result.add(row.id)
        changed = true
      }
    })
  }
  return result
}
