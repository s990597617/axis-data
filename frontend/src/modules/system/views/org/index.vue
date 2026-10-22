<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useOrgPage } from './composables/useOrgPage'

  defineOptions({ name: 'SysOrg' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useOrgPage()
</script>

<template>
  <div class="org-page">
    <AxisCrudPage :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
