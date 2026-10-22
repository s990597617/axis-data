<script setup lang="ts">
  import { computed } from 'vue'

  defineOptions({ name: 'MonitorSparkline' })

  /* ============ 属性 ============ */

  const props = withDefaults(
    defineProps<{
      values: number[]
      color?: string
      height?: number
    }>(),
    {
      color: 'var(--axis-primary-color)',
      height: 48
    }
  )

  /* ============ 视图模型 ============ */

  const path = computed(() => {
    const points = props.values.filter(v => Number.isFinite(v))
    if (points.length < 2) return ''
    const max = Math.max(...points, 1)
    const min = Math.min(...points, 0)
    const span = Math.max(max - min, 1)
    const w = 120
    const h = props.height
    const step = w / (points.length - 1)
    return points
      .map((value, index) => {
        const x = index * step
        const y = h - ((value - min) / span) * (h - 4) - 2
        return `${index === 0 ? 'M' : 'L'}${x.toFixed(1)} ${y.toFixed(1)}`
      })
      .join(' ')
  })

  const areaPath = computed(() => {
    if (!path.value) return ''
    const w = 120
    const h = props.height
    return `${path.value} L${w} ${h} L0 ${h} Z`
  })
</script>

<template>
  <svg
    class="monitor-sparkline"
    :viewBox="`0 0 120 ${height}`"
    preserveAspectRatio="none"
    aria-hidden="true"
  >
    <path v-if="areaPath" class="monitor-sparkline__area" :d="areaPath" :fill="color" />
    <path
      v-if="path"
      class="monitor-sparkline__line"
      :d="path"
      :stroke="color"
      fill="none"
      stroke-width="1.25"
      stroke-linecap="round"
      stroke-linejoin="round"
      vector-effect="non-scaling-stroke"
    />
  </svg>
</template>

<style scoped lang="scss">
  .monitor-sparkline {
    display: block;
    width: 100%;
    height: v-bind(height + 'px');

    &__area {
      opacity: 0.08;
    }

    &__line {
      opacity: 0.9;
    }
  }
</style>
