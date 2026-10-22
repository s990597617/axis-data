import type { IngestJobDto, SaveIngestJobInput } from '@/modules/datahub/api/ingest-job'
import {
  createIngestJob,
  deleteIngestJob,
  getIngestJobPage,
  runIngestJob,
  updateIngestJob
} from '@/modules/datahub/api/ingest-job'
import { listDatasetLookup, type DataHubLookupItem } from '@/modules/datahub/api/dataset'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  AxisCrudPage,
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormFieldContext, AxisFormOption } from '@/shared/components/axis-form'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const strategyOptions: AxisFormOption[] = [
  { label: '全量', value: 1 },
  { label: '增量', value: 2 },
  { label: '差异检测', value: 3 }
]

/** Cron 预设：值为 Quartz 6 段表达式；空串=不调度；__custom__=自定义。 */
const CRON_CUSTOM = '__custom__'
const cronPresetOptions: AxisFormOption[] = [
  { label: '不调度（手动触发）', value: '' },
  { label: '每 5 分钟', value: '0 */5 * * * ?' },
  { label: '每 15 分钟', value: '0 */15 * * * ?' },
  { label: '每小时', value: '0 0 * * * ?' },
  { label: '每天 0 点', value: '0 0 0 * * ?' },
  { label: '每天 8 点', value: '0 0 8 * * ?' },
  { label: '每周一 8 点', value: '0 0 8 ? * MON' },
  { label: '自定义', value: CRON_CUSTOM }
]

function resolveCronPreset(expression: unknown): string {
  const raw = expression == null ? '' : String(expression).trim()
  if (!raw) return ''
  const hit = cronPresetOptions.find(
    item => item.value !== CRON_CUSTOM && String(item.value) === raw
  )
  return hit ? String(hit.value) : CRON_CUSTOM
}

function toLookupOptions(items: DataHubLookupItem[] | undefined): AxisFormOption[] {
  return (items ?? []).map(item => ({ label: item.label, value: item.value }))
}

function asIdString(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value)
}

