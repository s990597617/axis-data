import type { ConfigDto, SaveConfigInput } from '@/modules/system/api/config'
import { createConfig, deleteConfig, getConfigPage, updateConfig } from '@/modules/system/api/config'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

import { configTypeOptions } from '../utils/display'

export function useConfigPage() {
  const columns: AxisTableColumn<ConfigDto>[] = [
    { key: 'code', title: '参数编码', ellipsis: { tooltip: true }, search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '参数名称' },
    { key: 'groupCode', title: '参数分组', search: true },
    { key: 'value', title: '参数值', ellipsis: { tooltip: true } },
    { key: 'type', title: '类型', dict: configTypeOptions, dictRender: true, search: true },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 80 },
    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }
  ]

  const resource: AxisCrudResource<ConfigDto> = {
    title: '',
    entityName: '参数',
    permissions: systemPermissionCodes.config,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑参数「${String(model.name || '-')}」· 调整取值与分组`
        : '创建系统参数并设置编码、分组与取值',
    actionColumn: {
      width: 120,
      maxInline: 2
    },
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:tool-outlined',
        columns: 2,
        fields: [
          {
            field: 'code',
            label: '参数编码',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入参数编码' }
          },
          {
            field: 'name',
            label: '参数名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入参数名称' }
          },
          {
            field: 'groupCode',
            label: '参数分组',
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入参数分组' }
          },
          {
            field: 'type',
            label: '参数类型',
            component: 'select',
            options: configTypeOptions,
            required: true,
            defaultValue: 1,
            componentProps: { placeholder: '请选择参数类型' }
          },
          {
            field: 'value',
            label: '参数值',
            component: 'textarea',
            fullWidth: true,
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 1024, rows: 4, placeholder: '请输入参数值' }
          }
        ]
      },
      {
        key: 'settings',
        title: '其他设置',
        icon: 'ant-design:setting-outlined',
        columns: 2,
        fields: [
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 9999, placeholder: '请输入排序' }
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
    request: getConfigPage,
    create: data => createConfig(data as unknown as SaveConfigInput),
    update: (id, data) => updateConfig(id, data as unknown as SaveConfigInput),
    remove: deleteConfig,
    canDelete: row => !row.isBuiltin
  }

  return { resource }
}
