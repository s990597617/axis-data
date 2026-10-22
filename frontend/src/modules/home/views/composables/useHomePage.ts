import { computed, onMounted, ref } from 'vue'

import { getErrorLogPage } from '@/modules/system/api/log'
import { getInboxPage, getUnreadCount, type InboxItemDto, type UnreadCountDto } from '@/modules/system/api/inbox'
import { getJobPage } from '@/modules/system/api/job'
import { getServerMonitor, type ServerMonitorDto } from '@/modules/system/api/monitor'
import { getOnlineSessionPage } from '@/modules/system/api/online'
import { getUserPage } from '@/modules/system/api/user'
import { hasAnyPermission } from '@/shared/access/access.service'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { formatDateTime } from '@/shared/components/axis-crud'
import { tryRequest } from '@/shared/http'
import { formatLocalDate, formatRelativeTime } from '@/shared/utils/date'
import { useUserStore } from '@/store/modules/user.store'

import { formatBytes, formatUptime } from '@/modules/system/views/monitor/utils/format'

export interface HomeMetric {
  key: string
  label: string
  value: string
  hint: string
  icon: string
  tone: 'primary' | 'success' | 'warning' | 'danger' | 'info'
  path?: string
}

export interface HomeShortcut {
  label: string
  desc: string
  icon: string
  path: string
}

const shortcutCandidates: Array<HomeShortcut & { permissions: string[] }> = [
  {
    label: '个人中心',
    desc: '查看与编辑个人资料',
    icon: 'ant-design:idcard-outlined',
    path: '/personal/profile',
    permissions: [systemPermissionCodes.profile.query]
  },
  {
    label: '我的消息',
    desc: '查看通知与站内信',
    icon: 'ant-design:bell-outlined',
    path: '/personal/inbox',
    permissions: [systemPermissionCodes.inbox.query]
  },
  {
    label: '用户管理',
    desc: '维护系统用户账号',
    icon: 'ant-design:user-outlined',
    path: '/base/user',
    permissions: [systemPermissionCodes.user.query]
  },
  {
    label: '角色管理',
    desc: '配置角色与权限',
    icon: 'ant-design:team-outlined',
    path: '/base/role',
    permissions: [systemPermissionCodes.role.query]
  },
  {
    label: '通知公告',
    desc: '发布与管理系统公告',
    icon: 'ant-design:notification-outlined',
    path: '/base/notice',
    permissions: [systemPermissionCodes.notice.query]
  },
  {
    label: '定时任务',
    desc: '查看任务调度状态',
    icon: 'ant-design:schedule-outlined',
    path: '/system/job',
    permissions: [systemPermissionCodes.job.query]
  },
  {
    label: '在线用户',
    desc: '查看当前在线会话',
    icon: 'ant-design:wifi-outlined',
    path: '/system/online',
    permissions: [systemPermissionCodes.online.query]
  },
  {
    label: '服务监控',
    desc: '查看 CPU 与内存负载',
    icon: 'ant-design:dashboard-outlined',
    path: '/system/monitor',
    permissions: [systemPermissionCodes.monitor.query]
  },
  {
    label: '文件管理',
    desc: '浏览与上传系统文件',
    icon: 'ant-design:folder-open-outlined',
    path: '/system/file',
    permissions: [systemPermissionCodes.file.query]
  },
  {
    label: '异常日志',
    desc: '排查系统运行异常',
    icon: 'ant-design:bug-outlined',
    path: '/system/log/error',
    permissions: [systemPermissionCodes.logError.query]
  }
]

function readPageTotal(data: unknown) {
  if (!data || typeof data !== 'object') return 0
  const source = data as Record<string, unknown>
  return Number(source.total ?? source.Total ?? source.itemCount ?? 0) || 0
}

function readPageItems<T>(data: unknown) {
  if (Array.isArray(data)) return data as T[]
  if (!data || typeof data !== 'object') return []
  const source = data as Record<string, unknown>
  const items = source.items ?? source.Items ?? source.records
  return Array.isArray(items) ? (items as T[]) : []
}