export function useIngestJobPage() {
  const router = useRouter()
  const pageRef = ref<InstanceType<typeof AxisCrudPage> | null>(null)
  const datasetOptions = ref<AxisFormOption[]>([])

  async function loadDatasetOptions(currentId?: unknown) {
    try {
      datasetOptions.value = toLookupOptions(await listDatasetLookup())
    } catch {
      datasetOptions.value = []
    }
    const id = asIdString(currentId)
    if (id && !datasetOptions.value.some(o => String(o.value) === id)) {
      datasetOptions.value = [{ label: `当前数据集（${id}）`, value: id }, ...datasetOptions.value]
    }
  }

  const columns: AxisTableColumn<IngestJobDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    {
      key: 'datasetName',
      title: '数据集',
      render: row => row.datasetName || row.datasetCode || asIdString(row.datasetId) || '-'
    },
    { key: 'strategy', title: '策略', dict: strategyOptions },
    {
      key: 'cronExpression',
      title: 'Cron',
      render: row => {
        const raw = String(row.cronExpression ?? '').trim()
        if (!raw) return '不调度'
        const preset = cronPresetOptions.find(
          item => item.value !== CRON_CUSTOM && String(item.value) === raw
        )
        return preset ? String(preset.label) : raw
      }
    },
    { key: 'batchSize', title: '批大小', width: 90 },
    { key: 'orderNo', title: '排序', width: 80 },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }
  ]

  const resource: AxisCrudResource<IngestJobDto> = {
    title: '',
    entityName: '采集任务',
    permissions: dataHubPermissionCodes.ingestJob,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑采集任务「${String((model as Record<string, unknown>).name || '-')}」`
        : '新建采集任务',
    actionColumn: {
      width: 200,
      maxInline: 2
    },
    extraActions: [
      {
        key: 'run',
        label: '立即执行',
        icon: 'ant-design:play-circle-outlined',
        type: 'primary',
        permissions: [dataHubPermissionCodes.ingestJob.run],
        ifShow: row => (row as IngestJobDto).status === 1,
        onClick: row => handleRun(row as IngestJobDto)
      }
    ],
    formSections: [
      {
        key: 'basic',
        title: '基本信息',
        icon: 'ant-design:idcard-outlined',
        columns: 2,
        fields: [
          {
            field: 'datasetId',
            label: '数据集',
            component: 'select',
            required: true,
            defaultValue: null,
            options: () => datasetOptions.value,
            componentProps: {
              filterable: true,
              clearable: true,
              placeholder: '请选择数据集'
            }
          },
          {
            field: 'name',
            label: '名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入名称' }
          },
          {
            field: 'code',
            label: '编码',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入编码' }
          },
          {
            field: 'strategy',
            label: '策略',
            component: 'select',
            options: strategyOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'cronPreset',
            label: '调度频率',
            component: 'select',
            options: cronPresetOptions,
            required: true,
            defaultValue: '0 0 * * * ?',
            help: '常用调度一键选择；需要特殊节奏时选「自定义」填写 Quartz 表达式',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              filterable: true,
              placeholder: '请选择调度频率',
              onUpdateValue: (value: unknown) => {
                ctx.setValue(value)
                const preset = value == null ? '' : String(value)
                if (preset !== CRON_CUSTOM) {
                  ctx.model.cronExpression = preset
                } else if (!String(ctx.model.cronExpression ?? '').trim()) {
                  ctx.model.cronExpression = '0 0 * * * ?'
                }
              }
            })) as unknown as Record<string, unknown>
          },
          {
            field: 'cronExpression',
            label: 'Cron 表达式',
            defaultValue: '0 0 * * * ?',
            ifShow: model => String(model.cronPreset ?? '') === CRON_CUSTOM,
            help: 'Quartz 6 段：秒 分 时 日 月 周，例如 0 0 8 * * ?',
            componentProps: {
              maxlength: 64,
              placeholder: '如 0 0 * * * ?'
            }
          },
          {
            field: 'batchSize',
            label: '批大小',
            component: 'inputNumber',
            required: true,
            defaultValue: 1000,
            componentProps: { min: 1, max: 1000000, placeholder: '每批条数' }
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 999999, placeholder: '请输入排序' }
          },
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'remark',
            label: '备注',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 256,
              showCount: true,
              rows: 2,
              placeholder: '请输入备注'
            }
          }
        ]
      }
    ],
    onFormOpen: async ({ model }) => {
      model.datasetId = asIdString(model.datasetId) || null
      const preset = resolveCronPreset(model.cronExpression)
      model.cronPreset = preset
      if (preset !== CRON_CUSTOM && preset !== '') {
        model.cronExpression = preset
      }
      if (preset === '' && (model.cronExpression == null || model.cronExpression === undefined)) {
        model.cronExpression = ''
      }
      await loadDatasetOptions(model.datasetId)
    },
    beforeSubmit: data => {
      data.datasetId = asIdString(data.datasetId)
      if (!data.datasetId) throw new Error('请选择数据集')

      const preset = data.cronPreset == null ? '' : String(data.cronPreset)
      if (preset === CRON_CUSTOM) {
        data.cronExpression =
          data.cronExpression == null ? '' : String(data.cronExpression).trim()
      } else {
        data.cronExpression = preset
      }
      delete data.cronPreset
      return data
    },
    request: getIngestJobPage,
    create: data => createIngestJob(data as unknown as SaveIngestJobInput),
    update: (id, data) => updateIngestJob(id, data as unknown as SaveIngestJobInput),
    remove: deleteIngestJob
  }

  async function handleRun(row: IngestJobDto) {
    dialog.warning({
      title: '立即执行',
      content: `确定立即执行采集任务「${row.name}」？将创建一条采集运行记录并从源端读取一批数据。`,
      positiveText: '执行',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [result, error] = await tryRequest(runIngestJob(row.id))
        if (error) return
        const payload = result
        if (!payload) {
          message.error('采集执行完成，但未返回运行结果')
          await pageRef.value?.reload()
          return
        }
        if (payload.runStatus === 4) {
          message.error(payload.errorMessage || '采集执行失败')
        } else {
          message.success(`采集完成，读取 ${payload.rowCountRead} 行`)
        }
        await pageRef.value?.reload()
        dialog.info({
          title: '查看运行记录',
          content: '是否跳转到「采集运行」查看本次记录？',
          positiveText: '去查看',
          negativeText: '留在此页',
          onPositiveClick: () => {
            void router.push({
              path: '/datahub/ingest-run',
              query: { jobId: row.id }
            })
          }
        })
      }
    })
  }

  return { resource, pageRef }
}
