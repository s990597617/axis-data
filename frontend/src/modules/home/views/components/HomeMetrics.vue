<script setup lang="ts">
  import { useRouter } from 'vue-router'

  import { Icon } from '@/shared/icons'

  import type { HomeMetric } from '../composables/useHomePage'

  defineOptions({ name: 'HomeMetrics' })

  defineProps<{
    metrics: HomeMetric[]
  }>()

  const router = useRouter()

  function handleClick(item: HomeMetric) {
    if (!item.path) return
    router.push(item.path)
  }
</script>

<template>
  <section class="home-metrics">
    <button
      v-for="(item, index) in metrics"
      :key="item.key"
      type="button"
      class="home-metric"
      :class="[`is-${item.tone}`, { 'is-clickable': !!item.path }]"
      :style="{ '--metric-delay': `${0.06 + index * 0.05}s` }"
      :aria-label="item.path ? `${item.label}：${item.value}，点击查看详情` : undefined"
      @click="handleClick(item)"
    >
      <span class="home-metric__icon">
        <Icon :icon="item.icon" />
      </span>
      <span class="home-metric__body">
        <span class="home-metric__label">{{ item.label }}</span>
        <strong class="home-metric__value">{{ item.value }}</strong>
        <span class="home-metric__hint">{{ item.hint }}</span>
      </span>
      <span v-if="item.path" class="home-metric__arrow" aria-hidden="true">
        <Icon icon="ant-design:arrow-up-outlined" />
      </span>
      <span class="home-metric__glow" aria-hidden="true" />
    </button>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/metrics.scss';
</style>
