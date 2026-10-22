import { h } from 'vue'
import { NButton, NTag } from 'naive-ui'

import type { InboxItemDto } from '@/modules/system/api/inbox'
import { inboxSourceTypeLabel } from '@/modules/personal/views/inbox/utils/display'
import { notification } from '@/shared/utils/message'
import { router } from '@/router'

/* ============ 类型 ============ */

export interface InboxIncomingNotifyOptions {
  delta: number
  item?: InboxItemDto | null
}

/* ============ 工具函数 ============ */

function goInbox(item?: InboxItemDto | null) {
  if (item?.resolvedLink) {
    void router.push(item.resolvedLink)
    return
  }
  void router.push('/personal/inbox')
}

function truncate(text: string, max = 72) {
  const value = text.trim()
  if (value.length <= max) return value
  return `${value.slice(0, max)}…`
}

/* ============ 对外暴露 ============ */

// 右下角新消息提醒卡片（Naive Notification + 自定义内容）
export function showInboxIncomingNotify(options: InboxIncomingNotifyOptions) {
  const { delta, item } = options
  const typeLabel = item ? inboxSourceTypeLabel(item.sourceType) : '消息'
  const titleText = truncate(item?.title?.trim() || '新消息', 36)
  const summaryText = truncate((item?.summary || '').trim() || '请及时查看', 64)
  const headline = delta > 1 ? `收到 ${delta} 条新消息` : '新消息提醒'

  notification.create({
    class: 'axis-inbox-notify',
    closable: true,
    duration: 8000,
    keepAliveOnHover: true,
    title: () =>
      h('div', { class: 'axis-inbox-notify__header' }, [
        h('span', { class: 'axis-inbox-notify__eyebrow' }, headline),
        h(
          NTag,
          {
            size: 'small',
            round: true,
            bordered: false,
            type: item?.sourceType === 1 ? 'info' : 'success'
          },
          { default: () => typeLabel }
        )
      ]),
    content: () =>
      h('div', { class: 'axis-inbox-notify__body' }, [
        h('div', { class: 'axis-inbox-notify__title', title: item?.title || titleText }, titleText),
        h('div', { class: 'axis-inbox-notify__summary' }, summaryText),
        h('div', { class: 'axis-inbox-notify__tip' }, '请及时查看')
      ]),
    action: () =>
      h(
        NButton,
        {
          text: true,
          type: 'primary',
          size: 'small',
          onClick: (e: MouseEvent) => {
            e.stopPropagation()
            goInbox(item)
          }
        },
        { default: () => '查看详情' }
      ),
    onClick: () => goInbox(item)
  })
}
