<script setup lang="ts">
  import { computed, onMounted, ref, watch } from 'vue'
  import { useRouter } from 'vue-router'
  import { NBadge, NButton, NEmpty, NPopover, NSpin, NTag } from 'naive-ui'

  import type { InboxItemDto } from '@/modules/system/api/inbox'
  import { formatDateTime } from '@/shared/components/axis-crud'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'
  import { Icon } from '@/shared/icons'
  import { inboxSourceTypeLabel } from '@/modules/personal/views/inbox/utils/display'
  import { useNotifyStore, type NotifyListFilter } from '@/store/modules/notify.store'
  import { useUserStore } from '@/store/modules/user.store'

  defineOptions({ name: 'LayoutHeaderNotify' })

  /* ============ 状态 & 引用 ============ */

  const userStore = useUserStore()
  const notifyStore = useNotifyStore()
  const router = useRouter()

  const count = computed(() => notifyStore.total)
  const popoverShow = ref(false)
  const detailShow = ref(false)
  const detailLoading = ref(false)
  const detailTitle = ref('消息详情')
  const detailStatus = ref('')
  const detailFields = ref<DetailField[]>([])

  /* ============ 生命周期 ============ */

  onMounted(() => {
    if (userStore.isLogin) void notifyStore.connect()
  })

  watch(
    () => userStore.isLogin,
    async loggedIn => {
      if (loggedIn) await notifyStore.connect()
      else await notifyStore.disconnect()
    }
  )

  /* ============ 交互操作 ============ */

  async function onShow(show: boolean) {
    popoverShow.value = show
    if (show) await notifyStore.refreshRecent()
  }

  async function switchFilter(filter: NotifyListFilter) {
    await notifyStore.setListFilter(filter)
  }

  async function onItemClick(item: InboxItemDto) {
    popoverShow.value = false

    // 有业务跳转时优先进业务页
    if (item.resolvedLink) {
      if (!item.isRead) await notifyStore.markRead(item)
      router.push(item.resolvedLink)
      return
    }

    detailLoading.value = true
    detailShow.value = true
    detailTitle.value = item.title
    detailStatus.value = item.isRead ? '已读' : '未读'

    const detail = await notifyStore.openItem(item)
    detailLoading.value = false
    if (!detail) {
      detailShow.value = false
      return
    }

    detailTitle.value = detail.title
    detailStatus.value = '已读'
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
      {
        key: 'content',
        label: '内容',
        value: detail.content || detail.summary || '-',
        fullRow: true
      }
    ]
  }

  function goInboxPage() {
    popoverShow.value = false
    router.push('/personal/inbox')
  }
</script>

<template>
  <div class="header-notify-root">
    <NPopover
      :show="popoverShow"
      trigger="click"
      placement="bottom-end"
      :width="380"
      @update:show="onShow"
    >
      <template #trigger>
        <!-- 样式需在本组件内声明：父级 scoped 的 icon-btn 无法穿透到子组件 -->
        <button type="button" class="header-notify-trigger" title="通知">
          <NBadge
            :value="count"
            :max="99"
            :show="count > 0"
            :offset="[4, -2]"
          >
            <span class="header-notify-trigger__icon">
              <Icon icon="ant-design:bell-outlined" />
            </span>
          </NBadge>
        </button>
      </template>

      <div class="header-notify">
        <div class="header-notify__header">
          <span class="header-notify__title">消息中心</span>
          <NButton
            text
            type="primary"
            size="small"
            :disabled="count <= 0"
            @click="notifyStore.markAllRead()"
          >
            全部已读
          </NButton>
        </div>

        <div class="header-notify__tabs">
          <button
            type="button"
            class="header-notify__tab"
            :class="{ 'is-active': notifyStore.listFilter === 'unread' }"
            @click="switchFilter('unread')"
          >
            未读
          </button>
          <button
            type="button"
            class="header-notify__tab"
            :class="{ 'is-active': notifyStore.listFilter === 'all' }"
            @click="switchFilter('all')"
          >
            全部
          </button>
        </div>

        <NSpin :show="notifyStore.loading">
          <NEmpty
            v-if="!notifyStore.recent.length"
            :description="notifyStore.listFilter === 'unread' ? '暂无未读消息' : '暂无消息'"
            class="header-notify__empty"
          />
          <div v-else class="header-notify__list">
            <button
              v-for="item in notifyStore.recent"
              :key="item.id"
              type="button"
              class="header-notify__item"
              :class="{ 'is-read': item.isRead }"
              @click="onItemClick(item)"
            >
              <div class="header-notify__item-top">
                <span class="header-notify__item-title">{{ item.title }}</span>
                <div class="header-notify__item-tags">
                  <NTag
                    size="tiny"
                    round
                    :bordered="false"
                    :type="item.sourceType === 1 ? 'info' : 'success'"
                  >
                    {{ inboxSourceTypeLabel(item.sourceType) }}
                  </NTag>
                  <NTag
                    v-if="notifyStore.listFilter === 'all'"
                    size="tiny"
                    round
                    :bordered="false"
                    :type="item.isRead ? 'default' : 'warning'"
                  >
                    {{ item.isRead ? '已读' : '未读' }}
                  </NTag>
                </div>
              </div>
              <p class="header-notify__item-summary">{{ item.summary || '-' }}</p>
              <span class="header-notify__item-time">{{ formatDateTime(item.createdTime) }}</span>
            </button>
          </div>
        </NSpin>

        <div class="header-notify__footer">
          <NButton text type="primary" block @click="goInboxPage">
            查看全部消息
          </NButton>
        </div>
      </div>
    </NPopover>

    <DetailDrawer
      v-model:show="detailShow"
      :title="detailTitle"
      :subtitle="detailStatus"
      icon="ant-design:mail-outlined"
      :loading="detailLoading"
      :fields="detailFields"
      :status-text="detailStatus"
      :status-type="detailStatus === '已读' ? 'success' : 'warning'"
      width="520"
    />
  </div>
