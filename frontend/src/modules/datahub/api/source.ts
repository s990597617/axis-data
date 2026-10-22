import { request } from '@/shared/http'

/** 数据源列表数据。 */
export interface SourceDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  sourceType?: number
  secretRef?: string
  isReadOnly?: boolean
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 数据源保存数据。 */
export interface SaveSourceInput {
  name: string
  code: string
  sourceType?: number
  secretRef?: string
  isReadOnly?: boolean
  orderNo?: number
  status: number
  remark?: string
}

export function getSourcePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/datahub/source/page', params })
}

export function createSource(data: SaveSourceInput) {
  return request.post<void>({ url: '/datahub/source', data })
}

export function updateSource(id: string, data: SaveSourceInput) {
  return request.put<void>({ url: `/datahub/source/${id}`, data })
}

export function deleteSource(id: string) {
  return request.delete<void>({ url: `/datahub/source/${id}` })
}

/** 密钥引用可选项。 */
export interface SecretRefOptionItem {
  label: string
  value: string
  configured: boolean
}

export interface SecretRefOptions {
  configKeys: SecretRefOptionItem[]
  envKeys: SecretRefOptionItem[]
}

/** 从后端配置/环境变量动态加载可选密钥键（不含明文）。 */
export function listSecretRefOptions() {
  return request.get<SecretRefOptions>({ url: '/datahub/source/secret-ref-options' })
}
