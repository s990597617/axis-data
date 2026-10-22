import type { MenuOption } from 'naive-ui'
import { NIcon } from 'naive-ui'
import { computed, h, ref, watch, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { buildNameToPathMap, buildRouteTree, type RouteNode } from '@/router/helpers/route-tree'
import { staticRoutes } from '@/router/routes/static-routes'
import { Icon } from '@/shared/icons'
import type { AppRouteRecord } from '@/shared/types/app-route'
import { useAppStore } from '@/store/modules/app.store'
import { usePermissionStore } from '@/store/modules/permission.store'

/**
 * 一级菜单简化项
 *
 * @description
 * 保留 `meta.icon` 原始字符串，便于如分栏布局图标栏等场景自定义渲染。
 */
export interface RootMenuItem {
  key: string
  label: string
  icon?: string
}

/**
 * 布局菜单 Hook 返回值
 */
export interface UseLayoutMenuReturn {
  /** 完整菜单选项（树形） */
  menuOptions: Ref<MenuOption[]>
  /** 一级菜单列表（去除 children，仅保留顶级节点） */
  rootMenus: Ref<MenuOption[]>
  /** 当前激活一级菜单 key */
  activeRootKey: Ref<string>
  /** 当前激活一级菜单下的二级菜单列表 */
  subMenus: Ref<MenuOption[]>
  /** 一级菜单简化项（带原始图标字符串，供自定义渲染如图标栏使用） */
  rootMenuItems: Ref<RootMenuItem[]>
  /** 展开的子菜单 key 列表 */
  expandKeys: Ref<string[]>
  /** 更新展开菜单 */
  updateExpandKeys: (keys: string[]) => void
  /** 当前选中的菜单 key（叶子节点） */
  currentMenu: Ref<string>
  /** 选择菜单并跳转 */
  updateValue: (key: string) => void
  /** 切换激活的一级菜单（自动跳转到其第一个可访问叶子） */
  setActiveRoot: (key: string) => void
}

/**
 * 生成菜单图标渲染函数
 */
function renderIcon(iconName?: string): (() => ReturnType<typeof h>) | undefined {
  if (!iconName) return undefined
  return () => h(NIcon, null, { default: () => h(Icon, { icon: iconName }) })
}

/** 带深度标记的菜单项，供侧栏缩进样式使用。 */
type AxisMenuOption = MenuOption & { axisMenuDepth?: number }

/**
 * 将路由 IR 转换为 naive-ui 菜单树。
 * depth 从 1 起算；三级及以下不显示图标，并由样式单独控制缩进。
 */
function toMenuOptions(tree: readonly RouteNode[], depth = 1): AxisMenuOption[] {
  return tree.map(node => {
    const iconRender = depth >= 3 ? undefined : renderIcon(node.icon)
    const option: AxisMenuOption = {
      label: node.title,
      key: node.name,
      axisMenuDepth: depth,
      ...(iconRender ? { icon: iconRender } : {})
    }
    if (node.children?.length) option.children = toMenuOptions(node.children, depth + 1)
    return option
  })
}

/**
 * 三级菜单挂 class，用于把文字左缘对齐到二级标题（而非二级图标）。
 */
export function layoutMenuNodeProps(option: MenuOption) {
  const depth = (option as AxisMenuOption).axisMenuDepth
  if (depth !== undefined && depth >= 3) {
    return { class: 'axis-menu-level-3' }
  }
  return undefined
}

/**
 * 判断菜单 IR 中是否包含指定 name
 */
function treeContains(tree: readonly RouteNode[], name: string): boolean {
  for (const node of tree) {
    if (node.name === name) return true
    if (node.children?.length && treeContains(node.children, name)) return true
  }
  return false
}

/**
 * 递归查找菜单节点下的第一个叶子节点 key
 */
function findFirstLeafKey(option: MenuOption): string | null {
  if (!option.children || option.children.length === 0) {
    return typeof option.key === 'string' ? option.key : null
  }
  for (const child of option.children) {
    const leaf = findFirstLeafKey(child as MenuOption)
    if (leaf) return leaf
  }
  return null
}

/**
 * 布局菜单消费 Hook
 *
 * @description
 * 负责基于**路由记录**生成菜单树并同步选中/展开态，不处理菜单的渲染样式。
 * 路由元信息为唯一数据源，权限过滤由 `permission.store` 完成。
 */
export function useLayoutMenu(): UseLayoutMenuReturn {
  const route = useRoute()
  const router = useRouter()
  const permissionStore = usePermissionStore()
  const appStore = useAppStore()

  /** 菜单源：静态路由 + 动态路由 */
  const appRoutes = computed<AppRouteRecord[]>(() => [
    ...(staticRoutes as AppRouteRecord[]),
    ...permissionStore.accessRoutes
  ])

  /** 统一的路由 IR，naive 菜单 / 激活判断均基于此 */
  const menuTree = computed<RouteNode[]>(() => buildRouteTree(appRoutes.value))
  const menuOptions = computed<MenuOption[]>(() => toMenuOptions(menuTree.value))
  const routePathMap = computed(() => buildNameToPathMap(appRoutes.value))

  const expandKeys = ref<string[]>([])
  const currentMenu = ref<string>('')

  /**
   * 根据当前路由同步菜单选中与展开
   */
  function syncFromRoute() {
    const matchedNames = route.matched
      .map(item => item.name)
      .filter((name): name is string => typeof name === 'string')
      .filter(name => treeContains(menuTree.value, name))

    if (!matchedNames.length) {
      currentMenu.value = ''
      expandKeys.value = []
      return
    }

    currentMenu.value = matchedNames.at(-1) ?? ''

    // 手风琴模式下，只保留当前激活链路的最末一个父级
    if (appStore.layout.accordionMenu) {
      const parents = matchedNames.slice(0, -1)
      expandKeys.value = parents.slice(-1)
    } else {
      expandKeys.value = matchedNames.slice(0, -1)
    }
  }

  watch(() => route.fullPath, syncFromRoute, { immediate: true })

  function updateExpandKeys(keys: string[]) {
    expandKeys.value = keys
  }

  function updateValue(key: string) {
    currentMenu.value = key
    const targetPath = routePathMap.value.get(key)
    if (targetPath && targetPath !== route.path) {
      router.push(targetPath)
    }
  }

  /**
   * 一级菜单列表（剔除 children，仅保留顶级节点）
   *
   * @description
   * 常用于顶部 / 侧边图标栏等只展示一级入口的场景。
   */
  const rootMenus = computed<MenuOption[]>(() =>
    menuOptions.value.map(({ children: _children, ...rest }) => rest)
  )

  const rootMenuItems = computed<RootMenuItem[]>(() =>
    menuTree.value.map(node => ({ key: node.name, label: node.title, icon: node.icon }))
  )

  /**
   * 当前激活的一级菜单 key
   *
   * @description
   * 基于 `route.matched` 链路与菜单树的首层 key 求交集；若无匹配则落到第一个一级菜单。
   */
  const activeRootKey = computed<string>(() => {
    const rootKeys = menuOptions.value
      .map(m => m.key)
      .filter((k): k is string => typeof k === 'string')
    const matchedNames = route.matched
      .map(item => item.name)
      .filter((name): name is string => typeof name === 'string')
    return matchedNames.find(name => rootKeys.includes(name)) ?? rootKeys[0] ?? ''
  })

  /**
   * 当前激活一级菜单下的二级菜单
   */
  const subMenus = computed<MenuOption[]>(() => {
    const root = menuOptions.value.find(m => m.key === activeRootKey.value)
    return (root?.children as MenuOption[] | undefined) ?? []
  })

  /**
   * 递归查找菜单节点下的第一个叶子节点 key
   */
  function findFirstLeafKey(option: MenuOption): string | null {
    if (!option.children || option.children.length === 0) {
      return typeof option.key === 'string' ? option.key : null
    }
    for (const child of option.children) {
      const leaf = findFirstLeafKey(child as MenuOption)
      if (leaf) return leaf
    }
    return null
  }

  /**
   * 切换激活的一级菜单并跳转到其第一个叶子
   */
  function setActiveRoot(key: string) {
    const root = menuOptions.value.find(m => m.key === key)
    if (!root) return
    const leafKey = findFirstLeafKey(root)
    if (leafKey) updateValue(leafKey)
  }

  return {
    menuOptions,
    rootMenus,
    rootMenuItems,
    activeRootKey,
    subMenus,
    expandKeys,
    updateExpandKeys,
    currentMenu,
    updateValue,
    setActiveRoot
  }
}