</template>

<style scoped lang="scss">
  .header-notify-root {
    display: inline-flex;
  }

  // 与 layout-header__icon-btn 对齐（父级 scoped 样式不穿透）
  .header-notify-trigger {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 34px;
    height: 34px;
    padding: 0;
    border: none;
    border-radius: 8px;
    background: transparent;
    color: var(--axis-header-text-2);
    font-size: 18px;
    cursor: pointer;
    transition: all 0.15s ease;

    &:hover {
      background: var(--axis-header-action);
      color: var(--axis-primary-color);
    }

    &__icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 18px;
      height: 18px;
      line-height: 1;
    }

    :deep(.n-badge) {
      display: inline-flex;
      line-height: 1;
    }

    // 单数字如「1」字形偏窄，用 flex 居中避免视觉歪斜
    :deep(.n-badge-sup) {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      box-sizing: border-box;
      font-size: 10px;
      font-weight: 600;
      height: 16px;
      min-width: 16px;
      padding: 0 4px;
      line-height: 1;
    }
  }

  .header-notify {
    display: flex;
    flex-direction: column;
    gap: 10px;
    min-height: 120px;
    padding-bottom: 4px;

    &__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
    }

    &__title {
      color: var(--axis-text-color-1);
      font-size: 14px;
      font-weight: 600;
    }

    &__tabs {
      display: flex;
      gap: 4px;
      padding: 2px;
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-bg-color) 70%, var(--axis-card-color));
    }

    &__tab {
      flex: 1;
      height: 28px;
      border: 0;
      border-radius: calc(var(--axis-border-radius, 6px) - 1px);
      background: transparent;
      color: var(--axis-text-color-3);
      font-size: 12px;
      cursor: pointer;
      transition: all 0.15s ease;

      &.is-active {
        background: var(--axis-card-color);
        color: var(--axis-primary-color);
        font-weight: 600;
        box-shadow: 0 1px 2px color-mix(in srgb, var(--axis-text-color-1) 8%, transparent);
      }
    }

    &__empty {
      padding: 24px 0;
    }

    &__list {
      display: flex;
      flex-direction: column;
      gap: 6px;
      max-height: 360px;
      overflow: auto;
    }

    &__item {
      display: flex;
      flex-direction: column;
      gap: 4px;
      padding: 10px 12px;
      border: 0;
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-bg-color) 55%, var(--axis-card-color));
      text-align: left;
      cursor: pointer;

      &:hover {
        background: color-mix(in srgb, var(--axis-primary-color) 8%, var(--axis-card-color));
      }

      &.is-read {
        opacity: 0.78;
      }
    }

    &__item-top {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
    }

    &__item-tags {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      flex-shrink: 0;
    }

    &__item-title {
      min-width: 0;
      overflow: hidden;
      color: var(--axis-text-color-1);
      font-size: 13px;
      font-weight: 600;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__item-summary {
      margin: 0;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 1.5;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    &__item-time {
      color: var(--axis-text-color-3);
      font-size: 11px;
    }

    &__footer {
      padding: 10px 0 6px;
      margin-top: 2px;
      border-top: 1px dashed color-mix(in srgb, var(--axis-border-color) 85%, transparent);
    }
  }
</style>
