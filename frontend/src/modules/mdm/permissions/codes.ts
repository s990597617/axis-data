/**
 * Mdm 域权限码。
 *
 * 与后端 MdmPermissionCodes 保持一一对应，页面不得自行拼写权限码。
 */
export const mdmPermissionCodes = {
  material: createCrudPermissions('material'),
  materialCategory: createCrudPermissions('materialCategory'),
  process: createCrudPermissions('process'),
  routing: createCrudPermissions('routing')
}

/**
 * 生成遵循统一命名约定的基础增删改查权限码。
 */
function createCrudPermissions(module: string) {
  return {
    query: `mdm:${module}:query`,
    create: `mdm:${module}:create`,
    edit: `mdm:${module}:edit`,
    delete: `mdm:${module}:delete`
  } as const
}

export type CrudPermissionSet = {
  query: string
  create: string
  edit: string
  delete: string
}
