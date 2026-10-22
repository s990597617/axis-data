<script setup lang="ts">
  import { useRouter } from 'vue-router'

  import type { InboxItemDto } from '@/modules/system/api/inbox'
  import { formatDateTime } from '@/shared/components/axis-crud'
  import { Icon } from '@/shared/icons'
  import { formatRelativeTime } from '@/shared/utils/date'

  defineOptions({ name: 'HomeInboxFeed' })

  defineProps<{
    items: InboxItemDto[]
  }>()

  const router = useRouter()

  function formatTime(value?: string) {
    if (!value) return '-'
    return formatRelativeTime(value) || formatDateTime(value)
  }

  function sourceLabel(item: InboxItemDto) {
    if (item.sourceType === 1) return '公告'
    if (item.sourceType === 2) return '消息'
    return '通知'
  }

  function goToInbox() {
    router.push('/personal/inbox')
  }
</script>

<template>
  <section class="home-panel home-panel--inbox">
    <header class="home-panel__header">
      <div class="home-panel__header-main">
        <span class="home-panel__icon home-panel__icon--warm">
          <Icon icon="ant-design:bell-outlined" />
        </span>
        <div class="home-panel__heading">
          <span class="home-panel__eyebrow">INBOX</span>
          <div class="home-panel__title">最近消息</div>
          <span class="home-panel__subtitle">不错过每一条重要动态</span>
        </div>
      </div>
      <button type="button" class="home-panel__link" @click="goToInbox">
        全部消息
        <Icon icon="ant-design:arrow-right-outlined" />
      </button>
    </header>

    <div v-if="items.length" class="home-inbox">
      <article
        v-for="item in items"
        :key="item.id"
        class="home-inbox__item"
        :class="{ 'is-unread': !item.isRead }"
      >
        <span class="home-inbox__marker">
          <span class="home-inbox__dot" />
        </span>
        <div class="home-inbox__body">
          <div class="home-inbox__title-row">
            <strong class="home-inbox__title">{{ item.title }}</strong>
            <span class="home-inbox__tag">{{ sourceLabel(item) }}</span>
          </div>
          <p v-if="item.summary" class="home-inbox__summary">{{ item.summary }}</p>
          <div class="home-inbox__meta">
            <span>{{ item.senderName || '系统' }}</span>
            <i />
            <span>{{ formatTime(item.createdTime) }}</span>
          </div>
        </div>
      </article>
    </div>

    <div v-else class="home-panel__empty">
      <Icon icon="ant-design:inbox-outlined" style="font-size: 28px; opacity: 0.45" />
      <span>暂无消息，系统运行平静</span>
    </div>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/inbox.scss';
</style>
