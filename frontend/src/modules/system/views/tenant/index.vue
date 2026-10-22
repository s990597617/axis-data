<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import TenantGrantDialog from './components/TenantGrantDialog.vue'
  import { useTenantPage } from './composables/useTenantPage'

  defineOptions({ name: 'SysTenant' })

  const { resource, grantDialogRef } = useTenantPage()
</script>

<template>
  <div class="tenant-page">
    <AxisCrudPage :resource="resource" />
    <TenantGrantDialog ref="grantDialogRef" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
