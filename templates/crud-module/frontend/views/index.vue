<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { use__Module__Page } from './composables/use__Module__Page'

  defineOptions({ name: '__Entity__' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = use__Module__Page()
</script>

<template>
  <div class="__module_kebab__-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
