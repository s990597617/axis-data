import { request } from '@/shared/http'

/** 工艺路线工序行。 */
export interface RoutingStepDto {
  id?: string
  seq: number
  processId: string
  processCode?: string
  processName?: string
  standardMinutes: number
  isReportPoint: boolean
  remark?: string
}

/** 工艺路线列表/详情。 */
export interface RoutingDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  stepCount: number
  orderNo: number
  status: number
  remark?: string
  steps?: RoutingStepDto[]
  createdTime: string
}

/** 保存工艺路线。 */
export interface SaveRoutingInput {
  name: string
  code?: string
  steps: SaveRoutingStepInput[]
  orderNo: number
  status: number
  remark?: string
}

/** 保存时的工序行。 */
export interface SaveRoutingStepInput {
  seq: number
  processId: string
  standardMinutes: number
  isReportPoint: boolean
  remark?: string
}

/** 工艺路线下一编码预览。 */
export interface RoutingNextCodeDto {
  code: string
}

export function getRoutingPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/mdm/routing/page', params })
}

export function getRouting(id: string) {
  return request.get<RoutingDto>({ url: `/mdm/routing/${id}` })
}

export function createRouting(data: SaveRoutingInput) {
  return request.post<void>({ url: '/mdm/routing', data })
}

export function previewRoutingCode() {
  return request.get<RoutingNextCodeDto>({ url: '/mdm/routing/next-code' })
}

export function updateRouting(id: string, data: SaveRoutingInput) {
  return request.put<void>({ url: `/mdm/routing/${id}`, data })
}

export function deleteRouting(id: string) {
  return request.delete<void>({ url: `/mdm/routing/${id}` })
}
