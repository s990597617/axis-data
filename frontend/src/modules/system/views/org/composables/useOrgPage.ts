import { computed } from 'vue'

import type { OrgDto, SaveOrgInput } from '@/modules/system/api/org'
import { createOrg, deleteOrg, getOrgList, updateOrg } from '@/modules/system/api/org'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { loadDictOptions, useDict } from '@/shared/composables/use-dict'

/** 机构模块使用的系统字典编码（与 SysDictTypeSeed 对齐）。 */
const orgDictCodes = {
  /** 机构类型：公司 / 部门 / 小组等。 */
  type: 'org_type',
  /** 通用启用/禁用状态。 */
  status: 'common_status'
} as const

/** 机构维护页：树表 + 分区表单；类型/状态走系统字典缓存。 */
export function useOrgPage() {
  // 表单选项：useDict 自动拉取并与表格 dict 共享 loadDictOptions 缓存。
  const { options: orgTypeOptions } = useDict(orgDictCodes.type)
  const { options: statusDictOptions } = useDict(orgDictCodes.status)

  /** 树表列：类型/状态用 dict 渲染与搜索；状态列额外用 renderStatus 着色。 */
  const columns: AxisTableColumn<OrgDto>[] = [
    { key: 'code', title: '机构编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '机构名称' },
    {
      key: 'type',
      title: '机构类型',
      dict: () => loadDictOptions(orgDictCodes.type),
      dictRender: true,
      search: true
    },
    {
      key: 'status',
      title: '状态',
      dict: () => loadDictOptions(orgDictCodes.status),
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 80 },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }
  ]

  // computed：依赖 orgTypeOptions / statusDictOptions，字典到达后表单选项会刷新。
  const resource = computed(() => {
    const next: AxisCrudResource<OrgDto> = {
      title: '',
      entityName: '机构',
      permissions: systemPermissionCodes.org,
      columns,
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑机构「${String(model.name || '-')}」· 调整归属与基础信息`
          : '创建机构并设置上级归属与类型',
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
              label: '上级机构',
              component: 'treeSelect',
              // 'tree'：由 AxisCrud 注入当前机构树，根节点用 '0'。
              options: 'tree',
              defaultValue: '0'
            },
            {
              field: 'code',
              label: '机构编码',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '请输入机构编码' }
            },
            {
              field: 'name',
              label: '机构名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '请输入机构名称' }
            },
            {
              field: 'type',
              label: '机构类型',
              component: 'select',
              // SelectOption 与 AxisFormOption 结构兼容，显式断言满足字段类型。
              options: orgTypeOptions.value as AxisFormOption[],
              required: true,
              // 默认「部门」类取值，需与 org_type 字典 value 对齐。
              defaultValue: 3,
              componentProps: { placeholder: '请选择机构类型' }
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
              options: statusDictOptions.value as AxisFormOption[],
              required: true,
              // common_status：1=启用。
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
      request: getOrgList,
      create: data => createOrg(data as unknown as SaveOrgInput),
      update: (id, data) => updateOrg(id, data as unknown as SaveOrgInput),
      remove: deleteOrg,
      pagination: false,
      tree: true
    }
    return next
  })

  return { resource }
}
