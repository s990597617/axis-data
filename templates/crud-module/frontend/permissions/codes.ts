/**
 * __Area__ 域权限码。
 *
 * 与后端 __PermissionCodesType__ 保持一一对应，页面不得自行拼写权限码。
 */
export const __permissionCodesVar__ = {
  __module__: createCrudPermissions('__route_key__')
}

/**
 * 生成遵循统一命名约定的基础增删改查权限码。
 */
function createCrudPermissions(module: string) {
  return {
    query: `__PermArea__:${module}:query`,
    create: `__PermArea__:${module}:create`,
    edit: `__PermArea__:${module}:edit`,
    delete: `__PermArea__:${module}:delete`
  } as const
}

export type CrudPermissionSet = {
  query: string
  create: string
  edit: string
  delete: string
}
