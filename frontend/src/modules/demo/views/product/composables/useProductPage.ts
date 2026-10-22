import type { ProductDto, SaveProductInput } from '@/modules/demo/api/product'
import {
  createProduct,
  deleteProduct,
  getProductPage,
  updateProduct
} from '@/modules/demo/api/product'
import { demoPermissionCodes } from '@/modules/demo/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function useProductPage() {
  const columns: AxisTableColumn<ProductDto>[] = [
    { key: 'code', title: '产品编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '产品名称' },
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

  const resource: AxisCrudResource<ProductDto> = {
    title: '',
    entityName: '产品',
    permissions: demoPermissionCodes.product,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑产品「${String(model.name || '-')}」· 调整编码与状态`
        : '创建产品并设置编码与排序',
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
            label: '产品编码',
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入产品编码' }
          },
          {
            field: 'name',
            label: '产品名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入产品名称' }
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
    request: getProductPage,
    create: data => createProduct(data as unknown as SaveProductInput),
    update: (id, data) => updateProduct(id, data as unknown as SaveProductInput),
    remove: deleteProduct
  }

  return { resource }
}
