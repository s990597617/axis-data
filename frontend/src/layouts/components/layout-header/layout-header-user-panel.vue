<script setup lang="ts">
  import { ref } from 'vue'
  import { useRouter } from 'vue-router'

  import ChangePasswordDialog from '@/modules/personal/views/profile/components/ChangePasswordDialog.vue'
  import { Icon } from '@/shared/icons'

  defineProps<{
    account?: string
    avatarSrc: string
    nickname: string
    role: string
  }>()

  const emit = defineEmits<{
    logout: []
  }>()

  const router = useRouter()
  const showUserPanel = ref(false)
  const changePasswordDialogRef = ref<InstanceType<typeof ChangePasswordDialog> | null>(null)

  function closePanel() {
    showUserPanel.value = false
  }

  function goProfile() {
    closePanel()
    router.push('/personal/profile')
  }

  function openChangePassword() {
    closePanel()
    changePasswordDialogRef.value?.open()
  }

  function handleLogout() {
    closePanel()
    emit('logout')
  }
</script>

<template>
  <n-popover
    trigger="hover"
    placement="bottom-end"
    :show="showUserPanel"
    :show-arrow="false"
    raw
    style="padding: 0"
    @update:show="(value: boolean) => (showUserPanel = value)"
  >
    <template #trigger>
      <div class="layout-header-user">
        <n-avatar round :size="30" :src="avatarSrc" />
        <span class="layout-header-user__nickname">{{ nickname }}</span>
        <Icon icon="ant-design:down-outlined" class="layout-header-user__chevron" />
      </div>
    </template>

    <div class="user-panel">
      <div class="user-panel__header">
        <div class="user-panel__profile">
          <div class="user-panel__avatar">
            <img :src="avatarSrc" alt="avatar" />
            <span class="user-panel__status" />
          </div>

          <div class="user-panel__info">
            <div class="user-panel__name">
              <span>{{ nickname }}</span>
            </div>
            <div v-if="account" class="user-panel__account">{{ account }}</div>
          </div>

          <div class="user-panel__role" :title="role">
            <span>{{ role }}</span>
          </div>
        </div>
      </div>

      <div class="user-panel__menu">
        <div class="user-panel__item" @click="goProfile">
          <Icon icon="ant-design:user-outlined" class="user-panel__item-icon" :width="16" :height="16" />
          <div class="user-panel__item-content">
            <span class="user-panel__item-title">个人中心</span>
            <span class="user-panel__item-desc">查看与编辑个人资料</span>
          </div>
          <Icon icon="ant-design:right-outlined" class="user-panel__item-arrow" :width="12" :height="12" />
        </div>

        <div class="user-panel__item" @click="openChangePassword">
          <Icon icon="ant-design:lock-outlined" class="user-panel__item-icon" :width="16" :height="16" />
          <div class="user-panel__item-content">
            <span class="user-panel__item-title">修改密码</span>
            <span class="user-panel__item-desc">更新账户登录密码</span>
          </div>
          <Icon icon="ant-design:right-outlined" class="user-panel__item-arrow" :width="12" :height="12" />
        </div>
      </div>

      <div class="user-panel__footer">
        <button type="button" class="user-panel__logout" @click="handleLogout">
          <Icon icon="ant-design:logout-outlined" />
          <span>退出登录</span>
        </button>
      </div>
    </div>
  </n-popover>

  <ChangePasswordDialog ref="changePasswordDialogRef" />
</template>

