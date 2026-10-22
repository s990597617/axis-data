<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import RoleDetailDrawer from './components/RoleDetailDrawer.vue'
  import RoleGrantDialog from './components/RoleGrantDialog.vue'
  import { useRolePage } from './composables/useRolePage'

  defineOptions({ name: 'SysRole' })

  /* ============ 状态 & 引用 ============ */

  const { grantDialogRef, detailDrawerRef, resource } = useRolePage()
</script>

<template>
  <div class="role-page">
    <AxisCrudPage :resource="resource" />
    <RoleGrantDialog ref="grantDialogRef" />
    <RoleDetailDrawer ref="detailDrawerRef" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
