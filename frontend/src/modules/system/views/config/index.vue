<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useConfigPage } from './composables/useConfigPage'

  defineOptions({ name: 'SysConfig' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useConfigPage()
</script>

<template>
  <div class="config-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
