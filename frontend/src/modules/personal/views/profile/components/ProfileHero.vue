<script setup lang="ts">
  import { ref } from 'vue'
  import { NSpin } from 'naive-ui'

  import type { ProfileDto } from '@/modules/system/api/profile'
  import { Icon } from '@/shared/icons'
  import defaultAvatar from '@/assets/images/avatar.svg'

  defineOptions({ name: 'ProfileHero' })

  const props = defineProps<{
    accountTypeLabel: string
    avatarSrc: string
    createdTimeText: string
    displayName: string
    isAccountEnabled: boolean
    primaryRoleName: string
    profile: ProfileDto | null
    statusHint: string
    statusTagLabel: string
    statusValueLabel: string
    uploadingAvatar: boolean
  }>()

  const emit = defineEmits<{
    copyAccount: []
    avatarChange: [event: Event]
  }>()

  const avatarInputRef = ref<HTMLInputElement | null>(null)

  function openAvatarPicker() {
    if (props.uploadingAvatar) return
    avatarInputRef.value?.click()
  }
</script>

<template>
  <header class="profile-hero">
    <div class="profile-hero__left">
      <div class="profile-hero__avatar-wrap">
        <button
          type="button"
          class="profile-hero__avatar"
          title="点击更换头像"
          :disabled="uploadingAvatar"
          @click="openAvatarPicker"
        >
          <n-spin :show="uploadingAvatar" size="small">
            <img :src="avatarSrc || defaultAvatar" alt="avatar" />
          </n-spin>
        </button>
        <button
          type="button"
          class="profile-hero__camera"
          title="更换头像"
          :disabled="uploadingAvatar"
          @click="openAvatarPicker"
        >
          <Icon icon="ant-design:camera-filled" />
        </button>
        <input
          ref="avatarInputRef"
          type="file"
          accept="image/jpeg,image/png,image/gif,image/webp"
          hidden
          @change="emit('avatarChange', $event)"
        />
      </div>

      <div class="profile-hero__info">
        <div class="profile-hero__title-row">
          <h2 class="profile-hero__name">{{ displayName }}</h2>
          <span
            class="profile-hero__status-tag"
            :class="isAccountEnabled ? 'is-enabled' : 'is-disabled'"
          >
            {{ statusTagLabel }}
          </span>
        </div>

        <div class="profile-hero__rows">
          <div class="profile-hero__row">
            <span class="profile-hero__label">账号 ID：</span>
            <span class="profile-hero__value">{{ profile?.account || '-' }}</span>
            <button
              type="button"
              class="profile-hero__copy"
              title="复制账号"
              @click="emit('copyAccount')"
            >
              <Icon icon="ant-design:copy-outlined" />
            </button>
          </div>
          <div class="profile-hero__row">
            <span class="profile-hero__label">角色：</span>
            <span class="profile-hero__role">{{ primaryRoleName }}</span>
          </div>
          <div class="profile-hero__row">
            <span class="profile-hero__label">所属部门：</span>
            <span class="profile-hero__value">{{ profile?.orgName || '未分配' }}</span>
          </div>
        </div>
      </div>
    </div>

    <div class="profile-hero__stats">
      <div class="profile-stat">
        <span class="profile-stat__icon profile-stat__icon--green">
          <Icon icon="ant-design:safety-certificate-filled" />
        </span>
        <div class="profile-stat__body">
          <span class="profile-stat__label">账号状态</span>
          <strong class="profile-stat__value">{{ statusValueLabel }}</strong>
          <span class="profile-stat__hint">{{ statusHint }}</span>
        </div>
      </div>

      <div class="profile-stat">
        <span class="profile-stat__icon profile-stat__icon--blue">
          <Icon icon="ant-design:crown-filled" />
        </span>
        <div class="profile-stat__body">
          <span class="profile-stat__label">账号类型</span>
          <strong class="profile-stat__value">{{ accountTypeLabel }}</strong>
          <span class="profile-stat__hint">权限级别</span>
        </div>
      </div>

      <div class="profile-stat">
        <span class="profile-stat__icon profile-stat__icon--orange">
          <Icon icon="ant-design:idcard-filled" />
        </span>
        <div class="profile-stat__body">
          <span class="profile-stat__label">所属岗位</span>
          <strong class="profile-stat__value">{{ profile?.posName || '未分配' }}</strong>
          <span class="profile-stat__hint">当前岗位</span>
        </div>
      </div>

      <div class="profile-stat">
        <span class="profile-stat__icon profile-stat__icon--purple">
          <Icon icon="ant-design:calendar-filled" />
        </span>
        <div class="profile-stat__body">
          <span class="profile-stat__label">创建时间</span>
          <strong class="profile-stat__value">{{ createdTimeText }}</strong>
          <span class="profile-stat__hint">注册日期</span>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped lang="scss">
  @use '../styles/hero.scss';
</style>
