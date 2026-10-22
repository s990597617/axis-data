<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useDatasetPage } from './composables/useDatasetPage'

  defineOptions({ name: 'DhDataset' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useDatasetPage()
</script>

<template>
  <div class="dataset-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
