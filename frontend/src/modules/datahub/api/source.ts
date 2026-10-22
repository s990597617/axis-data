import { request } from '@/shared/http'
import type { DataHubLookupItem } from '@/modules/datahub/api/dataset'

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

export function listSourceLookup() {
  return request.get<DataHubLookupItem[]>({ url: '/datahub/source/lookup' })
}

/** 列出数据源表/视图（元数据）。objectType: 1=表 2=视图 3=查询(空)。 */
export function listSourceObjects(sourceId: string, objectType: number) {
  return request.get<DataHubLookupItem[]>({
    url: /datahub/source//objects,
    params: { objectType }
  })
}

/** 列出数据源对象列（元数据）。 */
export function listSourceColumns(sourceId: string, objectName: string) {
  return request.get<DataHubLookupItem[]>({
    url: /datahub/source//columns,
    params: { objectName }
  })
}