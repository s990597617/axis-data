import { request } from '@/shared/http'

/** 采集运行列表数据。 */
export interface IngestRunDto extends Record<string, unknown> {
  id: string
  jobId?: string
  jobName?: string
  jobCode?: string
  name: string
  code: string
  runStatus?: number
  startedTime?: string
  finishedTime?: string
  rowCountRead?: string | number
  rowCountChanged?: string | number
  errorMessage?: string
  watermark?: string
  orderNo?: number
  status: number
  remark?: string
  createdTime: string
}

/** 采集运行保存数据。 */
export interface SaveIngestRunInput {
  jobId: string
  name?: string
  code?: string
  runStatus?: number
  startedTime?: string
  finishedTime?: string
  rowCountRead?: number
  rowCountChanged?: number
  errorMessage?: string
  watermark?: string
  orderNo?: number
  status: number
  remark?: string
}

export function getIngestRunPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/datahub/ingest-run/page', params })
}

export function createIngestRun(data: SaveIngestRunInput) {
  return request.post<void>({ url: '/datahub/ingest-run', data })
}

export function updateIngestRun(id: string, data: SaveIngestRunInput) {
  return request.put<void>({ url: `/datahub/ingest-run/${id}`, data })
}

export function deleteIngestRun(id: string) {
  return request.delete<void>({ url: `/datahub/ingest-run/${id}` })
}
