import { request } from '@/shared/http'

export interface UserPageQuery {
  page: number
  pageSize: number
  orgId?: string
  keyword?: string
  status?: number
}

export interface UserDto extends Record<string, unknown> {
  id: string
  version: string
  orgId: string
  orgName: string
  posId: string
  managerUserId?: string
  account: string
  realName: string
  nickName?: string
  avatar?: string
  sex: number
  birthday?: string
  phone?: string
  email?: string
  idCardNum?: string
  jobNum?: string
  joinDate?: string
  accountType: number
  orderNo: number
  status: number
  remark?: string
  createdTime: string
  modifiedTime?: string
  roleIds?: string[]
  roleNames?: string[]
}

export interface CreateUserInput {
  orgId: string
  posId?: string
  managerUserId?: string
  account: string
  realName: string
  nickName: string
  avatar?: string
  sex?: number
  birthday?: string
  phone?: string
  email?: string
  idCardNum?: string
  jobNum?: string
  joinDate?: string
  accountType: number
  orderNo: number
  status: number
  remark?: string
  roleIds?: string[]
}

export interface UpdateUserInput {
  id: string
  version: string
  orgId: string
  posId?: string
  managerUserId?: string
  realName: string
  nickName: string
  avatar?: string
  sex?: number
  birthday?: string
  phone?: string
  email?: string
  idCardNum?: string
  jobNum?: string
  joinDate?: string
  accountType: number
  orderNo: number
  status: number
  remark?: string
  roleIds?: string[]
}

export interface ResetPasswordInput {
  newPassword: string
}

export function getUserPage(params: UserPageQuery) {
  return request.get<unknown>({ url: '/sys/user/page', params })
}

export function getUser(id: string) {
  return request.get<UserDto>({ url: `/sys/user/${id}` })
}

export function createUser(data: CreateUserInput) {
  return request.post<void>({ url: '/sys/user', data })
}

export function updateUser(id: string, data: UpdateUserInput) {
  return request.put<void>({ url: `/sys/user/${id}`, data })
}

export function deleteUser(id: string) {
  return request.delete<void>({ url: `/sys/user/${id}` })
}

export function resetPassword(id: string, newPassword: string) {
  return request.post<void>({ url: `/sys/user/${id}/reset-password`, data: { newPassword } })
}

export function changeUserStatus(id: string, status: number | string) {
  return request.post<void>({ url: `/sys/user/${id}/status`, data: { status } })
}

export interface ImportResult {
  successCount: number
  failureCount: number
  failures: { row: number; message: string }[]
}

/** 下载用户导入模板。 */
export function downloadUserImportTemplate() {
  return request.get<Blob>(
    {
      url: '/sys/user/import-template',
      responseType: 'blob'
    },
    { isTransformResponse: false }
  )
}

/** 导入用户 Excel。 */
export function importUsers(file: File) {
  const data = new FormData()
  data.append('file', file)
  return request.post<ImportResult>({
    url: '/sys/user/import',
    data,
    headers: { 'Content-Type': false as unknown as string }
  })
}

/** 按筛选条件导出用户 Excel。 */
export function exportUsers(params: Record<string, unknown>) {
  return request.get<Blob>(
    {
      url: '/sys/user/export',
      params,
      responseType: 'blob'
    },
    { isTransformResponse: false }
  )
}
