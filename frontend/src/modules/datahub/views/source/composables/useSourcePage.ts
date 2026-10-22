import type { SourceDto, SaveSourceInput } from '@/modules/datahub/api/source'
import {
  createSource,
  deleteSource,
  getSourcePage,
  updateSource
} from '@/modules/datahub/api/source'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function useSourcePage() {
  const columns: AxisTableColumn<SourceDto>[] = [
    { key: 'name', title: 'Name', search: { field: 'keyword', placeholder: '关键字' } },
    { key: 'code', title: 'Code' },
    { key: 'sourceType', title: 'SourceType' },
    { key: 'secretRef', title: 'SecretRef', ellipsis: { tooltip: true } },
    { key: 'isReadOnly', title: 'IsReadOnly' },
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

  const resource: AxisCrudResource<SourceDto> = {
    title: '',
    entityName: '数据源',
    permissions: dataHubPermissionCodes.source,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据源「${String((model as Record<string, unknown>).name || '-')}」`
        : '创建数据源',
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
            field: 'sourceType',
            label: 'SourceType',
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入SourceType' }
          },
          {
            field: 'secretRef',
            label: 'SecretRef',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 128,
              showCount: true,
              rows: 2,
              placeholder: '请输入SecretRef'
            }
          },
          {
            field: 'isReadOnly',
            label: 'IsReadOnly',
            component: 'switch',
            defaultValue: false
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
    request: getSourcePage,
    create: data => createSource(data as unknown as SaveSourceInput),
    update: (id, data) => updateSource(id, data as unknown as SaveSourceInput),
    remove: deleteSource
  }

  return { resource }
}
