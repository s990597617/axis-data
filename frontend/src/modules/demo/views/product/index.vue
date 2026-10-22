<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useProductPage } from './composables/useProductPage'

  defineOptions({ name: 'SysProduct' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useProductPage()
</script>

<template>
  <div class="product-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
