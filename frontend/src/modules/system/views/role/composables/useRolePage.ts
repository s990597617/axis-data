import { computed, onMounted, ref } from 'vue'

import { getOrgTree, type OrgTreeNode } from '@/modules/system/api/org'
import type { RoleDto, SaveRoleInput } from '@/modules/system/api/role'
import {
  createRole,
  deleteRole,
  getRolePage,
  updateRole
} from '@/modules/system/api/role'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn, AxisTableRowAction } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'

import type RoleDetailDrawer from '../components/RoleDetailDrawer.vue'
import type RoleGrantDialog from '../components/RoleGrantDialog.vue'
import { dataScopeOptions } from '../utils/display'

export function useRolePage() {
  const grantDialogRef = ref<InstanceType<typeof RoleGrantDialog> | null>(null)
  const detailDrawerRef = ref<InstanceType<typeof RoleDetailDrawer> | null>(null)
  const orgOptions = ref<AxisFormOption[]>([])

  // 机构树 → treeSelect 选项
  function mapOrgOptions(nodes: OrgTreeNode[]): AxisFormOption[] {
    return nodes.map(node => ({
      key: node.id,
      label: node.name,
      children: node.children?.length ? mapOrgOptions(node.children) : undefined
    }))
  }

  async function loadOrgOptions() {
    const [data] = await tryRequest(getOrgTree())
    orgOptions.value = mapOrgOptions(data ?? [])
  }

  onMounted(() => {
    void loadOrgOptions()
  })

  const columns: AxisTableColumn<RoleDto>[] = [
    { key: 'code', title: '角色编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '角色名称' },
    { key: 'dataScope', title: '数据范围', dict: dataScopeOptions, dictRender: true, search: true },
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

  const detailAction: AxisTableRowAction<RoleDto> = {
    key: 'detail',
    label: '详情',
    icon: 'ant-design:eye-outlined',
    type: 'primary',
    permissions: [systemPermissionCodes.role.detail],
    onClick: row => detailDrawerRef.value?.open(row.id)
  }

  const grantAction: AxisTableRowAction<RoleDto> = {
    key: 'grant',
    label: '分配权限',
    icon: 'ant-design:safety-certificate-outlined',
    color: '#2080f0',
    permissions: [systemPermissionCodes.role.grant],
    ifShow: row => row.code !== 'super-admin',
    onClick: row => grantDialogRef.value?.open(row)
  }

  const resource = computed<AxisCrudResource<RoleDto>>(() => ({
    title: '',
    entityName: '角色',
    permissions: systemPermissionCodes.role,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑角色 @${String(model.code || '-')} · 调整基础信息与数据范围`
        : '创建角色并设置数据范围 · 权限可在列表中分配',
    actionColumn: {
      width: 168,
      maxInline: 4
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
            label: '角色编码',
            required: true,
            defaultValue: '',
            disabled: model => model.code === 'super-admin',
            componentProps: { maxlength: 64, placeholder: '请输入角色编码' }
          },
          {
            field: 'name',
            label: '角色名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入角色名称' }
          },
          {
            field: 'dataScope',
            label: '数据范围',
            component: 'select',
            options: dataScopeOptions,
            required: true,
            defaultValue: 5,
            componentProps: { placeholder: '请选择数据范围' }
          },
          {
            field: 'orgIds',
            label: '自定义机构',
            component: 'treeSelect',
            options: orgOptions.value,
            fullWidth: true,
            defaultValue: [],
            ifShow: model => Number(model.dataScope) === 2,
            required: true,
            help: '仅「自定义机构」数据范围时生效',
            componentProps: {
              multiple: true,
              checkable: true,
              cascade: false,
              filterable: true,
              maxTagCount: 3,
              keyField: 'key',
              labelField: 'label',
              childrenField: 'children',
              placeholder: '请选择可访问机构'
            }
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
    request: getRolePage,
    create: data => createRole(data as unknown as SaveRoleInput),
    update: (id, data) => updateRole(id, data as unknown as SaveRoleInput),
    remove: deleteRole,
    leadingActions: [detailAction],
    extraActions: [grantAction],
    canDelete: row => row.code !== 'super-admin',
    beforeSubmit: data => {
      if (Number(data.dataScope) !== 2) data.orgIds = []
      return data
    }
  }))

  return {
    grantDialogRef,
    detailDrawerRef,
    resource
  }
}
