/**
 * DataHub 域权限码。
 *
 * 与后端 DataHubPermissionCodes 保持一一对应，页面不得自行拼写权限码。
 */
export const dataHubPermissionCodes = {
  source: createCrudPermissions('source'),
  dataset: createCrudPermissions('dataset'),
  ingestJob: {
    ...createCrudPermissions('ingest-job'),
    run: 'datahub:ingest-job:run'
  },
  ingestRun: createCrudPermissions('ingest-run'),
}

/**
 * 生成遵循统一命名约定的基础增删改查权限码。
 */
function createCrudPermissions(module: string) {
  return {
    query: `datahub:${module}:query`,
    create: `datahub:${module}:create`,
    edit: `datahub:${module}:edit`,
    delete: `datahub:${module}:delete`
  } as const
}

export type CrudPermissionSet = {
  query: string
  create: string
  edit: string
  delete: string
}
