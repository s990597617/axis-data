<script setup lang="ts">
  import { NButton } from 'naive-ui'

  import { AxisTable } from '@/shared/components/axis-table'
  import { DetailDrawer } from '@/shared/components/detail-drawer'
  import { Icon } from '@/shared/icons'

  import { useInboxPage } from './composables/useInboxPage'

  defineOptions({ name: 'SysInbox' })

  const {
    columns,
    currentDetail,
    detailFields,
    detailLoading,
    detailSubtitle,
    detailVisible,
    handleMarkAllRead,
    openBizLink,
    permissions,
    request,
    showDetail,
    tableRef
  } = useInboxPage()
</script>

<template>
  <div class="inbox-page">
    <AxisTable
      ref="tableRef"
      :columns="columns"
      :request="request"
      :search-config="{ minItemWidth: 220, maxColumns: 3 }"
      row-key="id"
      :pagination="{ pageSize: 20 }"
      :action-column="{
        width: 88,
        maxInline: 1,
        actions: [
          {
            key: 'detail',
            label: '查看',
            icon: 'ant-design:eye-outlined',
            type: 'primary',
            permissions: [permissions.query],
            onClick: row => showDetail(row)
          }
        ]
      }"
    >
      <template #toolbar>
        <NButton
          v-permission="permissions.readAll"
          type="primary"
          size="small"
          secondary
          @click="handleMarkAllRead"
        >
          <template #icon>
            <Icon icon="ant-design:check-circle-outlined" />
          </template>
          全部已读
        </NButton>
      </template>
    </AxisTable>

    <DetailDrawer
      v-model:show="detailVisible"
      title="消息详情"
      :subtitle="detailSubtitle"
      icon="ant-design:mail-outlined"
      :loading="detailLoading"
      :fields="detailFields"
      section-title="基本信息"
      width="560"
    >
      <template v-if="currentDetail?.resolvedLink" #footer>
        <NButton type="primary" @click="openBizLink">
          打开业务页面
        </NButton>
      </template>
    </DetailDrawer>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
