<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import JobLogDrawer from './components/JobLogDrawer.vue'
  import { useJobPage } from './composables/useJobPage'

  defineOptions({ name: 'SysJob' })

  /* ============ 状态 & 引用 ============ */

  const { resource, pageRef, logDrawerRef } = useJobPage()
</script>

<template>
  <div class="job-page">
    <AxisCrudPage ref="pageRef" :resource="resource" />
    <JobLogDrawer ref="logDrawerRef" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
