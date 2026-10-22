<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import { useNoticePage } from './composables/useNoticePage'

  defineOptions({ name: 'SysNotice' })

  /* ============ 状态 & 引用 ============ */

  const { resource, pageRef } = useNoticePage()
</script>

<template>
  <div class="notice-page">
    <AxisCrudPage ref="pageRef" :resource="resource" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
