<script setup lang="ts">
  import { AxisTable } from '@/shared/components/axis-table'

  import { useOnlinePage } from './composables/useOnlinePage'

  defineOptions({ name: 'SysOnline' })

  /* ============ 状态 & 引用 ============ */

  const { actionColumn, columns, request, tableRef } = useOnlinePage()
</script>

<template>
  <div class="online-page">
    <AxisTable
      ref="tableRef"
      :columns="columns"
      :action-column="actionColumn"
      :request="request"
      :search-config="{ minItemWidth: 260, maxColumns: 2 }"
      row-key="id"
      :pagination="{ pageSize: 20 }"
    />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
