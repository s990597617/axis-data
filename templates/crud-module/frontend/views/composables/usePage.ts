import type { __Module__Dto, Save__Module__Input } from '@/modules/__area__/api/__module_kebab__'
import {
  create__Module__,
  delete__Module__,
  get__Module__Page,
  update__Module__
} from '@/modules/__area__/api/__module_kebab__'
import { __permissionCodesVar__ } from '@/modules/__area__/permissions/codes'
import {
  formatDateTime,
__FIELD_FE_STATUS_IMPORT__  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'

export function use__Module__Page() {
  const columns: AxisTableColumn<__Module__Dto>[] = [
__FIELD_FE_COLUMNS__
  ]

  const resource: AxisCrudResource<__Module__Dto> = {
    title: '',
    entityName: '__CnName__',
    permissions: __permissionCodesVar__.__module__,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑__CnName__「${String((model as Record<string, unknown>).name || '-')}」`
        : '创建__CnName__',
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
__FIELD_FE_FORM_FIELDS__
        ]
      }
    ],
    request: get__Module__Page,
    create: data => create__Module__(data as unknown as Save__Module__Input),
    update: (id, data) => update__Module__(id, data as unknown as Save__Module__Input),
    remove: delete__Module__
  }

  return { resource }
}
