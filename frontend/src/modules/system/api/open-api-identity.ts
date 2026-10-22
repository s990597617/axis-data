import { request } from '@/shared/http'

/** 开放接口身份列表项。 */
export interface OpenApiIdentityDto extends Record<string, unknown> {
  id: string
  accessKey: string
  tenantId?: string
  tenantName?: string
  userId: string
  userAccount?: string
  userRealName?: string
  status: number
  remark?: string
  createdTime: string
}

/** 保存开放接口身份。 */
export interface SaveOpenApiIdentityInput {
  accessKey: string
  accessSecret?: string
  tenantId?: string | number | null
  userId: string | number
  status: number
  remark?: string
}

/** 创建后回传明文密钥。 */
export interface CreateOpenApiIdentityResult {
  id: string
  accessKey: string
  accessSecret: string
}

/** 重置密钥结果。 */
export interface ResetOpenApiSecretResult {
  accessKey: string
  accessSecret: string
}

export function getOpenApiIdentityPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/open-api-identity/page', params })
}

export function createOpenApiIdentity(data: SaveOpenApiIdentityInput) {
  return request.post<CreateOpenApiIdentityResult>({ url: '/sys/open-api-identity', data })
}

export function updateOpenApiIdentity(id: string, data: SaveOpenApiIdentityInput) {
  return request.put<void>({ url: `/sys/open-api-identity/${id}`, data })
}

export function deleteOpenApiIdentity(id: string) {
  return request.delete<void>({ url: `/sys/open-api-identity/${id}` })
}

export function resetOpenApiSecret(id: string) {
  return request.post<ResetOpenApiSecretResult>({
    url: `/sys/open-api-identity/${id}/reset-secret`
  })
}
