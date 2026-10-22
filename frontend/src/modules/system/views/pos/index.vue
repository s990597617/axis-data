<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { usePosPage } from './composables/usePosPage'

  defineOptions({ name: 'SysPos' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = usePosPage()
</script>

<template>
  <div class="pos-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
