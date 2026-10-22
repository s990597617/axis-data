<script setup lang="ts">
  import { computed, h, ref } from 'vue'
  import { NButton, NTag } from 'naive-ui'

  import { getJobLogPage, type JobDto, type JobLogDto } from '@/modules/system/api/job'
  import { formatDateTime } from '@/shared/components/axis-crud'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'
  import { AxisTable, type AxisTableColumn } from '@/shared/components/axis-table'
  import { Icon } from '@/shared/icons'

  import { describeCron, jobTriggerTypeOptions } from '../utils/display'

  defineOptions({ name: 'JobLogDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const job = ref<JobDto | null>(null)
  const selectedLog = ref<JobLogDto | null>(null)

  /* ============ 视图模型 ============ */

  const statusText = computed(() => {
    if (!job.value) return undefined
    return job.value.status === 1 ? '启用' : '停用'
  })

  const statusType = computed(() => {
    if (!job.value) return undefined
    return job.value.status === 1 ? ('success' as const) : ('warning' as const)
  })

  const detailFields = computed<DetailField[]>(() => {
    const row = job.value
    if (!row) return []
    return [
      {
        key: 'jobCode',
        label: '任务编码',
        value: row.jobCode,
        type: 'code',
        icon: 'ant-design:code-outlined'
      },
      {
        key: 'name',
        label: '任务名称',
        value: row.name,
        icon: 'ant-design:schedule-outlined'
      },
      {
        key: 'groupName',
        label: '分组',
        value: row.groupName || 'default',
        icon: 'ant-design:folder-outlined'
      },
      {
        key: 'cron',
        label: '执行周期',
        value: describeCron(row.cron),
        icon: 'ant-design:field-time-outlined',
        ellipsis: true,
        fullRow: true,
        formatter: () => {
          const friendly = describeCron(row.cron)
          return friendly === row.cron ? row.cron : `${friendly}（${row.cron}）`
        }
      },
      {
        key: 'concurrent',
        label: '允许并发',
        value: row.concurrent ? '是' : '否',
        type: 'tag',
        tagType: row.concurrent ? 'warning' : 'default',
        icon: 'ant-design:cluster-outlined'
      },
      {
        key: 'nextFireTime',
        label: '下次执行',
        value: row.nextFireTime ? formatDateTime(row.nextFireTime) : '-',
        icon: 'ant-design:clock-circle-outlined'
      }
    ]
  })

  const columns: AxisTableColumn<JobLogDto>[] = [
    {
      key: 'startedTime',
      title: '开始时间',
      width: 168,
      render: row => formatDateTime(row.startedTime)
    },
    {
      key: 'status',
      title: '结果',
      width: 88,
      search: true,
      dict: [
        { label: '成功', value: 1 },
        { label: '失败', value: 0 }
      ],
      render: row => {
        const ok = row.status === 1
        return h(
          NTag,
          { size: 'small', round: true, type: ok ? 'success' : 'error', bordered: false },
          { default: () => (ok ? '成功' : '失败') }
        )
      }
    },
    {
      key: 'triggerType',
      title: '触发',
      width: 84,
      dict: jobTriggerTypeOptions,
      render: row => {
        const item = jobTriggerTypeOptions.find(option => option.value === row.triggerType)
        const manual = row.triggerType === 2
        return h(
          NTag,
          {
            size: 'small',
            round: true,
            type: manual ? 'info' : 'default',
            bordered: false
          },
          { default: () => item?.label ?? '-' }
        )
      }
    },
    {
      key: 'durationMs',
      title: '耗时',
      width: 96,
      render: row => formatDuration(row.durationMs)
    },
    {
      key: 'message',
      title: '摘要',
      ellipsis: { tooltip: true },
      render: row => {
        if (!row.exception) return row.message || '-'
        return h(
          'button',
          {
            type: 'button',
            class: 'job-log-drawer__link',
            onClick: () => {
              selectedLog.value = row
            }
          },
          row.message || '执行失败，点击查看异常'
        )
      }
    }
  ]

  /* ============ 交互操作 ============ */

  function formatDuration(ms?: number | null) {
    if (ms == null || Number.isNaN(ms)) return '-'
    if (ms < 1000) return `${ms} ms`
    return `${(ms / 1000).toFixed(2)} s`
  }

  function open(row: JobDto) {
    job.value = row
    selectedLog.value = null
    visible.value = true
  }

  function request(params: Record<string, unknown>) {
    return getJobLogPage({
      ...params,
      jobId: job.value?.id
    })
  }

  function clearSelected() {
    selectedLog.value = null
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="执行日志"
    :subtitle="job?.jobCode"
    icon="ant-design:history-outlined"
    icon-theme="info"
    :status-text="statusText"
    :status-type="statusType"
    :width="920"
    :fields="detailFields"
    section-title="任务信息"
    fill-content
    @close="clearSelected"
  >
    <section v-if="job" class="job-log-drawer">
      <div class="job-log-drawer__header">
        <span class="job-log-drawer__title">执行记录</span>
        <span class="job-log-drawer__hint">按时间倒序 · 支持筛选与分页</span>
      </div>

      <div class="job-log-drawer__table">
        <AxisTable
          v-if="visible"
          class="job-log-drawer__axis-table"
          :columns="columns"
          :request="request"
          row-key="id"
          :show-toolbar="false"
          :search-config="{ minItemWidth: 200, maxColumns: 2 }"
          :pagination="{ pageSize: 10, pageSizes: [10, 20, 50] }"
          :table-props="{ flexHeight: true, bordered: false }"
        />
      </div>

      <div v-if="selectedLog?.exception" class="job-log-drawer__detail is-error">
        <div class="job-log-drawer__detail-header">
          <div class="job-log-drawer__detail-heading">
            <Icon icon="ant-design:bug-outlined" />
            <span>异常详情</span>
            <NTag size="tiny" round type="error" :bordered="false">失败</NTag>
          </div>
          <NButton text size="small" @click="clearSelected">关闭</NButton>
        </div>
        <div class="job-log-drawer__detail-meta">
          <span>{{ formatDateTime(selectedLog.startedTime) }}</span>
          <span>·</span>
          <span>{{ formatDuration(selectedLog.durationMs) }}</span>
        </div>
        <pre class="job-log-drawer__exception">{{ selectedLog.exception }}</pre>
      </div>
    </section>
  </DetailDrawer>
</template>

<style scoped lang="scss">
  @use '../styles/log-drawer.scss';
</style>
