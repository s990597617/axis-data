import { request } from '@/shared/http'
import type { DataHubLookupItem } from '@/modules/datahub/api/dataset'

export type { DataHubLookupItem }

/** 采集任务列表数据。 */
export interface IngestJobDto extends Record<string, unknown> {
  id: string
  datasetId?: string
  datasetName?: string
  datasetCode?: string
  name: string
  code: string
  strategy?: number
  cronExpression?: string
  batchSize?: number
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 采集任务保存数据。 */
export interface SaveIngestJobInput {
  datasetId: string
  name: string
  code: string
  strategy?: number
  cronExpression?: string
  batchSize?: number
  orderNo?: number
  status: number
  remark?: string
}

export function getIngestJobPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/datahub/ingest-job/page', params })
}

export function listIngestJobLookup() {
  return request.get<DataHubLookupItem[]>({ url: '/datahub/ingest-job/lookup' })
}

export function createIngestJob(data: SaveIngestJobInput) {
  return request.post<void>({ url: '/datahub/ingest-job', data })
}

export function updateIngestJob(id: string, data: SaveIngestJobInput) {
  return request.put<void>({ url: `/datahub/ingest-job/${id}`, data })
}

export function deleteIngestJob(id: string) {
  return request.delete<void>({ url: `/datahub/ingest-job/${id}` })
}
