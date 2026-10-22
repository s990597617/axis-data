import type { DatasetDto, SaveDatasetInput } from '@/modules/datahub/api/dataset'
import {
  createDataset,
  deleteDataset,
  getDatasetPage,
  updateDataset
} from '@/modules/datahub/api/dataset'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function useDatasetPage() {
  const columns: AxisTableColumn<DatasetDto>[] = [
    { key: 'sourceId', title: 'SourceId' },
    { key: 'name', title: 'Name', search: { field: 'keyword', placeholder: '关键字' } },
    { key: 'code', title: 'Code' },
    { key: 'objectType', title: 'ObjectType' },
    { key: 'objectName', title: 'ObjectName', ellipsis: { tooltip: true } },
    { key: 'customSql', title: 'CustomSql' },
    { key: 'primaryKeyFields', title: 'PrimaryKeyFields', ellipsis: { tooltip: true } },
    { key: 'incrementalField', title: 'IncrementalField', ellipsis: { tooltip: true } },
    { key: 'updateStrategy', title: 'UpdateStrategy' },
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

  const resource: AxisCrudResource<DatasetDto> = {
    title: '',
    entityName: '数据集',
    permissions: dataHubPermissionCodes.dataset,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据集「${String((model as Record<string, unknown>).name || '-')}」`
        : '创建数据集',
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
            field: 'sourceId',
            label: 'SourceId',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 0, max: 999999, placeholder: '请输入SourceId' }
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
            field: 'objectType',
            label: 'ObjectType',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入ObjectType' }
          },
          {
            field: 'objectName',
            label: 'ObjectName',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 256,
              showCount: true,
              rows: 2,
              placeholder: '请输入ObjectName'
            }
          },
          {
            field: 'customSql',
            label: 'CustomSql',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入CustomSql' }
          },
          {
            field: 'primaryKeyFields',
            label: 'PrimaryKeyFields',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 256,
              showCount: true,
              rows: 2,
              placeholder: '请输入PrimaryKeyFields'
            }
          },
          {
            field: 'incrementalField',
            label: 'IncrementalField',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 128,
              showCount: true,
              rows: 2,
              placeholder: '请输入IncrementalField'
            }
          },
          {
            field: 'updateStrategy',
            label: 'UpdateStrategy',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入UpdateStrategy' }
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
    request: getDatasetPage,
    create: data => createDataset(data as unknown as SaveDatasetInput),
    update: (id, data) => updateDataset(id, data as unknown as SaveDatasetInput),
    remove: deleteDataset
  }

  return { resource }
}
