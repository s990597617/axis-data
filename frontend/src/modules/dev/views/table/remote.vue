<script setup lang="ts">
  import { ref } from 'vue'

  import {
    AxisTable,
    type AxisTableAction,
    type AxisTableActionColumn,
    type AxisTableColumn,
    type AxisTableQuery
  } from '@/shared/components/axis-table'
  import { message } from '@/shared/utils/message'

  import {
    departmentOptions,
    filterDemoUsers,
    roleOptions,
    statusOptions,
    type DemoUser
  } from './data'

  defineOptions({ name: 'TableDemoRemote' })

  const tableRef = ref<AxisTableAction<DemoUser> | null>(null)

  const columns: AxisTableColumn<DemoUser>[] = [
    {
      key: 'id',
      title: 'ID',
      width: 90,
      hideInSearch: true
    },
    {
      key: 'username',
      title: '账号',
      search: {
        field: 'keyword',
        placeholder: '输入账号 / 昵称',
        primary: true
      }
    },
    {
      key: 'nickname',
      title: '昵称'
    },
    {
      key: 'status',
      title: '状态',
      search: { primary: true },
      dict: async () => {
        await wait(220)
        return { data: statusOptions }
      }
    },
    {
      key: 'role',
      title: '角色',
      search: true,
      dict: roleOptions
    },
    {
      key: 'department',
      title: '部门',
      search: { advanced: true },
      dict: departmentOptions
    },
    {
      key: 'createdAt',
      title: '创建时间',
      search: {
        field: 'createdAtRange',
        component: 'dateRange',
        advanced: true,
        span: 2
      }
    }
  ]

  const actionColumn: AxisTableActionColumn<DemoUser> = {
    width: 150,
    actions: [
      {
        key: 'view',
        label: '查看',
        type: 'primary',
        onClick: row => {
          message.info(`查看 ${row.nickname}`)
        }
      },
      {
        key: 'enable',
        label: '启用',
        ifShow: row => row.status !== 1,
        confirm: row => `确认启用「${row.nickname}」吗？`,
        onClick: row => {
          message.success(`已模拟启用 ${row.nickname}`)
        }
      },
      {
        key: 'disable',
        label: '禁用',
        type: 'error',
        ifShow: row => row.status === 1,
        confirm: row => `确认禁用「${row.nickname}」吗？`,
        onClick: row => {
          message.warning(`已模拟禁用 ${row.nickname}`)
        }
      },
      {
        key: 'detail',
        label: '更多详情',
        more: true,
        onClick: row => {
          message.info(`打开 ${row.username} 的更多详情`)
        }
      }
    ]
  }

  async function requestUsers(params: AxisTableQuery) {
    await wait(520)

    const page = Number(params.page ?? 1)
    const pageSize = Number(params.pageSize ?? 10)
    const filtered = filterDemoUsers(params)
    const start = (page - 1) * pageSize

    return {
      data: {
        records: filtered.slice(start, start + pageSize),
        total: filtered.length,
        current: page,
        pages: Math.ceil(filtered.length / pageSize)
      }
    }
  }

  function wait(delay: number) {
    return new Promise(resolve => window.setTimeout(resolve, delay))
  }

  function reloadWithToast() {
    void tableRef.value?.reload().then(() => {
      message.success('远程数据已刷新')
    })
  }
</script>

<template>
  <AxisTable
    ref="tableRef"
    title="远程请求表格"
    title-tooltip="演示 request、分页适配、异步字典、搜索联动、操作列和错误状态"
    :columns="columns"
    :request="requestUsers"
    :search-config="{ minItemWidth: 220, maxColumns: 4, collapsedRows: 1 }"
    :action-column="actionColumn"
    :pagination="{ pageSize: 10 }"
    selection="multiple"
    row-key="id"
    column-setting-key="axis-table-demo-remote-columns"
    state-storage-key="axis-table-demo-remote-state"
    auto-height
    :height-offset="330"
  >
    <template #toolbar>
      <n-button size="small" @click="reloadWithToast">刷新并提示</n-button>
    </template>
  </AxisTable>
</template>
