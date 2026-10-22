/**
 * Demo 域权限码。
 *
 * 与后端 DemoPermissionCodes 保持一一对应，页面不得自行拼写权限码。
 */
export const demoPermissionCodes = {
  product: createCrudPermissions('product'),
  posClone: createCrudPermissions('pos-clone'),
  sku: createCrudPermissions('sku'),
  brand: createCrudPermissions('brand'),
}

/**
 * 生成遵循统一命名约定的基础增删改查权限码。
 */
function createCrudPermissions(module: string) {
  return {
    query: `demo:${module}:query`,
    create: `demo:${module}:create`,
    edit: `demo:${module}:edit`,
    delete: `demo:${module}:delete`
  } as const
}

export type CrudPermissionSet = {
  query: string
  create: string
  edit: string
  delete: string
}
