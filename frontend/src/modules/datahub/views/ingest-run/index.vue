<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useIngestRunPage } from './composables/useIngestRunPage'

  defineOptions({ name: 'DhIngestRun' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useIngestRunPage()
</script>

<template>
  <div class="ingest-run-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
