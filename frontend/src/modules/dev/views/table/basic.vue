<script setup lang="ts">
  import { h, ref } from 'vue'

  import {
    AxisTable,
    type AxisTableAction,
    type AxisTableColumn
  } from '@/shared/components/axis-table'
  import { message } from '@/shared/utils/message'

  import {
    demoUsers,
    filterDemoUsers,
    roleOptions,
    statusOptions,
    type DemoUser
  } from './data'

  defineOptions({ name: 'TableDemoBasic' })

  const tableRef = ref<AxisTableAction<DemoUser> | null>(null)

  const columns: AxisTableColumn<DemoUser>[] = [
    {
      key: 'id',
      title: 'ID',
      width: 90
    },
    {
      key: 'username',
      title: '账号',
      search: {
        field: 'keyword',
        placeholder: '搜索账号或昵称'
      }
    },
    {
      key: 'nickname',
      title: '昵称'
    },
    {
      key: 'status',
      title: '状态',
      search: true,
      dict: statusOptions
    },
    {
      key: 'role',
      title: '角色',
      search: true,
      dict: roleOptions
    },
    {
      key: 'department',
      title: '部门'
    },
    {
      key: 'score',
      title: '评分',
      render: row =>
        h(
          'span',
          {
            style: {
              color: row.score >= 90 ? 'var(--axis-success-color)' : 'var(--axis-text-color-2)'
            }
          },
          row.score
        )
    }
  ]

  function exportCurrentData() {
    tableRef.value?.exportCsv('基础表格演示.csv')
  }

  function showSelectedRows() {
    const rows = tableRef.value?.getSelectedRows() ?? []
    message.info(rows.length ? `已选择 ${rows.length} 条` : '暂未选择数据')
  }
</script>

<template>
  <AxisTable
    ref="tableRef"
    title="基础静态表格"
    title-tooltip="演示静态 data、列级搜索、字典反显、行选择和 CSV 导出"
    :columns="columns"
    :data="demoUsers"
    :local-filter="(_rows, params) => filterDemoUsers(params)"
    :pagination="{ pageSize: 10 }"
    selection="multiple"
    row-key="id"
    column-setting-key="axis-table-demo-basic-columns"
    state-storage-key="axis-table-demo-basic-state"
    auto-height
    :height-offset="330"
  >
    <template #toolbar>
      <n-button size="small" @click="showSelectedRows">查看选中</n-button>
      <n-button size="small" type="primary" @click="exportCurrentData">导出当前数据</n-button>
    </template>
  </AxisTable>
</template>
