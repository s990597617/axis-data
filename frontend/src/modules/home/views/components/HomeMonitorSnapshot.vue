<script setup lang="ts">
  import { useRouter } from 'vue-router'

  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'HomeMonitorSnapshot' })

  defineProps<{
    canQueryMonitor: boolean
    monitorSummary: {
      cpuText: string
      memoryText: string
      uptimeText: string
      hostText: string
      runtimeText: string
      collectedAt: string
    } | null
    recentErrors: Array<{ title: string; time: string }>
  }>()

  const router = useRouter()

  function goTo(path: string) {
    router.push(path)
  }
</script>

<template>
  <section class="home-panel home-panel--monitor">
    <header class="home-panel__header">
      <div class="home-panel__header-main">
        <span class="home-panel__icon home-panel__icon--success">
          <Icon icon="ant-design:dashboard-outlined" />
        </span>
        <div class="home-panel__heading">
          <span class="home-panel__eyebrow">OPERATIONS</span>
          <div class="home-panel__title">运行状态</div>
          <span class="home-panel__subtitle">主机负载与近期异常一目了然</span>
        </div>
      </div>
      <div class="home-panel__header-actions">
        <span class="home-monitor__status" :class="{ 'is-warning': recentErrors.length }">
          <i />
          {{ recentErrors.length ? `${recentErrors.length} 条异常` : '运行平稳' }}
        </span>
        <button
          v-if="canQueryMonitor"
          type="button"
          class="home-panel__link"
          @click="goTo('/system/monitor')"
        >
          监控详情
          <Icon icon="ant-design:arrow-right-outlined" />
        </button>
      </div>
    </header>

    <div class="home-monitor">
      <div class="home-monitor__layout" :class="{ 'has-errors': recentErrors.length }">
        <div v-if="monitorSummary" class="home-monitor__summary">
          <div class="home-monitor__grid">
            <div class="home-monitor__stat">
              <span class="home-monitor__label">CPU 负载</span>
              <strong class="home-monitor__value">{{ monitorSummary.cpuText }}</strong>
              <span class="home-monitor__bar">
                <i
                  :style="{ width: monitorSummary.cpuText === '-' ? '0%' : monitorSummary.cpuText }"
                />
              </span>
            </div>
            <div class="home-monitor__stat">
              <span class="home-monitor__label">内存占用</span>
              <strong class="home-monitor__value">{{ monitorSummary.memoryText }}</strong>
              <span class="home-monitor__bar">
                <i
                  :style="{
                    width: monitorSummary.memoryText === '-' ? '0%' : monitorSummary.memoryText
                  }"
                />
              </span>
            </div>
            <div class="home-monitor__stat">
              <span class="home-monitor__label">运行时长</span>
              <strong class="home-monitor__value is-text">{{ monitorSummary.uptimeText }}</strong>
              <span class="home-monitor__caption">持续稳定服务</span>
            </div>
            <div class="home-monitor__stat">
              <span class="home-monitor__label">服务主机</span>
              <strong class="home-monitor__value is-text">{{ monitorSummary.hostText }}</strong>
              <span class="home-monitor__caption">当前运行节点</span>
            </div>
          </div>

          <div class="home-monitor__meta">
            <span>{{ monitorSummary.runtimeText }}</span>
            <span>最近采集 {{ monitorSummary.collectedAt }}</span>
          </div>
        </div>

        <div v-if="recentErrors.length" class="home-monitor__errors">
          <div class="home-monitor__errors-title">
            <span class="home-monitor__errors-icon">
              <Icon icon="ant-design:bug-outlined" />
            </span>
            <span>
              <b>最近异常</b>
              <small>建议及时检查</small>
            </span>
          </div>
          <button
            v-for="(item, index) in recentErrors"
            :key="`${item.title}-${index}`"
            type="button"
            class="home-monitor__error"
            @click="goTo('/system/log/error')"
          >
            <span class="home-monitor__error-index">{{ String(index + 1).padStart(2, '0') }}</span>
            <span class="home-monitor__error-content">
              <span class="home-monitor__error-title">{{ item.title }}</span>
              <span class="home-monitor__error-time">{{ item.time }}</span>
            </span>
            <Icon icon="ant-design:right-outlined" />
          </button>
        </div>
      </div>

      <div v-if="!monitorSummary && !recentErrors.length" class="home-panel__empty">
        暂无运行数据权限
      </div>
    </div>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/monitor.scss';
</style>
