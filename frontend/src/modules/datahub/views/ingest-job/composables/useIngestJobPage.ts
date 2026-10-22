import type { IngestJobDto, SaveIngestJobInput } from '@/modules/datahub/api/ingest-job'
import {
  createIngestJob,
  deleteIngestJob,
  getIngestJobPage,
  updateIngestJob
} from '@/modules/datahub/api/ingest-job'
import { listDatasetLookup, type DataHubLookupItem } from '@/modules/datahub/api/dataset'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormOption } from '@/shared/components/axis-form'
import { ref } from 'vue'

const strategyOptions: AxisFormOption[] = [
  { label: '全量', value: 1 },
  { label: '增量', value: 2 },
  { label: '差异检测', value: 3 }
]

function toLookupOptions(items: DataHubLookupItem[] | undefined): AxisFormOption[] {
  return (items ?? []).map(item => ({ label: item.label, value: item.value }))
}

function asIdString(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value)
}

export function useIngestJobPage() {
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
    { key: 'cronExpression', title: 'Cron' },
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
            field: 'cronExpression',
            label: 'Cron 表达式',
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '如 0 0 * * * ?' }
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
      await loadDatasetOptions(model.datasetId)
    },
    beforeSubmit: data => {
      data.datasetId = asIdString(data.datasetId)
      if (!data.datasetId) throw new Error('请选择数据集')
    },
    request: getIngestJobPage,
    create: data => createIngestJob(data as unknown as SaveIngestJobInput),
    update: (id, data) => updateIngestJob(id, data as unknown as SaveIngestJobInput),
    remove: deleteIngestJob
  }

  return { resource }
}
