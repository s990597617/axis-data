<script setup lang="ts" generic="T extends string">
import { computed } from 'vue'

const props = defineProps<{
  activeKey: T
  tabs: readonly { key: T; label: string }[]
}>()

const emit = defineEmits<{
  'update:activeKey': [key: T]
}>()

const activeIndex = computed(() =>
  Math.max(
    props.tabs.findIndex((tab) => tab.key === props.activeKey),
    0,
  ),
)

const indicatorStyle = computed(() => {
  const count = Math.max(props.tabs.length, 1)
  // padding 4+4，tab 间距 gap 4*(n-1)；指示器宽度须与 flex:1 的 tab 一致
  const gaps = (count - 1) * 4
  return {
    width: `calc((100% - ${8 + gaps}px) / ${count})`,
    transform: `translateX(calc(${activeIndex.value} * (100% + 4px)))`,
  }
})
</script>

<template>
  <div class="pref-tabs">
    <div class="pref-tabs__indicator" :style="indicatorStyle" />
    <div
      v-for="tab in tabs"
      :key="tab.key"
      class="pref-tabs__item"
      :class="{ 'is-active': activeKey === tab.key }"
      @click="emit('update:activeKey', tab.key)"
    >
      {{ tab.label }}
    </div>
  </div>
</template>
