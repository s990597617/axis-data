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
import type { AxisFormOption } from '@/shared/components/axis-form'

const sourceTypeOptions: AxisFormOption[] = [
  { label: 'SQL Server', value: 1 },
  { label: 'PostgreSQL', value: 2 },
  { label: 'MySQL', value: 3 },
  { label: 'HTTP', value: 4 }
]

export function useSourcePage() {
  const columns: AxisTableColumn<SourceDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'sourceType', title: '数据源类型', dict: sourceTypeOptions },
    { key: 'secretRef', title: '密钥引用', ellipsis: { tooltip: true } },
    { key: 'isReadOnly', title: '只读' },
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

  const resource: AxisCrudResource<SourceDto> = {
    title: '',
    entityName: '数据源',
    permissions: dataHubPermissionCodes.source,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据源「${String((model as Record<string, unknown>).name || '-')}」`
        : '新增数据源',
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
            field: 'sourceType',
            label: '数据源类型',
            component: 'select',
            options: sourceTypeOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'secretRef',
            label: '密钥引用',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 128,
              showCount: true,
              rows: 2,
              placeholder: '密钥/连接配置引用，勿填明文连接串'
            }
          },
          {
            field: 'isReadOnly',
            label: '只读',
            component: 'switch',
            defaultValue: true
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
    request: getSourcePage,
    create: data => createSource(data as unknown as SaveSourceInput),
    update: (id, data) => updateSource(id, data as unknown as SaveSourceInput),
    remove: deleteSource
  }

  return { resource }
}
