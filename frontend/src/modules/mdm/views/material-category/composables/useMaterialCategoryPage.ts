import { computed } from 'vue'

import type { MaterialCategoryDto, SaveMaterialCategoryInput } from '@/modules/mdm/api/material-category'
import {
  createMaterialCategory,
  deleteMaterialCategory,
  getMaterialCategoryList,
  updateMaterialCategory
} from '@/modules/mdm/api/material-category'
import { mdmPermissionCodes } from '@/modules/mdm/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

/** 物料分类维护页：树表 + 分区表单。 */
export function useMaterialCategoryPage() {
  const columns: AxisTableColumn<MaterialCategoryDto>[] = [
    { key: 'code', title: '分类编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '分类名称' },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 80 },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', width: 170, render: row => formatDateTime(row.createdTime) }
  ]

  const resource = computed(() => {
    const next: AxisCrudResource<MaterialCategoryDto> = {
      title: '',
      entityName: '分类',
      permissions: mdmPermissionCodes.materialCategory,
      columns,
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑分类「${String(model.name || '-')}」· 调整上级与基础信息`
          : '创建物料分类并设置上级归属',
      actionColumn: {
        width: 120,
        maxInline: 2
      },
      formSections: [
        {
          key: 'basic',
          title: '基础信息',
          icon: 'ant-design:apartment-outlined',
          columns: 2,
          fields: [
            {
              field: 'parentId',
              label: '上级分类',
              component: 'treeSelect',
              options: 'tree',
              defaultValue: '0'
            },
            {
              field: 'code',
              label: '分类编码',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '请输入分类编码' }
            },
            {
              field: 'name',
              label: '分类名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '请输入分类名称' }
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
      request: getMaterialCategoryList,
      create: data => createMaterialCategory(data as unknown as SaveMaterialCategoryInput),
      update: (id, data) => updateMaterialCategory(id, data as unknown as SaveMaterialCategoryInput),
      remove: deleteMaterialCategory,
      pagination: false,
      tree: true
    }
    return next
  })

  return { resource }
}
