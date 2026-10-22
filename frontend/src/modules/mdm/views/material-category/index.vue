<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useMaterialCategoryPage } from './composables/useMaterialCategoryPage'

  defineOptions({ name: 'MdmMaterialCategory' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useMaterialCategoryPage()
</script>

<template>
  <div class="material-category-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
