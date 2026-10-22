import { ref } from 'vue'

import type { SaveTenantInput, TenantDto } from '@/modules/system/api/tenant'
import { createTenant, deleteTenant, getTenantPage, updateTenant } from '@/modules/system/api/tenant'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn, AxisTableRowAction } from '@/shared/components/axis-table'

import type TenantGrantDialog from '../components/TenantGrantDialog.vue'
import { tenantTypeOptions } from '../utils/display'

export function useTenantPage() {
  const grantDialogRef = ref<InstanceType<typeof TenantGrantDialog> | null>(null)

  const columns: AxisTableColumn<TenantDto>[] = [
    { key: 'code', title: '租户编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '租户名称' },
    { key: 'type', title: '隔离类型', dict: tenantTypeOptions, dictRender: true, search: true },
    {
      key: 'expireTime',
      title: '到期日期',
      render: row => row.expireTime?.substring(0, 10) ?? '长期'
    },
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

  const grantAction: AxisTableRowAction<TenantDto> = {
    key: 'grant',
    label: '分配菜单',
    icon: 'ant-design:apartment-outlined',
    color: '#2080f0',
    permissions: [systemPermissionCodes.tenant.grant],
    onClick: row => grantDialogRef.value?.open(row)
  }

  const resource: AxisCrudResource<TenantDto> = {
    title: '',
    entityName: '租户',
    permissions: systemPermissionCodes.tenant,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑租户「${String(model.name || '-')}」· 调整隔离方式与有效期`
        : '创建租户并设置隔离类型与到期时间；将自动开通管理员与默认菜单',
    actionColumn: {
      width: 180,
      maxInline: 3
    },
    extraActions: [grantAction],
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:cloud-outlined',
        columns: 2,
        fields: [
          {
            field: 'code',
            label: '租户编码',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入租户编码' }
          },
          {
            field: 'name',
            label: '租户名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入租户名称' }
          },
          {
            field: 'type',
            label: '隔离类型',
            component: 'select',
            options: tenantTypeOptions,
            required: true,
            defaultValue: 1,
            componentProps: { placeholder: '请选择隔离类型' }
          },
          {
            field: 'dbConfigId',
            label: '数据库配置编号',
            defaultValue: '',
            help: '仅独立数据库模式必填，须与启动配置 ConnectionConfigs.ConfigId 一致',
            ifShow: model => model.type === 3,
            componentProps: { maxlength: 64, placeholder: '请输入数据库配置编号' }
          },
          {
            field: 'adminUserId',
            label: '管理员用户编号',
            defaultValue: '',
            help: '留空则自动创建 {编码}_admin 管理员',
            componentProps: { placeholder: '可选，留空自动创建' }
          },
          {
            field: 'expireTime',
            label: '到期日期',
            component: 'date',
            defaultValue: null
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
    request: getTenantPage,
    create: data => createTenant(data as unknown as SaveTenantInput),
    update: (id, data) => updateTenant(id, data as unknown as SaveTenantInput),
    remove: deleteTenant,
    beforeSubmit: data => ({
      ...data,
      dbConfigId: data.type === 3 ? data.dbConfigId : null,
      adminUserId: data.adminUserId || null,
      expireTime: data.expireTime || null
    })
  }

  return { resource, grantDialogRef }
}
