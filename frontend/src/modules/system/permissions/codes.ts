/**
 * 系统管理权限码。
 *
 * 此处与后端 SystemPermissionCodes 保持一一对应，页面和通用组件不得自行拼写权限码。
 */
export const systemPermissionCodes = {
  user: {
    query: 'sys:user:query',
    detail: 'sys:user:detail',
    create: 'sys:user:create',
    edit: 'sys:user:edit',
    delete: 'sys:user:delete',
    resetPassword: 'sys:user:resetPwd',
    import: 'sys:user:import',
    export: 'sys:user:export'
  },
  config: createCrudPermissions('config'),
  codeRule: {
    ...createCrudPermissions('codeRule'),
    preview: 'sys:codeRule:preview'
  },
  menu: createCrudPermissions('menu'),
  org: createCrudPermissions('org'),
  position: createCrudPermissions('pos'),
  tenant: {
    ...createCrudPermissions('tenant'),
    grant: 'sys:tenant:grant'
  },
  role: {
    ...createCrudPermissions('role'),
    detail: 'sys:role:detail',
    grant: 'sys:role:grant'
  },
  dict: {
    ...createCrudPermissions('dict'),
    dataQuery: 'sys:dict:data:query',
    dataCreate: 'sys:dict:data:create',
    dataEdit: 'sys:dict:data:edit',
    dataDelete: 'sys:dict:data:delete'
  },
  logLogin: {
    query: 'sys:logLogin:query',
    clear: 'sys:logLogin:clear'
  },
  logVisit: {
    query: 'sys:logVisit:query',
    clear: 'sys:logVisit:clear'
  },
  logOperate: {
    query: 'sys:logOperate:query',
    clear: 'sys:logOperate:clear'
  },
  logError: {
    query: 'sys:logError:query',
    clear: 'sys:logError:clear'
  },
  profile: {
    query: 'sys:profile:query',
    update: 'sys:profile:update',
    changePassword: 'sys:profile:changePwd'
  },
  monitor: {
    query: 'sys:monitor:query'
  },
  online: {
    query: 'sys:online:query',
    kick: 'sys:online:kick'
  },
  file: {
    query: 'sys:file:query',
    upload: 'sys:file:upload',
    download: 'sys:file:download',
    delete: 'sys:file:delete'
  },
  cache: {
    query: 'sys:cache:query',
    clear: 'sys:cache:clear'
  },
  codeGen: {
    query: 'sys:codeGen:query',
    preview: 'sys:codeGen:preview',
    generate: 'sys:codeGen:generate'
  },
  job: {
    ...createCrudPermissions('job'),
    run: 'sys:job:run',
    log: 'sys:job:log'
  },
  notice: {
    ...createCrudPermissions('notice'),
    publish: 'sys:notice:publish'
  },
  openApi: {
    ...createCrudPermissions('openApi'),
    resetSecret: 'sys:openApi:resetSecret'
  },
  inbox: {
    query: 'sys:inbox:query',
    read: 'sys:inbox:read',
    readAll: 'sys:inbox:readAll'
  }
} as const

/**
 * 生成遵循统一命名约定的基础增删改查权限码。
 */
function createCrudPermissions(module: string) {
  return {
    query: `sys:${module}:query`,
    create: `sys:${module}:create`,
    edit: `sys:${module}:edit`,
    delete: `sys:${module}:delete`
  } as const
}

export type CrudPermissionSet = {
  query: string
  create: string
  edit: string
  delete: string
}
