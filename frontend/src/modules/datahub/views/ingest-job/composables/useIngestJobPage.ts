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

export function useIngestJobPage() {
  const columns: AxisTableColumn<IngestJobDto>[] = [
    { key: 'datasetId', title: 'DatasetId' },
    { key: 'name', title: 'Name', search: { field: 'keyword', placeholder: '关键字' } },
    { key: 'code', title: 'Code' },
    { key: 'strategy', title: 'Strategy' },
    { key: 'cronExpression', title: 'CronExpression' },
    { key: 'batchSize', title: 'BatchSize' },
    { key: 'orderNo', title: 'OrderNo', width: 80 },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'remark', title: 'Remark', ellipsis: { tooltip: true } },
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
        : '创建采集任务',
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
            label: 'DatasetId',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入DatasetId' }
          },
          {
            field: 'name',
            label: 'Name',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入Name' }
          },
          {
            field: 'code',
            label: 'Code',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入Code' }
          },
          {
            field: 'strategy',
            label: 'Strategy',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入Strategy' }
          },
          {
            field: 'cronExpression',
            label: 'CronExpression',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入CronExpression' }
          },
          {
            field: 'batchSize',
            label: 'BatchSize',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入BatchSize' }
          },
          {
            field: 'orderNo',
            label: 'OrderNo',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 999999, placeholder: '请输入OrderNo' }
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
            label: 'Remark',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 256,
              showCount: true,
              rows: 2,
              placeholder: '请输入Remark'
            }
          },
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
