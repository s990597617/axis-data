<script setup lang="ts">
  import { NButton } from 'naive-ui'

  import { AxisTable } from '@/shared/components/axis-table'
  import { Icon } from '@/shared/icons'

  import CacheDetailDrawer from './components/CacheDetailDrawer.vue'
  import CacheGroupPanel from './components/CacheGroupPanel.vue'
  import { useCachePage } from './composables/useCachePage'

  defineOptions({ name: 'SysCache' })

  /* ============ 状态 & 引用 ============ */

  const {
    canClearSelected,
    columns,
    detailDrawerRef,
    groups,
    handleClearPrefix,
    handleFlushAll,
    handleRemoveKey,
    matchedTotal,
    overview,
    permissions,
    prefixHint,
    request,
    selectGroup,
    selectedCode,
    showDetail,
    tableRef,
    truncated
  } = useCachePage()
</script>

<template>
  <div class="cache-page">
    <section class="cache-page__overview">
      <div class="cache-page__stat">
        <span class="cache-page__stat-label">缓存类型</span>
        <span class="cache-page__stat-value">{{ overview?.cacheType || '-' }}</span>
      </div>
      <div class="cache-page__stat">
        <span class="cache-page__stat-label">全局前缀</span>
        <span class="cache-page__stat-value">{{ overview?.globalPrefix || '-' }}</span>
      </div>
      <div class="cache-page__stat">
        <span class="cache-page__stat-label">已知分组</span>
        <span class="cache-page__stat-value">{{ Math.max((groups.length || 1) - 1, 0) }}</span>
      </div>
      <div class="cache-page__stat">
        <span class="cache-page__stat-label">清空全部</span>
        <span class="cache-page__stat-value">
          {{ overview?.allowFlushAll ? '允许（非生产）' : '禁止（生产）' }}
        </span>
      </div>
    </section>

    <CacheGroupPanel
      :groups="groups"
      :selected-code="selectedCode"
      @select="selectGroup"
    />

    <section class="cache-page__table">
      <p class="cache-page__tip">
        {{ prefixHint }}
        <template v-if="matchedTotal">
          · 本次匹配 {{ matchedTotal }} 个键
          <template v-if="truncated">（已截断展示）</template>
        </template>
      </p>

      <AxisTable
        ref="tableRef"
        :columns="columns"
        :request="request"
        :immediate="false"
        :search-config="{ minItemWidth: 280, maxColumns: 2 }"
        row-key="key"
        :pagination="false"
        :action-column="{
          width: 160,
          maxInline: 2,
          actions: [
            {
              key: 'detail',
              label: '查看',
              icon: 'ant-design:eye-outlined',
              type: 'primary',
              permissions: [permissions.query],
              onClick: row => showDetail(row as any)
            },
            {
              key: 'delete',
              label: '删除',
              icon: 'ant-design:delete-outlined',
              type: 'error',
              permissions: [permissions.clear],
              onClick: row => handleRemoveKey(row as any)
            }
          ]
        }"
      >
        <template #toolbar>
          <NButton
            v-permission="permissions.clear"
            type="warning"
            size="small"
            secondary
            :disabled="!canClearSelected"
            @click="handleClearPrefix"
          >
            <template #icon>
              <Icon icon="ant-design:clear-outlined" />
            </template>
            清理当前分组
          </NButton>
          <NButton
            v-if="overview?.allowFlushAll"
            v-permission="permissions.clear"
            type="error"
            size="small"
            secondary
            @click="handleFlushAll"
          >
            <template #icon>
              <Icon icon="ant-design:delete-outlined" />
            </template>
            清空全部
          </NButton>
        </template>
      </AxisTable>
    </section>

    <CacheDetailDrawer ref="detailDrawerRef" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
