import type { IngestRunDto, SaveIngestRunInput } from '@/modules/datahub/api/ingest-run'
import {
  createIngestRun,
  deleteIngestRun,
  getIngestRunPage,
  updateIngestRun
} from '@/modules/datahub/api/ingest-run'
import { listIngestJobLookup, type DataHubLookupItem } from '@/modules/datahub/api/ingest-job'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormOption } from '@/shared/components/axis-form'
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'

const runStatusOptions: AxisFormOption[] = [
  { label: '待执行', value: 1 },
  { label: '运行中', value: 2 },
  { label: '成功', value: 3 },
  { label: '失败', value: 4 },
  { label: '已取消', value: 5 }
]

function toLookupOptions(items: DataHubLookupItem[] | undefined): AxisFormOption[] {
  return (items ?? []).map(item => ({ label: item.label, value: item.value }))
}

function asIdString(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value)
}

export function useIngestRunPage() {
  const route = useRoute()
  const initialJobId = ref<string>('')
  const jobOptions = ref<AxisFormOption[]>([])

  onMounted(() => {
    const raw = route.query.jobId
    const id = raw == null ? '' : String(raw).trim()
    if (id) initialJobId.value = id
  })

  async function loadJobOptions(currentId?: unknown) {
    try {
      jobOptions.value = toLookupOptions(await listIngestJobLookup())
    } catch {
      jobOptions.value = []
    }
    const id = asIdString(currentId)
    if (id && !jobOptions.value.some(o => String(o.value) === id)) {
      jobOptions.value = [{ label: `当前采集任务（${id}）`, value: id }, ...jobOptions.value]
    }
  }

  const columns: AxisTableColumn<IngestRunDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    {
      key: 'jobName',
      title: '采集任务',
      render: row => row.jobName || row.jobCode || asIdString(row.jobId) || '-'
    },
    { key: 'runStatus', title: '运行状态', dict: runStatusOptions },
    {
      key: 'startedTime',
      title: '开始时间',
      render: row => formatDateTime(row.startedTime)
    },
    {
      key: 'finishedTime',
      title: '结束时间',
      render: row => formatDateTime(row.finishedTime)
    },
    { key: 'rowCountRead', title: '读取行数', width: 100 },
    { key: 'rowCountChanged', title: '变更行数', width: 100 },
    { key: 'watermark', title: '水位线', ellipsis: { tooltip: true } },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }
  ]

  const resource: AxisCrudResource<IngestRunDto> = {
    title: '',
    entityName: '采集运行',
    permissions: dataHubPermissionCodes.ingestRun,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑采集运行「${String((model as Record<string, unknown>).name || '-')}」`
        : '新建采集运行',
    actionColumn: {
      width: 120,
      maxInline: 2
    },
    formSections: [
      {
        key: 'basic',
        title: '基本信息',
        icon: 'ant-design:idcard-outlined',
        columns: 2,
        fields: [
          {
            field: 'jobId',
            label: '采集任务',
            component: 'select',
            required: true,
            defaultValue: null,
            options: () => jobOptions.value,
            componentProps: {
              filterable: true,
              clearable: true,
              placeholder: '请选择采集任务'
            }
          },
          {
            field: 'name',
            label: '名称',
            defaultValue: '',
            componentProps: {
              maxlength: 128,
              placeholder: '可留空，新建时按任务自动生成'
            }
          },
          {
            field: 'code',
            label: '编码',
            defaultValue: '',
            componentProps: {
              maxlength: 64,
              placeholder: '可留空，新建时按任务自动生成'
            }
          },
          {
            field: 'runStatus',
            label: '运行状态',
            component: 'select',
            options: runStatusOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'startedTime',
            label: '开始时间',
            defaultValue: '',
            componentProps: { placeholder: 'YYYY-MM-DD HH:mm:ss' }
          },
          {
            field: 'finishedTime',
            label: '结束时间',
            defaultValue: '',
            componentProps: { placeholder: 'YYYY-MM-DD HH:mm:ss' }
          },
          {
            field: 'rowCountRead',
            label: '读取行数',
            component: 'inputNumber',
            defaultValue: 0,
            componentProps: { min: 0 }
          },
          {
            field: 'rowCountChanged',
            label: '变更行数',
            component: 'inputNumber',
            defaultValue: 0,
            componentProps: { min: 0 }
          },
          {
            field: 'watermark',
            label: '水位线',
            defaultValue: '',
            componentProps: { maxlength: 512, placeholder: '增量水位' }
          },
          {
            field: 'errorMessage',
            label: '错误信息',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            ifShow: model => Number(model.runStatus) === 4,
            componentProps: { rows: 2, maxlength: 1024, placeholder: '失败时填写' }
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 999999 }
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
      model.jobId = asIdString(model.jobId) || null
      await loadJobOptions(model.jobId)
    },
    beforeSubmit: data => {
      data.jobId = asIdString(data.jobId)
      if (!data.jobId) throw new Error('请选择采集任务')
      if (data.name === '') data.name = undefined
      if (data.code === '') data.code = undefined
      return data
    },
    request: params => {
      const merged = { ...params }
      if (initialJobId.value && merged.jobId == null && merged.JobId == null) {
        merged.jobId = initialJobId.value
      }
      return getIngestRunPage(merged)
    },
    create: data => createIngestRun(data as unknown as SaveIngestRunInput),
    update: (id, data) => updateIngestRun(id, data as unknown as SaveIngestRunInput),
    remove: deleteIngestRun
  }

  return { resource }
}
