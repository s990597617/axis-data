<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useSourcePage } from './composables/useSourcePage'

  defineOptions({ name: 'DhSource' })

  /* ============ 状态 & 引用 ============ */

  const { resource, pageRef } = useSourcePage()
</script>

<template>
  <div class="source-page">
    <AxisCrudPage ref="pageRef" :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>