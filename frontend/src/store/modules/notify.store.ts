import * as signalR from '@microsoft/signalr'
import { defineStore } from 'pinia'
import { computed, ref } from 'vue'

import {
  getInboxItem,
  getInboxPage,
  getUnreadCount,
  markInboxAllRead,
  markInboxRead,
  type InboxItemDto,
  type UnreadCountDto
} from '@/modules/system/api/inbox'
import { router } from '@/router'
import { tryRequest } from '@/shared/http'
import { showInboxIncomingNotify } from '@/shared/utils/inbox-notify'
import { message } from '@/shared/utils/message'
import { useUserStore } from '@/store/modules/user.store'

export type NotifyListFilter = 'unread' | 'all'

type ForceLogoutPayload = {
  reason?: string
  sessionId?: string
}

// Hub 不走 /api 前缀，与后端 MapHub('/hubs/notify') 对齐
function resolveHubUrl() {
  const useProxy = import.meta.env.VITE_USE_PROXY === 'true'
  const base = useProxy
    ? ''
    : ((import.meta.env.VITE_API_BASE_URL as string | undefined) || '').replace(/\/$/, '')
  return `${base}/hubs/notify`
}

export const useNotifyStore = defineStore('notify', () => {
  /* ============ 状态 ============ */

  const unread = ref<UnreadCountDto>({ total: 0, noticeCount: 0, messageCount: 0, otherCount: 0 })
  const recent = ref<InboxItemDto[]>([])
  const loading = ref(false)
  const listFilter = ref<NotifyListFilter>('unread')
  let connection: signalR.HubConnection | null = null

  const total = computed(() => unread.value.total)

  /* ============ 交互操作 ============ */

  async function refreshUnread() {
    const [data, error] = await tryRequest(getUnreadCount())
    if (error || !data) return
    unread.value = data
  }

  async function refreshRecent() {
    loading.value = true
    const params: Record<string, unknown> = { page: 1, pageSize: 12 }
    if (listFilter.value === 'unread') params.isRead = false

    const [data, error] = await tryRequest(getInboxPage(params))
    loading.value = false
    if (error) return
    const items = (data as { items?: InboxItemDto[] } | null)?.items
    recent.value = items ?? []
  }

  async function setListFilter(filter: NotifyListFilter) {
    if (listFilter.value === filter) return
    listFilter.value = filter
    await refreshRecent()
  }

  async function markRead(item: InboxItemDto) {
    if (item.isRead) return
    const [, error] = await tryRequest(markInboxRead(item.id))
    if (error) return
    await Promise.all([refreshUnread(), refreshRecent()])
  }

  async function markAllRead() {
    const [, error] = await tryRequest(markInboxAllRead())
    if (error) return
    await Promise.all([refreshUnread(), refreshRecent()])
  }

  // 打开详情：拉全文；未读则顺带标已读
  async function openItem(item: InboxItemDto) {
    const [detail, error] = await tryRequest(getInboxItem(item.id))
    if (error || !detail) return null
    if (!item.isRead) await markRead(item)
    return detail
  }

  /* ============ 新消息弹窗 ============ */

  // 未读数增加时右下角提醒；标已读导致减少不弹
  async function showIncomingToast(delta: number) {
    const [data] = await tryRequest(getInboxPage({ page: 1, pageSize: 1, isRead: false }))
    const item = (data as { items?: InboxItemDto[] } | null)?.items?.[0]
    showInboxIncomingNotify({ delta, item })
  }

  /* ============ SignalR ============ */

  async function connect() {
    const userStore = useUserStore()
    if (!userStore.token || connection) return

    connection = new signalR.HubConnectionBuilder()
      .withUrl(resolveHubUrl(), {
        accessTokenFactory: () => userStore.token
      })
      .withAutomaticReconnect()
      .build()

    connection.on('UnreadChanged', (payload: UnreadCountDto) => {
      const previousTotal = unread.value.total
      const nextTotal = Number(payload.total) || 0

      unread.value = {
        total: nextTotal,
        noticeCount: Number(payload.noticeCount) || 0,
        messageCount: Number(payload.messageCount) || 0,
        otherCount: Number(payload.otherCount) || 0
      }

      if (nextTotal > previousTotal) {
        void showIncomingToast(nextTotal - previousTotal)
      }

      void refreshRecent()
    })

    connection.on('ForceLogout', (payload: ForceLogoutPayload) => {
      message.warning(payload?.reason || '您已被强制下线，请重新登录')
      userStore.clearSession(router)
      void router.replace({ name: 'Login' })
      void disconnect()
    })

    try {
      await connection.start()
      await refreshUnread()
    } catch {
      await refreshUnread()
    }
  }

  async function disconnect() {
    if (!connection) return
    const current = connection
    connection = null
    try {
      await current.stop()
    } catch {
      // ignore
    }
  }

  /* ============ 对外暴露 ============ */

  return {
    unread,
    recent,
    loading,
    listFilter,
    total,
    refreshUnread,
    refreshRecent,
    setListFilter,
    markRead,
    markAllRead,
    openItem,
    connect,
    disconnect
  }
})
