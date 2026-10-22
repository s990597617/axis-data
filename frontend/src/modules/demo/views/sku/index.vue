<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useSkuPage } from './composables/useSkuPage'

  defineOptions({ name: 'SysSku' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useSkuPage()
</script>

<template>
  <div class="sku-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
