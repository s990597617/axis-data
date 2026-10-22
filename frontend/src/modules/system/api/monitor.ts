import { request } from '@/shared/http'

/** CPU 信息。 */
export interface ServerCpuDto {
  processorCount: number
  processUsage: number
  systemUsage?: number | null
}

/** 内存信息（字节）。 */
export interface ServerMemoryDto {
  totalBytes?: number | null
  usedBytes?: number | null
  availableBytes?: number | null
  usage?: number | null
  processWorkingSetBytes: number
  processPrivateBytes: number
  managedHeapBytes: number
}

/** 磁盘分区。 */
export interface ServerDiskDto {
  name: string
  label?: string | null
  driveFormat: string
  totalBytes: number
  usedBytes: number
  freeBytes: number
  usage: number
}

/** 运行时与进程。 */
export interface ServerRuntimeDto {
  frameworkDescription: string
  processArchitecture: string
  processId: number
  processName: string
  startTime: string
  uptimeSeconds: number
  threadCount: number
  handleCount: number
  environmentName: string
  isServerGc: boolean
  gcGen0Collections: number
  gcGen1Collections: number
  gcGen2Collections: number
}

/** 操作系统与主机。 */
export interface ServerOsDto {
  machineName: string
  osDescription: string
  osArchitecture: string
  ipAddresses: string
  contentRoot: string
}

/** 服务监控聚合。 */
export interface ServerMonitorDto {
  collectedAt: string
  cpu: ServerCpuDto
  memory: ServerMemoryDto
  disks: ServerDiskDto[]
  runtime: ServerRuntimeDto
  os: ServerOsDto
}

export function getServerMonitor() {
  return request.get<ServerMonitorDto>({ url: '/sys/monitor' })
}
