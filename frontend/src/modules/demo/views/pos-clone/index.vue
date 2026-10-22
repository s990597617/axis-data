<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { usePosClonePage } from './composables/usePosClonePage'

  defineOptions({ name: 'SysPosClone' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = usePosClonePage()
</script>

<template>
  <div class="pos-clone-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
