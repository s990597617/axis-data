<script setup lang="ts">
  import { NSpin } from 'naive-ui'

  import HomeHero from './components/HomeHero.vue'
  import HomeInboxFeed from './components/HomeInboxFeed.vue'
  import HomeMetrics from './components/HomeMetrics.vue'
  import HomeMonitorSnapshot from './components/HomeMonitorSnapshot.vue'
  import HomeShortcuts from './components/HomeShortcuts.vue'
  import { useHomePage } from './composables/useHomePage'

  defineOptions({ name: 'Home' })

  const {
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
    unread
  } = useHomePage()
</script>

<template>
  <div class="home-page">
    <n-spin :show="loading">
      <div class="home-page__stack">
        <HomeHero
          :display-name="displayName"
          :greeting="greeting"
          :org-name="orgName"
          :pos-name="posName"
          :today-text="todayText"
          :unread="unread"
        />

        <section class="home-overview" aria-labelledby="home-overview-title">
          <div class="home-section-heading">
            <div>
              <span class="home-section-heading__eyebrow">TODAY'S PULSE</span>
              <h2 id="home-overview-title" class="home-section-heading__title">今日概览</h2>
            </div>
            <p class="home-section-heading__desc">聚焦需要关注的消息与系统状态</p>
          </div>

          <HomeMetrics :metrics="metrics" />
        </section>

        <div class="home-grid">
          <HomeShortcuts :shortcuts="shortcuts" />
          <HomeInboxFeed :items="inboxItems" />
        </div>

        <HomeMonitorSnapshot
          v-if="canQueryMonitor || recentErrors.length"
          :can-query-monitor="canQueryMonitor"
          :monitor-summary="monitorSummary"
          :recent-errors="recentErrors"
        />
      </div>
    </n-spin>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';

  .home-page__stack {
    display: flex;
    flex-direction: column;
    gap: 18px;
  }
</style>
