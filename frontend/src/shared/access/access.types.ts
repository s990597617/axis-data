import type { AppRouteMeta, AppRouteRecord } from '@/shared/types/app-route'

/** 当前用户参与权限判断所需的最小身份信息。 */
export interface AccessSubject {
  /** 用户拥有的角色编码。 */
  roles?: readonly string[]
  /** 用户拥有的权限点编码。 */
  permissions?: readonly string[]
  /** 账号类型；999 为超级管理员（与后端 AccountTypeEnum.SuperAdmin 一致）。 */
  accountType?: number
}

/** 可参与访问控制判断的元信息。 */
export type AccessMeta = Pick<AppRouteMeta, 'roles' | 'permissions' | 'accessMode'>

/** 权限过滤函数接收的路由类型。 */
export type AccessRouteRecord = AppRouteRecord