<style scoped lang="scss">
  .layout-header-user {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    height: 34px;
    padding: 3px 8px 3px 4px;
    border: 1px solid transparent;
    border-radius: 10px;
    cursor: pointer;
    transition:
      background-color 0.2s ease,
      border-color 0.2s ease,
      color 0.2s ease;
    user-select: none;

    &:hover {
      background: var(--axis-header-action);
      border-color: color-mix(in srgb, var(--axis-border-color) 72%, transparent);

      .layout-header-user__chevron {
        color: var(--axis-text-color-2);
      }
    }

    &__nickname {
      max-width: 108px;
      overflow: hidden;
      color: var(--axis-header-text);
      font-size: 13px;
      font-weight: 500;
      white-space: nowrap;
      text-overflow: ellipsis;
    }

    &__chevron {
      color: var(--axis-text-color-3);
      font-size: 10px;
      transition: color 0.2s ease;
    }
  }

  :global(.user-panel) {
    width: 284px;
    overflow: hidden;
    border: 1px solid color-mix(in srgb, var(--axis-border-color) 78%, transparent);
    border-radius: 14px;
    background: var(--axis-card-color);
    box-shadow:
      0 10px 30px rgba(15, 23, 42, 0.08),
      0 2px 8px rgba(15, 23, 42, 0.04);
  }

  :global(.user-panel__header) {
    position: relative;
    padding: 14px;
    background:
      linear-gradient(
        180deg,
        color-mix(in srgb, var(--axis-primary-color) 4%, transparent),
        transparent 72%
      ),
      var(--axis-card-color);
  }

  :global(.user-panel__header::after) {
    position: absolute;
    right: 14px;
    bottom: 0;
    left: 14px;
    height: 1px;
    background: color-mix(in srgb, var(--axis-border-color) 72%, transparent);
    content: '';
  }

  :global(.user-panel__profile) {
    display: flex;
    align-items: center;
    gap: 10px;
    min-width: 0;
  }

  :global(.user-panel__avatar) {
    position: relative;
    display: flex;
    width: 42px;
    height: 42px;
    flex-shrink: 0;
    align-items: center;
    justify-content: center;
    overflow: hidden;
    border: 1px solid rgba(255, 255, 255, 0.72);
    border-radius: 12px;
    background: var(--axis-card-color);
    box-shadow: 0 4px 12px rgba(15, 23, 42, 0.06);
  }

  :global(.user-panel__avatar img) {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  :global(.user-panel__status) {
    position: absolute;
    right: 3px;
    bottom: 3px;
    width: 8px;
    height: 8px;
    border: 1.5px solid var(--axis-card-color);
    border-radius: 50%;
    background: #18a058;
  }

  :global(.user-panel__info) {
    min-width: 0;
    flex: 1;
  }

  :global(.user-panel__name) {
    display: flex;
    min-width: 0;
    align-items: center;
    color: var(--axis-text-color-1);
    font-size: 14px;
    font-weight: 600;
    line-height: 1.3;
  }

  :global(.user-panel__name span) {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  :global(.user-panel__account) {
    margin-top: 3px;
    overflow: hidden;
    color: var(--axis-text-color-3);
    font-size: 11px;
    line-height: 1.35;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  :global(.user-panel__role) {
    display: inline-flex;
    flex-shrink: 0;
    align-items: center;
    justify-content: center;
    max-width: 88px;
    height: 22px;
    padding: 0 8px;
    overflow: hidden;
    border: 1px solid color-mix(in srgb, #18a058 16%, transparent);
    border-radius: 999px;
    background: color-mix(in srgb, #18a058 7%, transparent);
    color: #4b6b57;
    font-size: 11px;
    font-weight: 500;
    line-height: 1;
    white-space: nowrap;
  }

  :global(.user-panel__role span) {
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  :global(.user-panel__menu) {
    padding: 10px;
  }

  :global(.user-panel__item) {
    display: flex;
    align-items: center;
    gap: 10px;
    min-height: 52px;
    padding: 10px 12px;
    border-radius: 12px;
    color: var(--axis-text-color-1);
    cursor: pointer;
    transition: background-color 0.18s ease;
  }

  :global(.user-panel__item + .user-panel__item) {
    margin-top: 6px;
  }

  :global(.user-panel__item:hover) {
    background: color-mix(in srgb, var(--axis-action-color) 62%, transparent);
  }

  :global(.user-panel__item-icon) {
    flex-shrink: 0;
    color: var(--axis-text-color-2);
  }

  :global(.user-panel__item-content) {
    display: flex;
    min-width: 0;
    flex: 1;
    flex-direction: column;
    justify-content: center;
    gap: 3px;
  }

  :global(.user-panel__item-title) {
    color: var(--axis-text-color-1);
    font-size: 13px;
    font-weight: 400;
    line-height: 1.3;
  }

  :global(.user-panel__item-desc) {
    overflow: hidden;
    color: var(--axis-text-color-3);
    font-size: 11px;
    font-weight: 400;
    line-height: 1.35;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  :global(.user-panel__item-arrow) {
    flex-shrink: 0;
    color: var(--axis-text-color-3);
    opacity: 0.55;
  }

  :global(.user-panel__footer) {
    padding: 4px 10px 10px;
  }

  :global(.user-panel__logout) {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
    width: 100%;
    height: 38px;
    border: 1px solid color-mix(in srgb, var(--axis-border-color) 78%, transparent);
    border-radius: 12px;
    background: color-mix(in srgb, var(--axis-action-color) 82%, transparent);
    color: var(--axis-text-color-2);
    font-size: 13px;
    font-weight: 500;
    cursor: pointer;
    transition:
      background-color 0.18s ease,
      border-color 0.18s ease,
      color 0.18s ease;
  }

  :global(.user-panel__logout:hover) {
    border-color: color-mix(in srgb, var(--axis-border-color) 92%, transparent);
    background: color-mix(in srgb, var(--axis-action-color) 100%, transparent);
    color: var(--axis-text-color-1);
  }
</style>
