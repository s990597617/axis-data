import { computed, onMounted, onUnmounted, ref } from 'vue'

import { getServerMonitor, type ServerMonitorDto } from '@/modules/system/api/monitor'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { tryRequest } from '@/shared/http'
import { formatLocalDateTime } from '@/shared/utils/date'

import {
  formatBytes,
  formatUptime,
  healthLabel,
  healthTone,
  usageColor,
  usageStatus
} from '../utils/format'

const REFRESH_MS = 5000
const HISTORY_SIZE = 24

export function useMonitorPage() {
  const loading = ref(false)
  const refreshing = ref(false)
  const autoRefresh = ref(true)
  const data = ref<ServerMonitorDto | null>(null)
  const cpuHistory = ref<number[]>([])
  const memoryHistory = ref<number[]>([])
  let timer: ReturnType<typeof setInterval> | null = null

  const permissions = systemPermissionCodes.monitor

  const collectedAtText = computed(() =>
    data.value?.collectedAt ? formatLocalDateTime(data.value.collectedAt) : '-'
  )

  const systemCpu = computed(() => data.value?.cpu.systemUsage ?? null)
  const processCpu = computed(() => data.value?.cpu.processUsage ?? 0)
  const primaryCpu = computed(() => systemCpu.value ?? processCpu.value)
  const memoryUsage = computed(() => data.value?.memory.usage ?? null)

  const overallUsage = computed(() => {
    const values = [primaryCpu.value, memoryUsage.value].filter(
      (item): item is number => item != null
    )
    if (!values.length) return null
    return Math.max(...values)
  })

  const health = computed(() => ({
    tone: healthTone(overallUsage.value),
    label: healthLabel(overallUsage.value)
  }))

  const uptimeText = computed(() => formatUptime(data.value?.runtime.uptimeSeconds))

  const ipList = computed(() =>
    (data.value?.os.ipAddresses || '')
      .split(',')
      .map(item => item.trim())
      .filter(Boolean)
  )

  const diskSummary = computed(() => {
    const disks = data.value?.disks ?? []
    if (!disks.length) return null
    const total = disks.reduce((sum, item) => sum + item.totalBytes, 0)
    const used = disks.reduce((sum, item) => sum + item.usedBytes, 0)
    const usage = total <= 0 ? 0 : (used * 100) / total
    return { total, used, usage, count: disks.length }
  })

  const overviewStats = computed(() => {
    const runtime = data.value?.runtime
    const memory = data.value?.memory
    if (!runtime || !memory) return []
    return [
      {
        key: 'uptime',
        label: '运行时长',
        value: uptimeText.value,
        icon: 'ant-design:field-time-outlined'
      },
      {
        key: 'threads',
        label: '线程',
        value: String(runtime.threadCount),
        icon: 'ant-design:apartment-outlined'
      },
      {
        key: 'handles',
        label: '句柄',
        value: String(runtime.handleCount),
        icon: 'ant-design:link-outlined'
      },
      {
        key: 'heap',
        label: '托管堆',
        value: formatBytes(memory.managedHeapBytes),
        icon: 'ant-design:cluster-outlined'
      }
    ]
  })

  const runtimeCards = computed(() => {
    const runtime = data.value?.runtime
    if (!runtime) return []
    return [
      {
        label: '运行时',
        value: runtime.frameworkDescription,
        icon: 'ant-design:code-outlined'
      },
      {
        label: '环境',
        value: runtime.environmentName || '-',
        icon: 'ant-design:cloud-server-outlined'
      },
      {
        label: '进程',
        value: `${runtime.processName} (#${runtime.processId})`,
        icon: 'ant-design:deployment-unit-outlined'
      },
      {
        label: '架构',
        value: runtime.processArchitecture,
        icon: 'ant-design:build-outlined'
      },
      {
        label: '启动时间',
        value: runtime.startTime ? formatLocalDateTime(runtime.startTime) : '-',
        icon: 'ant-design:play-circle-outlined'
      },
      {
        label: 'GC',
        value: `${runtime.isServerGc ? 'Server' : 'Workstation'} · G0 ${runtime.gcGen0Collections} / G1 ${runtime.gcGen1Collections} / G2 ${runtime.gcGen2Collections}`,
        icon: 'ant-design:clear-outlined'
      }
    ]
  })

  const hostCards = computed(() => {
    const os = data.value?.os
    const memory = data.value?.memory
    if (!os || !memory) return []
    return [
      {
        label: '主机名',
        value: os.machineName,
        icon: 'ant-design:desktop-outlined'
      },
      {
        label: '操作系统',
        value: os.osDescription,
        icon: 'ant-design:windows-outlined'
      },
      {
        label: '系统架构',
        value: os.osArchitecture,
        icon: 'ant-design:cluster-outlined'
      },
      {
        label: '系统内存',
        value: `${formatBytes(memory.usedBytes)} / ${formatBytes(memory.totalBytes)}`,
        icon: 'ant-design:database-outlined'
      },
      {
        label: '进程工作集',
        value: formatBytes(memory.processWorkingSetBytes),
        icon: 'ant-design:pie-chart-outlined'
      },
      {
        label: '内容根目录',
        value: os.contentRoot,
        icon: 'ant-design:folder-open-outlined'
      }
    ]
  })

  function pushHistory(target: typeof cpuHistory, value: number | null | undefined) {
    if (value == null || Number.isNaN(value)) return
    const next = [...target.value, Number(value.toFixed(1))]
    target.value = next.slice(-HISTORY_SIZE)
  }

  function syncHistory(snapshot: ServerMonitorDto) {
    pushHistory(cpuHistory, snapshot.cpu.systemUsage ?? snapshot.cpu.processUsage)
    pushHistory(memoryHistory, snapshot.memory.usage)
  }

  async function load(silent = false) {
    if (!silent) loading.value = true
    else refreshing.value = true
    const [result] = await tryRequest(getServerMonitor())
    if (!silent) loading.value = false
    else refreshing.value = false
    if (!result) return
    data.value = result
    syncHistory(result)
  }

  function startTimer() {
    stopTimer()
    if (!autoRefresh.value) return
    timer = setInterval(() => {
      void load(true)
    }, REFRESH_MS)
  }

  function stopTimer() {
    if (!timer) return
    clearInterval(timer)
    timer = null
  }

  function toggleAutoRefresh(value: boolean) {
    autoRefresh.value = value
    if (value) startTimer()
    else stopTimer()
  }

  onMounted(async () => {
    await load()
    await load(true)
    startTimer()
  })

  onUnmounted(() => {
    stopTimer()
  })

  return {
    autoRefresh,
    collectedAtText,
    cpuHistory,
    data,
    diskSummary,
    formatBytes,
    health,
    hostCards,
    ipList,
    load,
    loading,
    memoryHistory,
    memoryUsage,
    overviewStats,
    permissions,
    primaryCpu,
    processCpu,
    refreshing,
    runtimeCards,
    systemCpu,
    toggleAutoRefresh,
    uptimeText,
    usageColor,
    usageStatus
  }
}
