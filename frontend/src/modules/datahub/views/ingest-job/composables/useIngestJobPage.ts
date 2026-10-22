import type { IngestJobDto, SaveIngestJobInput } from '@/modules/datahub/api/ingest-job'
import {
  createIngestJob,
  deleteIngestJob,
  getIngestJobPage,
  updateIngestJob
} from '@/modules/datahub/api/ingest-job'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormOption } from '@/shared/components/axis-form'

const strategyOptions: AxisFormOption[] = [
  { label: '全量', value: 1 },
  { label: '增量', value: 2 },
  { label: '差异检测', value: 3 }
]

export function useIngestJobPage() {
  const columns: AxisTableColumn<IngestJobDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'datasetId', title: '数据集Id', width: 110 },
    { key: 'strategy', title: '策略', dict: strategyOptions },
    { key: 'cronExpression', title: 'Cron表达式' },
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
        : '新增采集任务',
    actionColumn: {
      width: 120,
      maxInline: 2
    },
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:idcard-outlined',
        columns: 2,
        fields: [
          {
            field: 'datasetId',
            label: '数据集Id',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 1, placeholder: '请输入数据集 Id' }
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
            label: 'Cron表达式',
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '例如 0 0/10 * * * ?' }
          },
          {
            field: 'batchSize',
            label: '批大小',
            component: 'inputNumber',
            required: true,
            defaultValue: 1000,
            componentProps: { min: 1, max: 999999, placeholder: '请输入批大小' }
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
    request: getIngestJobPage,
    create: data => createIngestJob(data as unknown as SaveIngestJobInput),
    update: (id, data) => updateIngestJob(id, data as unknown as SaveIngestJobInput),
    remove: deleteIngestJob
  }

  return { resource }
}
