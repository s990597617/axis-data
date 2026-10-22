import { computed, onMounted, ref, h } from 'vue'
import { NTooltip } from 'naive-ui'

import {
  changeJobStatus,
  createJob,
  deleteJob,
  getJobExecutors,
  getJobPage,
  runJob,
  updateJob,
  type JobDto,
  type JobExecutorDto,
  type SaveJobInput
} from '@/modules/system/api/job'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  AxisCrudPage,
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormFieldContext, AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

import JobLogDrawer from '../components/JobLogDrawer.vue'
import { describeCron, yesNoOptions } from '../utils/display'

export function useJobPage() {
  const pageRef = ref<InstanceType<typeof AxisCrudPage> | null>(null)
  const logDrawerRef = ref<InstanceType<typeof JobLogDrawer> | null>(null)
  const executors = ref<JobExecutorDto[]>([])

  const executorOptions = computed<AxisFormOption[]>(() =>
    executors.value.map(item => ({
      label: `${item.name}（${item.code}）`,
      value: item.code
    }))
  )

  const executorMap = computed(() => {
    const map = new Map<string, JobExecutorDto>()
    executors.value.forEach(item => map.set(item.code, item))
    return map
  })

  async function loadExecutors() {
    const [data, error] = await tryRequest(getJobExecutors())
    if (error || !data) return
    executors.value = Array.isArray(data) ? data : []
  }

  function applyExecutorDefaults(model: Record<string, unknown>, code: string) {
    const executor = executorMap.value.get(code)
    if (!executor) return

    model.name = executor.name
    model.groupName = executor.groupName || 'default'
    if (executor.cron) model.cron = executor.cron
    model.concurrent = executor.concurrent
    model.params = executor.defaultParams || ''
    model.orderNo = executor.orderNo
    model.remark = executor.remark || ''
    if (typeof executor.enabled === 'boolean') {
      model.status = executor.enabled ? 1 : 0
    }
  }

  onMounted(() => {
    void loadExecutors()
  })

  const columns: AxisTableColumn<JobDto>[] = [
    { key: 'jobCode', title: '任务编码', ellipsis: { tooltip: true }, search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '任务名称' },
    { key: 'groupName', title: '分组', search: true, width: 100 },
    {
      key: 'cron',
      title: '执行周期',
      ellipsis: { tooltip: true },
      minWidth: 180,
      render: row => {
        const friendly = describeCron(row.cron)
        if (friendly === row.cron) return row.cron
        return h(
          NTooltip,
          { placement: 'top' },
          {
            trigger: () => h('span', { class: 'job-cron-label' }, friendly),
            default: () => row.cron
          }
        )
      }
    },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    {
      key: 'executorRegistered',
      title: '执行器',
      width: 90,
      render: row => (row.executorRegistered ? '已注册' : '缺失')
    },
    {
      key: 'nextFireTime',
      title: '下次执行',
      width: 170,
      render: row => formatDateTime(row.nextFireTime)
    },
    {
      key: 'createdTime',
      title: '创建时间',
      width: 170,
      render: row => formatDateTime(row.createdTime)
    }
  ]

  async function handleRun(row: JobDto) {
    dialog.warning({
      title: '立即执行',
      content: `确定立即执行「${row.name}」？`,
      positiveText: '执行',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(runJob(row.id))
        if (error) return
        message.success('已触发执行')
      }
    })
  }

  async function handleToggle(row: JobDto) {
    const enable = row.status !== 1
    const [, error] = await tryRequest(changeJobStatus(row.id, enable))
    if (error) return
    message.success(enable ? '已启用' : '已停用')
    await pageRef.value?.reload()
  }

  const resource = computed<AxisCrudResource<JobDto>>(() => ({
    title: '',
    entityName: '任务',
    permissions: systemPermissionCodes.job,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑任务「${String(model.name || '-')}」· 调整 Cron 与启停`
        : '从已注册执行器中选择，创建后可调整 Cron / 启停 / 参数',
    actionColumn: {
      width: 220,
      maxInline: 3
    },
    extraActions: [
      {
        key: 'run',
        label: '执行',
        icon: 'ant-design:play-circle-outlined',
        type: 'primary',
        permissions: [systemPermissionCodes.job.run],
        onClick: row => handleRun(row as JobDto)
      },
      {
        key: 'enable',
        label: '启用',
        icon: 'ant-design:check-circle-outlined',
        type: 'success',
        permissions: [systemPermissionCodes.job.edit],
        ifShow: row => (row as JobDto).status !== 1,
        confirm: row => `确定启用「${(row as JobDto).name}」？`,
        onClick: row => handleToggle(row as JobDto)
      },
      {
        key: 'disable',
        label: '停用',
        icon: 'ant-design:stop-outlined',
        type: 'error',
        permissions: [systemPermissionCodes.job.edit],
        ifShow: row => (row as JobDto).status === 1,
        confirm: row => `确定停用「${(row as JobDto).name}」？`,
        onClick: row => handleToggle(row as JobDto)
      },
      {
        key: 'log',
        label: '日志',
        icon: 'ant-design:history-outlined',
        permissions: [systemPermissionCodes.job.log],
        onClick: row => logDrawerRef.value?.open(row as JobDto)
      }
    ],
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:schedule-outlined',
        columns: 2,
        fields: [
          {
            field: 'jobCode',
            label: '执行器',
            component: 'select',
            required: true,
            defaultValue: '',
            options: executorOptions.value,
            disabled: (model, editing) => editing && Boolean(model.isBuiltin),
            help: '对应代码中 IAxisJob.Code；带 AxisJobAttribute 的会在启动时自动入库',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              filterable: true,
              placeholder: executorOptions.value.length ? '请选择已注册执行器' : '暂无已注册执行器',
              onUpdateValue: (value: string) => {
                if (!value) return
                // 新增时选择执行器自动带出默认配置；编辑仅切换编码时也同步名称
                if (!ctx.model.id) {
                  applyExecutorDefaults(ctx.model, value)
                }
              }
            })) as unknown as Record<string, unknown>
          },
          {
            field: 'name',
            label: '任务名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入任务名称' }
          },
          {
            field: 'groupName',
            label: '任务分组',
            defaultValue: 'default',
            componentProps: { maxlength: 64, placeholder: '请输入分组' }
          },
          {
            field: 'cron',
            label: 'Cron',
            required: true,
            defaultValue: '0 */5 * * * ?',
            componentProps: { maxlength: 128, placeholder: 'Quartz Cron 表达式' }
          },
          {
            field: 'concurrent',
            label: '允许并发',
            component: 'radio',
            options: yesNoOptions,
            required: true,
            defaultValue: false
          },
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 0
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 9999 }
          },
          {
            field: 'params',
            label: '参数(JSON)',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: { maxlength: 2000, rows: 3, placeholder: '可选 JSON 参数' }
          },
          {
            field: 'remark',
            label: '备注',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: { maxlength: 256, rows: 2, placeholder: '请输入备注' }
          }
        ]
      }
    ],
    request: getJobPage,
    create: data => createJob(data as unknown as SaveJobInput),
    update: (id, data) => updateJob(id, data as unknown as SaveJobInput),
    remove: deleteJob,
    canDelete: row => !row.isBuiltin
  }))

  return { resource, pageRef, logDrawerRef }
}
