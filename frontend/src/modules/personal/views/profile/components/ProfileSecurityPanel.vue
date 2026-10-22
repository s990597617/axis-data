<script setup lang="ts">
  import { computed } from 'vue'
  import { NButton } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'ProfileSecurityPanel' })

  const props = defineProps<{
    email?: string | null
    phone?: string | null
    permissions: { changePassword: string }
  }>()

  const emit = defineEmits<{
    changePassword: []
  }>()

  // 根据资料完整度生成安全建议
  const tips = computed(() => {
    const list = [
      {
        key: 'password',
        done: true,
        text: '定期更换登录密码，避免使用过于简单的口令'
      },
      {
        key: 'phone',
        done: Boolean(props.phone),
        text: props.phone ? '已填写手机号，便于账号找回与通知' : '建议完善手机号，便于账号找回与通知'
      },
      {
        key: 'email',
        done: Boolean(props.email),
        text: props.email ? '已填写邮箱，可用于接收系统通知' : '建议完善邮箱，可用于接收系统通知'
      },
      {
        key: 'device',
        done: true,
        text: '请勿在公共设备勾选记住密码或保持登录'
      }
    ]
    return list
  })
</script>

<template>
  <section class="profile-security">
    <div class="profile-security__header">
      <h3 class="profile-security__title">账号安全</h3>
      <p class="profile-security__desc">保护账号安全，建议定期更新登录密码</p>
    </div>

    <div class="profile-security__list">
      <div class="profile-security__item">
        <span class="profile-security__icon">
          <Icon icon="ant-design:lock-outlined" />
        </span>
        <div class="profile-security__content">
          <span class="profile-security__item-title">登录密码</span>
          <span class="profile-security__item-desc">用于账号登录验证，建议定期更换</span>
        </div>
        <n-button
          size="small"
          secondary
          v-permission="permissions.changePassword"
          @click="emit('changePassword')"
        >
          修改密码
        </n-button>
      </div>

      <div class="profile-security__item">
        <span class="profile-security__icon">
          <Icon icon="ant-design:mobile-outlined" />
        </span>
        <div class="profile-security__content">
          <span class="profile-security__item-title">绑定手机</span>
          <span class="profile-security__item-desc">{{ phone || '未绑定手机号' }}</span>
        </div>
        <span class="profile-security__meta">{{ phone ? '已填写' : '未填写' }}</span>
      </div>

      <div class="profile-security__item">
        <span class="profile-security__icon">
          <Icon icon="ant-design:mail-outlined" />
        </span>
        <div class="profile-security__content">
          <span class="profile-security__item-title">绑定邮箱</span>
          <span class="profile-security__item-desc">{{ email || '未绑定邮箱' }}</span>
        </div>
        <span class="profile-security__meta">{{ email ? '已填写' : '未填写' }}</span>
      </div>
    </div>

    <div class="profile-security__tips">
      <div class="profile-security__tips-header">
        <Icon icon="ant-design:bulb-outlined" />
        <span>安全建议</span>
      </div>
      <ul class="profile-security__tips-list">
        <li v-for="tip in tips" :key="tip.key" class="profile-security__tip">
          <Icon
            :icon="tip.done ? 'ant-design:check-circle-filled' : 'ant-design:info-circle-outlined'"
            class="profile-security__tip-icon"
            :class="tip.done ? 'is-done' : 'is-todo'"
          />
          <span>{{ tip.text }}</span>
        </li>
      </ul>
    </div>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/security.scss';
</style>
