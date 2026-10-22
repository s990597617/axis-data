import { h, ref } from 'vue'
import { useRouter } from 'vue-router'
import { NTag } from 'naive-ui'

import {
  getInboxItem,
  getInboxPage,
  markInboxAllRead,
  markInboxRead,
  type InboxItemDto
} from '@/modules/system/api/inbox'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { formatDateTime } from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { DetailField } from '@/shared/components/detail-drawer'
import { tryRequest } from '@/shared/http'
import { message } from '@/shared/utils/message'
import { useNotifyStore } from '@/store/modules/notify.store'

import { inboxReadOptions, inboxSourceTypeLabel, inboxSourceTypeOptions } from '../utils/display'

export function useInboxPage() {
  /* ============ 状态 & 引用 ============ */

  const router = useRouter()
  const tableRef = ref<{ reload: () => Promise<void> } | null>(null)
  const detailVisible = ref(false)
  const detailLoading = ref(false)
  const detailSubtitle = ref('')
  const detailFields = ref<DetailField[]>([])
  const currentDetail = ref<InboxItemDto | null>(null)
  const permissions = systemPermissionCodes.inbox
  const notifyStore = useNotifyStore()

  /* ============ 列表列 ============ */

  const columns: AxisTableColumn<InboxItemDto>[] = [
    {
      key: 'title',
      title: '标题',
      search: { field: 'keyword', placeholder: '标题或摘要' }
    },
    {
      key: 'sourceType',
      title: '类型',
      width: 100,
      dict: inboxSourceTypeOptions,
      search: true,
      dictRender: true
    },
    {
      key: 'isRead',
      title: '状态',
      width: 90,
      dict: inboxReadOptions,
      search: true,
      render: row =>
        h(
          NTag,
          {
            size: 'small',
            round: true,
            type: row.isRead ? 'success' : 'warning',
            bordered: false
          },
          { default: () => (row.isRead ? '已读' : '未读') }
        )
    },
    {
      key: 'senderName',
      title: '发件人',
      width: 120,
      render: row => row.senderName || '-'
    },
    {
      key: 'createdTime',
      title: '时间',
      width: 170,
      render: row => formatDateTime(row.createdTime)
    }
  ]

  /* ============ 交互操作 ============ */

  async function request(params: Record<string, unknown>) {
    // 前端列 key 为 sourceType，与后端 InboxPageQuery.SourceType 对齐
    const { sourceType, ...rest } = params
    return getInboxPage({
      ...rest,
      sourceType: sourceType ?? params.itemType
    })
  }

  async function showDetail(row: InboxItemDto) {
    detailVisible.value = true
    detailLoading.value = true
    detailSubtitle.value = row.title
    currentDetail.value = null

    const [detail, error] = await tryRequest(getInboxItem(row.id))
    detailLoading.value = false
    if (error || !detail) {
      detailVisible.value = false
      return
    }

    if (!row.isRead) {
      await tryRequest(markInboxRead(row.id))
      await notifyStore.refreshUnread()
      await tableRef.value?.reload()
    }

    currentDetail.value = detail
    detailSubtitle.value = '已读'
    detailFields.value = [
      {
        key: 'type',
        label: '类型',
        value: inboxSourceTypeLabel(detail.sourceType),
        type: 'tag',
        tagType: detail.sourceType === 1 ? 'info' : 'success'
      },
      {
        key: 'status',
        label: '状态',
        value: '已读',
        type: 'tag',
        tagType: 'success'
      },
      {
        key: 'time',
        label: '时间',
        value: formatDateTime(detail.createdTime)
      },
      ...(detail.senderName
        ? [{ key: 'sender', label: '发件人', value: detail.senderName } satisfies DetailField]
        : []),
      ...(detail.resolvedLink
        ? [{ key: 'link', label: '业务链接', value: detail.resolvedLink, type: 'code' as const, fullRow: true }]
        : []),
      {
        key: 'content',
        label: '内容',
        value: detail.content || detail.summary || '-',
        fullRow: true
      }
    ]
  }

  function openBizLink() {
    const link = currentDetail.value?.resolvedLink
    if (!link) return
    detailVisible.value = false
    router.push(link)
  }

  async function handleMarkAllRead() {
    const [, error] = await tryRequest(markInboxAllRead())
    if (error) return
    message.success('已全部标为已读')
    await notifyStore.refreshUnread()
    await tableRef.value?.reload()
  }

  /* ============ 对外暴露 ============ */

  return {
    columns,
    currentDetail,
    detailFields,
    detailLoading,
    detailSubtitle,
    detailVisible,
    handleMarkAllRead,
    openBizLink,
    permissions,
    request,
    showDetail,
    tableRef
  }
}
