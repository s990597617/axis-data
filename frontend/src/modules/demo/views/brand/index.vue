<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useBrandPage } from './composables/useBrandPage'

  defineOptions({ name: 'SysBrand' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useBrandPage()
</script>

<template>
  <div class="brand-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
