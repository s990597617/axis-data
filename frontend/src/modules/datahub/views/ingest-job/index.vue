<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useIngestJobPage } from './composables/useIngestJobPage'

  defineOptions({ name: 'DhIngestJob' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useIngestJobPage()
</script>

<template>
  <div class="ingest-job-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
