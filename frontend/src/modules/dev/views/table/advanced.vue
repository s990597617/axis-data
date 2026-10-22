<script setup lang="ts">
  import { computed, ref } from 'vue'

  import {
    AxisTable,
    type AxisTableAction,
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

  defineOptions({ name: 'TableDemoAdvanced' })

  const tableRef = ref<AxisTableAction<DemoUser> | null>(null)
  const onlyHighScore = ref(false)

  const columns: AxisTableColumn<DemoUser>[] = [
    {
      key: 'id',
      title: 'ID',
      width: 90,
      hideInColumnSetting: true
    },
    {
      key: 'username',
      title: '账号',
      search: {
        field: 'keyword',
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
      dict: statusOptions
    },
    {
      key: 'role',
      title: '角色',
      search: { component: 'custom', advanced: true, span: 2 },
      dict: roleOptions
    },
    {
      key: 'department',
      title: '部门',
      search: { advanced: true },
      dict: departmentOptions
    },
    {
      key: 'score',
      title: '评分',
      search: {
        field: 'minScore',
        component: 'custom',
        advanced: true
      }
    }
  ]

  const tableSummary = computed(() => {
    const rows = tableRef.value?.getData() ?? []
    const selected = tableRef.value?.getSelectedRows() ?? []
    return {
      rows: rows.length,
      selected: selected.length
    }
  })

  async function requestUsers(params: AxisTableQuery) {
    await wait(360)

    const minScore = onlyHighScore.value ? 90 : Number(params.minScore ?? 0)
    const filtered = filterDemoUsers(params).filter(item => item.score >= minScore)

    return {
      items: filtered,
      total: filtered.length
    }
  }

  function wait(delay: number) {
    return new Promise(resolve => window.setTimeout(resolve, delay))
  }

  function batchArchive(clear?: () => void) {
    const rows = tableRef.value?.getSelectedRows() ?? []
    if (!rows.length) {
      message.warning('请先选择需要归档的数据')
      return
    }

    message.success(`已模拟归档 ${rows.length} 条数据`)
    clear?.()
  }

  function refreshAfterToggle() {
    void tableRef.value?.search()
  }
</script>

<template>
  <div class="advanced-demo">
    <n-card :bordered="false" class="advanced-demo__panel">
      <div class="advanced-demo__tools">
        <div>
          <div class="advanced-demo__title">进阶组合表格</div>
          <div class="advanced-demo__desc">
            演示字段级自定义搜索、外部条件联动、批量操作、自定义空状态和状态缓存
          </div>
        </div>

        <n-switch v-model:value="onlyHighScore" @update:value="refreshAfterToggle">
          <template #checked>仅高分</template>
          <template #unchecked>全部评分</template>
        </n-switch>
      </div>
    </n-card>

    <AxisTable
      ref="tableRef"
      title="自定义搜索与批量操作"
      :columns="columns"
      :request="requestUsers"
      :search-config="{ minItemWidth: 240, maxColumns: 3, collapsedRows: 1 }"
      :pagination="{ pageSize: 10 }"
      selection="multiple"
      row-key="id"
      column-setting-key="axis-table-demo-advanced-columns"
      state-storage-key="axis-table-demo-advanced-state"
      auto-height
      :height-offset="410"
      empty-description="暂无符合条件的数据"
    >
      <template #toolbar>
        <n-tag size="small">当前 {{ tableSummary.rows }} 条</n-tag>
      </template>

      <template #selection="{ selectedRows, clear }">
        <n-button
          size="small"
          type="primary"
          :disabled="!selectedRows.length"
          @click="batchArchive(clear)"
        >
          批量归档
        </n-button>
      </template>

      <template #search-role="{ value, setValue }">
        <n-radio-group
          :value="(value as string | number | boolean | null | undefined) ?? null"
          @update:value="value => setValue('role', value)"
        >
          <n-radio-button v-for="item in roleOptions" :key="String(item.value)" :value="item.value">
            {{ item.label }}
          </n-radio-button>
        </n-radio-group>
      </template>

      <template #search-minScore="{ value, setValue }">
        <n-input-number
          :value="Number(value ?? 0)"
          :min="0"
          :max="100"
          placeholder="最低评分"
          style="width: 130px"
          @update:value="value => setValue('minScore', value)"
        />
      </template>

      <template #empty>
        <n-empty description="没有找到匹配的演示数据">
          <template #extra>
            <n-button size="small" @click="tableRef?.resetSearch()">清空搜索条件</n-button>
          </template>
        </n-empty>
      </template>
    </AxisTable>
  </div>
</template>

<style scoped lang="scss">
  .advanced-demo {
    display: flex;
    flex-direction: column;
    gap: 16px;

    &__panel {
      border-radius: 8px;
    }

    &__tools {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
    }

    &__title {
      color: var(--axis-text-color-1);
      font-size: 16px;
      font-weight: 650;
    }

    &__desc {
      margin-top: 4px;
      color: var(--axis-text-color-3);
      font-size: 13px;
    }
  }
</style>
