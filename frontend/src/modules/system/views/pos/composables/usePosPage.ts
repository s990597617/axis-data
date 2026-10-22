import type { PositionDto, SavePositionInput } from '@/modules/system/api/position'
import {
  createPosition,
  deletePosition,
  getPositionPage,
  updatePosition
} from '@/modules/system/api/position'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function usePosPage() {
  const columns: AxisTableColumn<PositionDto>[] = [
    { key: 'code', title: '岗位编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '岗位名称' },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 80 },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }
  ]

  const resource: AxisCrudResource<PositionDto> = {
    title: '',
    entityName: '岗位',
    permissions: systemPermissionCodes.position,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑岗位「${String(model.name || '-')}」· 调整编码与状态`
        : '创建岗位并设置编码与排序',
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
            field: 'code',
            label: '岗位编码',
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入岗位编码' }
          },
          {
            field: 'name',
            label: '岗位名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入岗位名称' }
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
    request: getPositionPage,
    create: data => createPosition(data as unknown as SavePositionInput),
    update: (id, data) => updatePosition(id, data as unknown as SavePositionInput),
    remove: deletePosition
  }

  return { resource }
}
