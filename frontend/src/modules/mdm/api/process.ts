import { request } from '@/shared/http'

/** 工序列表数据。 */
export interface ProcessDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 工序保存数据。 */
export interface SaveProcessInput {
  name: string
  code?: string
  orderNo: number
  status: number
  remark?: string
}

/** 工序下一编码预览。 */
export interface ProcessNextCodeDto {
  code: string
}

export function getProcessPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/mdm/process/page', params })
}

export function createProcess(data: SaveProcessInput) {
  return request.post<void>({ url: '/mdm/process', data })
}

export function previewProcessCode() {
  return request.get<ProcessNextCodeDto>({ url: '/mdm/process/next-code' })
}

export function updateProcess(id: string, data: SaveProcessInput) {
  return request.put<void>({ url: `/mdm/process/${id}`, data })
}

export function deleteProcess(id: string) {
  return request.delete<void>({ url: `/mdm/process/${id}` })
}

/** 启用工序下拉。 */
export interface ProcessOptionDto {
  id: string
  code: string
  name: string
}

export function listEnabledProcesses() {
  return request.get<ProcessOptionDto[]>({ url: '/mdm/process/options' })
}
