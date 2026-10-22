<script setup lang="ts">
  import { computed } from 'vue'
  import { useRouter } from 'vue-router'

  import type { UnreadCountDto } from '@/modules/system/api/inbox'
  import defaultAvatar from '@/assets/images/avatar.svg'
  import { Icon } from '@/shared/icons'
  import { useUserStore } from '@/store/modules/user.store'

  defineOptions({ name: 'HomeHero' })

  defineProps<{
    displayName: string
    greeting: string
    orgName: string
    posName: string
    todayText: string
    unread: UnreadCountDto
  }>()

  const router = useRouter()
  const userStore = useUserStore()

  const avatarSrc = computed(() => userStore.userInfo?.avatar || defaultAvatar)
  const primaryRole = computed(() => userStore.userInfo?.roles?.[0] || '普通用户')

  function goTo(path: string) {
    router.push(path)
  }
</script>

<template>
  <header class="home-hero">
    <div class="home-hero__atmosphere" aria-hidden="true">
      <span class="home-hero__orb home-hero__orb--a" />
      <span class="home-hero__orb home-hero__orb--b" />
      <span class="home-hero__beam" />
      <span class="home-hero__brand">A</span>
    </div>

    <div class="home-hero__masthead">
      <div class="home-hero__brand-lockup">
        <span class="home-hero__brand-mark">AX</span>
        <span class="home-hero__brand-copy">
          <strong>AXIS MES</strong>
          <small>云枢制造执行</small>
        </span>
      </div>
      <span class="home-hero__date">
        <Icon icon="ant-design:calendar-outlined" />
        {{ todayText }}
      </span>
    </div>

    <div class="home-hero__content">
      <div class="home-hero__main">
        <div class="home-hero__copy">
          <div class="home-hero__kicker">
            <span class="home-hero__pulse" />
            <span>工作台已就绪</span>
          </div>
          <h1 class="home-hero__title">
            <span>{{ greeting }}，</span>
            <em>{{ displayName }}</em>
          </h1>
          <p class="home-hero__lead">从重要消息到运行状态，你今天需要关注的事项都已整理在这里。</p>
        </div>

        <div class="home-hero__identity">
          <div class="home-hero__avatar">
            <img :src="avatarSrc" :alt="`${displayName}的头像`" />
            <span class="home-hero__online" aria-label="当前在线" />
          </div>
          <div class="home-hero__meta">
            <span class="home-hero__chip">
              <Icon icon="ant-design:team-outlined" />
              {{ orgName }}
            </span>
            <span class="home-hero__chip">
              <Icon icon="ant-design:idcard-outlined" />
              {{ posName }}
            </span>
            <span class="home-hero__chip">
              <Icon icon="ant-design:crown-outlined" />
              {{ primaryRole }}
            </span>
          </div>
        </div>

        <div class="home-hero__actions">
          <button
            type="button"
            class="home-hero__btn home-hero__btn--primary"
            @click="goTo('/personal/inbox')"
          >
            <Icon icon="ant-design:bell-outlined" />
            进入消息中心
          </button>
          <button type="button" class="home-hero__btn" @click="goTo('/personal/profile')">
            <Icon icon="ant-design:user-outlined" />
            个人中心
          </button>
        </div>
      </div>

      <aside class="home-hero__aside">
        <div class="home-hero__glass">
          <div class="home-hero__glass-head">
            <span class="home-hero__glass-icon">
              <Icon icon="ant-design:notification-outlined" />
            </span>
            <span>
              <b>消息待办</b>
              <small>Inbox overview</small>
            </span>
          </div>
          <div class="home-hero__glass-total">
            <strong class="home-hero__glass-value">{{ unread.total }}</strong>
            <span>条未读消息</span>
          </div>
          <div class="home-hero__glass-rows">
            <div class="home-hero__glass-row">
              <span>公告</span>
              <b>{{ unread.noticeCount }}</b>
            </div>
            <div class="home-hero__glass-row">
              <span>站内信</span>
              <b>{{ unread.messageCount }}</b>
            </div>
            <div class="home-hero__glass-row">
              <span>其他</span>
              <b>{{ unread.otherCount }}</b>
            </div>
          </div>
          <button type="button" class="home-hero__inbox-link" @click="goTo('/personal/inbox')">
            处理全部消息
            <Icon icon="ant-design:arrow-right-outlined" />
          </button>
        </div>
      </aside>
    </div>
  </header>
</template>

<style scoped lang="scss">
  @use '../styles/hero.scss';
</style>