export function useHomePage() {
  const userStore = useUserStore()

  /* ============ 状态 ============ */

  const loading = ref(false)
  const unread = ref<UnreadCountDto>({ total: 0, noticeCount: 0, messageCount: 0, otherCount: 0 })
  const inboxItems = ref<InboxItemDto[]>([])
  const monitor = ref<ServerMonitorDto | null>(null)
  const onlineCount = ref<number | null>(null)
  const userCount = ref<number | null>(null)
  const enabledJobCount = ref<number | null>(null)
  const recentErrors = ref<Array<{ title: string; time: string }>>([])

  const accessSubject = computed(() => ({
    roles: userStore.userInfo?.roles ?? [],
    permissions: userStore.userInfo?.permissions ?? []
  }))

  const greeting = computed(() => {
    const hour = new Date().getHours()
    if (hour < 6) return '夜深了'
    if (hour < 9) return '早上好'
    if (hour < 12) return '上午好'
    if (hour < 14) return '中午好'
    if (hour < 18) return '下午好'
    return '晚上好'
  })

  const displayName = computed(
    () => userStore.userInfo?.nickname ?? userStore.userInfo?.username ?? '用户'
  )

  const orgName = computed(() => userStore.userInfo?.orgName || '未分配组织')
  const posName = computed(() => userStore.userInfo?.posName || '未分配岗位')
  const todayText = computed(() => formatLocalDate(new Date()) || '-')

  const canQueryMonitor = computed(() =>
    hasAnyPermission(accessSubject.value, [systemPermissionCodes.monitor.query])
  )

  const shortcuts = computed(() =>
    shortcutCandidates.filter(item => hasAnyPermission(accessSubject.value, item.permissions))
  )

  const metrics = computed<HomeMetric[]>(() => {
    const cards: HomeMetric[] = [
      {
        key: 'unread',
        label: '未读消息',
        value: String(unread.value.total),
        hint: unread.value.total
          ? `公告 ${unread.value.noticeCount} · 站内 ${unread.value.messageCount}`
          : '暂无未读消息',
        icon: 'ant-design:mail-outlined',
        tone: unread.value.total ? 'warning' : 'success',
        path: '/personal/inbox'
      }
    ]

    if (onlineCount.value != null) {
      cards.push({
        key: 'online',
        label: '在线用户',
        value: String(onlineCount.value),
        hint: '当前在线会话数',
        icon: 'ant-design:wifi-outlined',
        tone: 'success',
        path: '/system/online'
      })
    }

    if (userCount.value != null) {
      cards.push({
        key: 'users',
        label: '系统用户',
        value: String(userCount.value),
        hint: '已注册账号总数',
        icon: 'ant-design:user-outlined',
        tone: 'primary',
        path: '/base/user'
      })
    }

    if (enabledJobCount.value != null) {
      cards.push({
        key: 'jobs',
        label: '启用任务',
        value: String(enabledJobCount.value),
        hint: '当前启用的定时任务',
        icon: 'ant-design:schedule-outlined',
        tone: 'info',
        path: '/system/job'
      })
    }

    if (monitor.value) {
      const cpu = monitor.value.cpu.systemUsage ?? monitor.value.cpu.processUsage
      const memory = monitor.value.memory.usage
      cards.push({
        key: 'cpu',
        label: 'CPU 负载',
        value: cpu == null ? '-' : `${cpu.toFixed(1)}%`,
        hint: '系统整体 CPU 使用率',
        icon: 'ant-design:thunderbolt-outlined',
        tone: cpu != null && cpu >= 85 ? 'danger' : cpu != null && cpu >= 70 ? 'warning' : 'success',
        path: '/system/monitor'
      })
      cards.push({
        key: 'memory',
        label: '内存占用',
        value: memory == null ? '-' : `${memory.toFixed(1)}%`,
        hint: monitor.value.memory.usedBytes
          ? `${formatBytes(monitor.value.memory.usedBytes)} / ${formatBytes(monitor.value.memory.totalBytes)}`
          : '系统内存使用率',
        icon: 'ant-design:database-outlined',
        tone: memory != null && memory >= 85 ? 'danger' : memory != null && memory >= 70 ? 'warning' : 'success',
        path: '/system/monitor'
      })
    }

    return cards.slice(0, 4)
  })

  const monitorSummary = computed(() => {
    if (!monitor.value) return null
    const cpu = monitor.value.cpu.systemUsage ?? monitor.value.cpu.processUsage
    const memory = monitor.value.memory.usage
    return {
      cpuText: cpu == null ? '-' : `${cpu.toFixed(1)}%`,
      memoryText: memory == null ? '-' : `${memory.toFixed(1)}%`,
      uptimeText: formatUptime(monitor.value.runtime.uptimeSeconds),
      hostText: monitor.value.os.machineName,
      runtimeText: monitor.value.runtime.frameworkDescription,
      collectedAt: formatDateTime(monitor.value.collectedAt)
    }
  })

  /* ============ 数据加载 ============ */

  async function loadUnread() {
    const [data, error] = await tryRequest(getUnreadCount())
    if (error || !data) return
    unread.value = data
  }

  async function loadInbox() {
    const [data, error] = await tryRequest(getInboxPage({ page: 1, pageSize: 6 }))
    if (error) return
    inboxItems.value = readPageItems<InboxItemDto>(data)
  }

  async function loadOnlineCount() {
    if (!hasAnyPermission(accessSubject.value, [systemPermissionCodes.online.query])) return
    const [data, error] = await tryRequest(getOnlineSessionPage({ page: 1, pageSize: 1 }))
    if (error) return
    onlineCount.value = readPageTotal(data)
  }

  async function loadUserCount() {
    if (!hasAnyPermission(accessSubject.value, [systemPermissionCodes.user.query])) return
    const [data, error] = await tryRequest(getUserPage({ page: 1, pageSize: 1 }))
    if (error) return
    userCount.value = readPageTotal(data)
  }

  async function loadJobCount() {
    if (!hasAnyPermission(accessSubject.value, [systemPermissionCodes.job.query])) return
    const [data, error] = await tryRequest(getJobPage({ page: 1, pageSize: 1, status: 1 }))
    if (error) return
    enabledJobCount.value = readPageTotal(data)
  }

  async function loadMonitor() {
    if (!hasAnyPermission(accessSubject.value, [systemPermissionCodes.monitor.query])) return
    const [data, error] = await tryRequest(getServerMonitor())
    if (error) return
    monitor.value = data ?? null
  }

  async function loadRecentErrors() {
    if (!hasAnyPermission(accessSubject.value, [systemPermissionCodes.logError.query])) return
    const [data, error] = await tryRequest(getErrorLogPage({ page: 1, pageSize: 3 }))
    if (error) return
    recentErrors.value = readPageItems<Record<string, unknown>>(data).map(item => ({
      title: String(item.displayTitle ?? item.fullName ?? item.actionName ?? '系统异常'),
      time: formatRelativeTime(item.createdTime as string) || formatDateTime(item.createdTime)
    }))
  }

  async function loadDashboard() {
    loading.value = true
    await Promise.all([
      loadUnread(),
      loadInbox(),
      loadOnlineCount(),
      loadUserCount(),
      loadJobCount(),
      loadMonitor(),
      loadRecentErrors()
    ])
    loading.value = false
  }

  onMounted(() => {
    void loadDashboard()
  })

  /* ============ 对外暴露 ============ */

  return {
    canQueryMonitor,
    displayName,
    greeting,
    inboxItems,
    loading,
    metrics,
    monitorSummary,
    orgName,
    posName,
    recentErrors,
    shortcuts,
    todayText,
    unread,
    loadDashboard
  }
}
