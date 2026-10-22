import { request } from '@/shared/http'

/** 定时任务列表项。 */
export interface JobDto extends Record<string, unknown> {
  id: string
  jobCode: string
  name: string
  groupName?: string
  cron: string
  concurrent: boolean
  params?: string
  isBuiltin: boolean
  orderNo: number
  status: number
  remark?: string
  createdTime: string
  nextFireTime?: string
  executorRegistered: boolean
}

/** 已注册任务执行器。 */
export interface JobExecutorDto {
  code: string
  name: string
  groupName?: string
  cron?: string
  concurrent: boolean
  defaultParams?: string
  builtin: boolean
  orderNo: number
  enabled: boolean
  remark?: string
  hasMetadata: boolean
}

/** 保存定时任务。 */
export interface SaveJobInput {
  id?: string
  jobCode: string
  name: string
  groupName?: string
  cron: string
  concurrent: boolean
  params?: string
  orderNo: number
  status: number
  remark?: string
}

/** 任务执行日志。 */
export interface JobLogDto extends Record<string, unknown> {
  id: string
  jobId: string
  jobCode: string
  jobName?: string
  status: number
  triggerType: number
  startedTime: string
  endedTime?: string
  durationMs?: number
  message?: string
  exception?: string
}

export function getJobPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/job/page', params })
}

export function getJobExecutors() {
  return request.get<JobExecutorDto[]>({ url: '/sys/job/executors' })
}

export function createJob(data: SaveJobInput) {
  return request.post<void>({ url: '/sys/job', data })
}

export function updateJob(id: string, data: SaveJobInput) {
  return request.put<void>({ url: `/sys/job/${id}`, data })
}

export function deleteJob(id: string) {
  return request.delete<void>({ url: `/sys/job/${id}` })
}

export function changeJobStatus(id: string, enabled: boolean) {
  return request.put<void>({ url: `/sys/job/${id}/status`, data: { enabled } })
}

export function runJob(id: string) {
  return request.post<void>({ url: `/sys/job/${id}/run` })
}

export function getJobLogPage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/job/log/page', params })
}
