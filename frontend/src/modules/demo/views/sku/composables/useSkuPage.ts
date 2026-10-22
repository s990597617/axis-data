import type { SkuDto, SaveSkuInput } from '@/modules/demo/api/sku'
import {
  createSku,
  deleteSku,
  getSkuPage,
  updateSku
} from '@/modules/demo/api/sku'
import { demoPermissionCodes } from '@/modules/demo/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function useSkuPage() {
  const columns: AxisTableColumn<SkuDto>[] = [
    { key: 'code', title: '岗位编码', search: { field: 'keyword', placeholder: '关键字' } },
    { key: 'name', title: '岗位名称' },
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

  const resource: AxisCrudResource<SkuDto> = {
    title: '',
    entityName: 'SKU',
    permissions: demoPermissionCodes.sku,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑SKU「${String((model as Record<string, unknown>).name || '-')}」`
        : '创建SKU',
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
            required: false,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入岗位编码' }
          },
          {
            field: 'name',
            label: '岗位名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入岗位名称' }
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
          },
        ]
      }
    ],
    request: getSkuPage,
    create: data => createSku(data as unknown as SaveSkuInput),
    update: (id, data) => updateSku(id, data as unknown as SaveSkuInput),
    remove: deleteSku
  }

  return { resource }
}
