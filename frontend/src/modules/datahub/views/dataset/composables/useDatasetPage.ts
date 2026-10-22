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
import type { AxisFormOption } from '@/shared/components/axis-form'

const objectTypeOptions: AxisFormOption[] = [
  { label: '表', value: 1 },
  { label: '视图', value: 2 },
  { label: '查询', value: 3 }
]

const updateStrategyOptions: AxisFormOption[] = [
  { label: '全量', value: 1 },
  { label: '增量', value: 2 },
  { label: '差异检测', value: 3 }
]

export function useDatasetPage() {
  const columns: AxisTableColumn<DatasetDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'sourceId', title: '数据源Id', width: 110 },
    { key: 'objectType', title: '对象类型', dict: objectTypeOptions },
    { key: 'objectName', title: '对象名', ellipsis: { tooltip: true } },
    { key: 'primaryKeyFields', title: '主键字段', ellipsis: { tooltip: true } },
    { key: 'incrementalField', title: '增量字段' },
    { key: 'updateStrategy', title: '更新策略', dict: updateStrategyOptions },
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

  const resource: AxisCrudResource<DatasetDto> = {
    title: '',
    entityName: '数据集',
    permissions: dataHubPermissionCodes.dataset,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据集「${String((model as Record<string, unknown>).name || '-')}」`
        : '新增数据集',
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
            label: '数据源Id',
            component: 'inputNumber',
            required: true,
            defaultValue: 0,
            componentProps: { min: 1, placeholder: '请输入数据源 Id' }
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
            field: 'objectType',
            label: '对象类型',
            component: 'select',
            options: objectTypeOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'objectName',
            label: '对象名',
            defaultValue: '',
            componentProps: { maxlength: 256, placeholder: '表名/视图名' }
          },
          {
            field: 'customSql',
            label: '自定义SQL',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: { rows: 3, placeholder: '对象类型为查询时填写' }
          },
          {
            field: 'primaryKeyFields',
            label: '主键字段',
            defaultValue: '',
            componentProps: { maxlength: 256, placeholder: '多个字段用逗号分隔' }
          },
          {
            field: 'incrementalField',
            label: '增量字段',
            defaultValue: '',
            componentProps: { maxlength: 128, placeholder: '增量策略时填写' }
          },
          {
            field: 'updateStrategy',
            label: '更新策略',
            component: 'select',
            options: updateStrategyOptions,
            required: true,
            defaultValue: 1
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
    request: getDatasetPage,
    create: data => createDataset(data as unknown as SaveDatasetInput),
    update: (id, data) => updateDataset(id, data as unknown as SaveDatasetInput),
    remove: deleteDataset
  }

  return { resource }
}
