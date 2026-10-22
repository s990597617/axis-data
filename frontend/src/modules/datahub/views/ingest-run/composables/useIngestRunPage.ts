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
import type { AxisFormOption } from '@/shared/components/axis-form'

const runStatusOptions: AxisFormOption[] = [
  { label: '待执行', value: 1 },
  { label: '运行中', value: 2 },
  { label: '成功', value: 3 },
  { label: '失败', value: 4 },
  { label: '已取消', value: 5 }
]

export function useIngestRunPage() {
  const columns: AxisTableColumn<IngestRunDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'jobId', title: '任务Id', width: 100 },
    { key: 'runStatus', title: '运行状态', dict: runStatusOptions, search: true },
    { key: 'startedTime', title: '开始时间', render: row => formatDateTime(row.startedTime) },
    { key: 'finishedTime', title: '结束时间', render: row => formatDateTime(row.finishedTime) },
    { key: 'rowCountRead', title: '读取行数', width: 100 },
    { key: 'rowCountChanged', title: '变更行数', width: 100 },
    { key: 'watermark', title: '水位线', ellipsis: { tooltip: true } },
    { key: 'errorMessage', title: '错误信息', ellipsis: { tooltip: true } },
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

  const resource: AxisCrudResource<IngestRunDto> = {
    title: '',
    entityName: '采集运行',
    permissions: dataHubPermissionCodes.ingestRun,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑采集运行「${String((model as Record<string, unknown>).name || '-')}」`
        : '新增采集运行',
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
            label: '任务Id',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 1, placeholder: '请输入采集任务 Id' }
          },
          {
            field: 'name',
            label: '名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 128, placeholder: '请输入名称' }
          },
          {
            field: 'code',
            label: '编码',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入编码' }
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
            defaultValue: null,
            componentProps: { placeholder: '可选，ISO 时间' }
          },
          {
            field: 'finishedTime',
            label: '结束时间',
            defaultValue: null,
            componentProps: { placeholder: '可选，ISO 时间' }
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
            componentProps: {
              maxlength: 1024,
              showCount: true,
              rows: 2,
              placeholder: '失败时填写'
            }
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
    request: getIngestRunPage,
    create: data => createIngestRun(data as unknown as SaveIngestRunInput),
    update: (id, data) => updateIngestRun(id, data as unknown as SaveIngestRunInput),
    remove: deleteIngestRun
  }

  return { resource }
}
