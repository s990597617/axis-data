import type { App, Directive, DirectiveBinding } from 'vue'

import { hasAnyPermission } from '@/shared/access'
import { useUserStore } from '@/store/modules/user.store'

type PermissionDirectiveValue = string | string[]

function check(value: PermissionDirectiveValue | undefined): boolean {
  if (!value) return true

  const userStore = useUserStore()
  const permissions = Array.isArray(value) ? value : [value]

  return hasAnyPermission(
    {
      roles: userStore.roles,
      permissions: userStore.permissions
    },
    permissions
  )
}

function update(el: HTMLElement, binding: DirectiveBinding<PermissionDirectiveValue>) {
  if (!check(binding.value)) {
    el.parentNode?.removeChild(el)
  }
}

/**
 * v-permission 按钮级权限指令。
 *
 * 指令值支持单个权限码或权限码数组，数组采用 OR 语义，命中任意一个即可显示元素。
 * 超级权限规则不在指令内单独维护，而是复用 shared/access 的统一判断。
 *
 * @example
 * ```vue
 * <n-button v-permission="'sys:user:create'">新增</n-button>
 * <n-button v-permission="['sys:user:delete', 'sys:user:batch-delete']">删除</n-button>
 * ```
 */
export const permissionDirective: Directive<HTMLElement, PermissionDirectiveValue> = {
  mounted: update,
  updated: update
}

export function setupPermissionDirective(app: App) {
  app.directive('permission', permissionDirective)
}
