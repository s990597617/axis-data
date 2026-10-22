import { request } from '@/shared/http'

/** 下拉选项。 */
export interface DataHubLookupItem {
  value: string
  label: string
}

/** 数据集列表数据。 */
export interface DatasetDto extends Record<string, unknown> {
  id: string
  sourceId?: string
  sourceName?: string
  sourceCode?: string
  name: string
  code: string
  objectType?: number
  objectName?: string
  customSql?: string
  primaryKeyFields?: string
  incrementalField?: string
  updateStrategy?: number
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 数据集保存数据。 */
export interface SaveDatasetInput {
  sourceId: string
  name: string
  code: string
  objectType?: number
  objectName?: string
  customSql?: string
  primaryKeyFields?: string
  incrementalField?: string
  updateStrategy?: number
  orderNo?: number
  status: number
  remark?: string
}

export function getDatasetPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/datahub/dataset/page', params })
}

export function listDatasetLookup() {
  return request.get<DataHubLookupItem[]>({ url: '/datahub/dataset/lookup' })
}

export function createDataset(data: SaveDatasetInput) {
  return request.post<void>({ url: '/datahub/dataset', data })
}

export function updateDataset(id: string, data: SaveDatasetInput) {
  return request.put<void>({ url: `/datahub/dataset/${id}`, data })
}

export function deleteDataset(id: string) {
  return request.delete<void>({ url: `/datahub/dataset/${id}` })
}
