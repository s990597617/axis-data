<script setup lang="ts">
  import { computed } from 'vue'
  import { NProgress } from 'naive-ui'

  import { usageColor, usageStatus } from '../utils/format'

  defineOptions({ name: 'MonitorGauge' })

  /* ============ 属性 ============ */

  const props = withDefaults(
    defineProps<{
      value?: number | null
      label: string
      sublabel?: string
    }>(),
    {
      value: null,
      sublabel: ''
    }
  )

  /* ============ 视图模型 ============ */

  const percentage = computed(() =>
    props.value == null ? 0 : Math.min(100, Math.max(0, Number(props.value.toFixed(1))))
  )

  const display = computed(() => (props.value == null ? '--' : `${percentage.value.toFixed(1)}%`))
</script>

<template>
  <div class="monitor-gauge">
    <n-progress
      type="circle"
      :percentage="percentage"
      :status="usageStatus(value)"
      :color="usageColor(value)"
      :rail-color="'color-mix(in srgb, var(--axis-border-color) 55%, transparent)'"
      :stroke-width="8"
      :offset-degree="180"
      style="width: 118px"
    >
      <div class="monitor-gauge__center">
        <strong>{{ display }}</strong>
        <span>{{ label }}</span>
      </div>
    </n-progress>
    <p v-if="sublabel" class="monitor-gauge__sub">{{ sublabel }}</p>
  </div>
</template>

<style scoped lang="scss">
  .monitor-gauge {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;

    &__center {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 2px;

      strong {
        color: var(--axis-text-color-1);
        font-size: 22px;
        font-weight: 700;
        line-height: 1.1;
      }

      span {
        color: var(--axis-text-color-3);
        font-size: 12px;
      }
    }

    &__sub {
      margin: 0;
      color: var(--axis-text-color-3);
      font-size: 12px;
      text-align: center;
      line-height: 1.5;
    }
  }
</style>
