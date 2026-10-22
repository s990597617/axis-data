import type { IngestRunDto, SaveIngestRunInput } from '@/modules/datahub/api/ingest-run'
import {
  createIngestRun,
  deleteIngestRun,
  getIngestRunPage,
  updateIngestRun
} from '@/modules/datahub/api/ingest-run'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function useIngestRunPage() {
  const columns: AxisTableColumn<IngestRunDto>[] = [
    { key: 'jobId', title: 'JobId' },
    { key: 'name', title: 'Name', search: { field: 'keyword', placeholder: '关键字' }, ellipsis: { tooltip: true } },
    { key: 'code', title: 'Code' },
    { key: 'runStatus', title: 'RunStatus' },
    { key: 'startedTime', title: 'StartedTime', render: row => formatDateTime(row.startedTime) },
    { key: 'finishedTime', title: 'FinishedTime', render: row => formatDateTime(row.finishedTime) },
    { key: 'rowCountRead', title: 'RowCountRead' },
    { key: 'rowCountChanged', title: 'RowCountChanged' },
    { key: 'errorMessage', title: 'ErrorMessage', ellipsis: { tooltip: true } },
    { key: 'watermark', title: 'Watermark', ellipsis: { tooltip: true } },
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

  const resource: AxisCrudResource<IngestRunDto> = {
    title: '',
    entityName: '采集运行',
    permissions: dataHubPermissionCodes.ingestRun,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑采集运行「${String((model as Record<string, unknown>).name || '-')}」`
        : '创建采集运行',
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
            field: 'jobId',
            label: 'JobId',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入JobId' }
          },
          {
            field: 'name',
            label: 'Name',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 128,
              showCount: true,
              rows: 2,
              placeholder: '请输入Name'
            }
          },
          {
            field: 'code',
            label: 'Code',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入Code' }
          },
          {
            field: 'runStatus',
            label: 'RunStatus',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入RunStatus' }
          },
          {
            field: 'startedTime',
            label: 'StartedTime',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入StartedTime' }
          },
          {
            field: 'finishedTime',
            label: 'FinishedTime',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入FinishedTime' }
          },
          {
            field: 'rowCountRead',
            label: 'RowCountRead',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入RowCountRead' }
          },
          {
            field: 'rowCountChanged',
            label: 'RowCountChanged',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入RowCountChanged' }
          },
          {
            field: 'errorMessage',
            label: 'ErrorMessage',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 1024,
              showCount: true,
              rows: 2,
              placeholder: '请输入ErrorMessage'
            }
          },
          {
            field: 'watermark',
            label: 'Watermark',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 512,
              showCount: true,
              rows: 2,
              placeholder: '请输入Watermark'
            }
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
    request: getIngestRunPage,
    create: data => createIngestRun(data as unknown as SaveIngestRunInput),
    update: (id, data) => updateIngestRun(id, data as unknown as SaveIngestRunInput),
    remove: deleteIngestRun
  }

  return { resource }
}
