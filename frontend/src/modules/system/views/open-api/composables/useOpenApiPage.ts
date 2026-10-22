import { computed, onMounted, ref } from 'vue'

import {
  createOpenApiIdentity,
  deleteOpenApiIdentity,
  getOpenApiIdentityPage,
  resetOpenApiSecret,
  updateOpenApiIdentity,
  type CreateOpenApiIdentityResult,
  type OpenApiIdentityDto,
  type SaveOpenApiIdentityInput
} from '@/modules/system/api/open-api-identity'
import { getTenantPage, type TenantDto } from '@/modules/system/api/tenant'
import { getUserPage, type UserDto } from '@/modules/system/api/user'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

function generateSecret() {
  const bytes = new Uint8Array(24)
  crypto.getRandomValues(bytes)
  return Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('')
}

function showSecretOnce(accessKey: string, accessSecret: string, title: string) {
  dialog.info({
    title,
    content: `身份标识：${accessKey}\n密钥（仅显示一次，请立即保存）：\n${accessSecret}`,
    positiveText: '我已保存'
  })
}

function unwrapPageItems<T>(data: unknown): T[] {
  if (!data || typeof data !== 'object') return []
  const source = data as Record<string, unknown>
  const raw = source.items ?? source.Items ?? source.list ?? source.List
  return Array.isArray(raw) ? (raw as T[]) : []
}

export function useOpenApiPage() {
  const usageVisible = ref(false)
  const tenantOptions = ref<AxisFormOption[]>([])
  const userOptions = ref<AxisFormOption[]>([])

  async function loadTenantOptions() {
    const [data] = await tryRequest(getTenantPage({ page: 1, pageSize: 500 }))
    const items = unwrapPageItems<TenantDto>(data)
    tenantOptions.value = [
      { label: '平台（不绑定租户）', value: '' },
      ...items.map(item => ({
        label: `${item.name}（${item.code}）`,
        value: String(item.id)
      }))
    ]
  }

  async function loadUserOptions() {
    const [data] = await tryRequest(getUserPage({ page: 1, pageSize: 500 }))
    const items = unwrapPageItems<UserDto>(data)
    userOptions.value = items.map(item => ({
      label: `${item.realName || item.account}（${item.account}）`,
      value: String(item.id)
    }))
  }

  onMounted(() => {
    void Promise.all([loadTenantOptions(), loadUserOptions()])
  })

  const columns: AxisTableColumn<OpenApiIdentityDto>[] = [
    {
      key: 'accessKey',
      title: '身份标识',
      search: { field: 'keyword', placeholder: 'AccessKey 或备注' }
    },
    {
      key: 'userAccount',
      title: '绑定用户',
      render: row =>
        row.userRealName
          ? `${row.userRealName}（${row.userAccount || '-'}）`
          : row.userAccount || String(row.userId)
    },
    {
      key: 'tenantName',
      title: '绑定租户',
      render: row => row.tenantName || (row.tenantId ? String(row.tenantId) : '平台')
    },
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    {
      key: 'createdTime',
      title: '创建时间',
      render: row => formatDateTime(row.createdTime)
    }
  ]

  const resource = computed<AxisCrudResource<OpenApiIdentityDto>>(() => ({
    title: '',
    entityName: '开放接口',
    permissions: systemPermissionCodes.openApi,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑身份「${String(model.accessKey || '-')}」· 密钥请用重置操作更换`
        : '创建开放接口；密钥可自动生成，创建成功后仅展示一次',
    actionColumn: {
      width: 180,
      maxInline: 2
    },
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:api-outlined',
        columns: 2,
        fields: [
          {
            field: 'accessKey',
            label: '身份标识',
            required: true,
            defaultValue: '',
            disabled: (_model, editing) => editing,
            componentProps: {
              maxlength: 64,
              placeholder: 'AccessKey，字母数字_-'
            }
          },
          {
            field: 'accessSecret',
            label: '密钥',
            defaultValue: '',
            ifShow: model => !model.id,
            component: 'custom',
            componentProps: {
              maxlength: 128,
              placeholder: '可留空自动生成，或点击生成'
            }
          },
          {
            field: 'userId',
            label: '绑定用户',
            component: 'select',
            options: userOptions.value,
            required: true,
            defaultValue: null,
            componentProps: {
              filterable: true,
              clearable: false,
              placeholder: '请选择绑定用户'
            }
          },
          {
            field: 'tenantId',
            label: '绑定租户',
            component: 'select',
            options: tenantOptions.value,
            defaultValue: '',
            componentProps: {
              filterable: true,
              clearable: true,
              placeholder: '可空，表示平台级'
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
    request: getOpenApiIdentityPage,
    create: async data => {
      const payload = { ...(data as unknown as SaveOpenApiIdentityInput) }
      if (!payload.accessSecret) payload.accessSecret = generateSecret()
      if (!payload.tenantId) payload.tenantId = null
      const [result, error] = await tryRequest(createOpenApiIdentity(payload))
      if (error) throw error
      const created = result as CreateOpenApiIdentityResult | null
      if (created?.accessSecret) {
        showSecretOnce(created.accessKey, created.accessSecret, '创建成功')
      } else {
        message.success('创建成功')
      }
    },
    update: (id, data) => {
      const payload = { ...(data as unknown as SaveOpenApiIdentityInput) }
      if (!payload.tenantId) payload.tenantId = null
      return updateOpenApiIdentity(id, payload)
    },
    remove: deleteOpenApiIdentity,
    extraActions: [
      {
        key: 'resetSecret',
        label: '重置密钥',
        icon: 'ant-design:key-outlined',
        permissions: [systemPermissionCodes.openApi.resetSecret],
        confirm: row =>
          `确定重置「${(row as OpenApiIdentityDto).accessKey}」的密钥吗？旧密钥立即失效。`,
        onClick: async row => {
          const item = row as OpenApiIdentityDto
          const [data, error] = await tryRequest(resetOpenApiSecret(String(item.id)))
          if (error || !data) return
          showSecretOnce(data.accessKey, data.accessSecret, '密钥已重置')
        }
      }
    ]
  }))

  return {
    resource,
    usageVisible,
    generateSecret
  }
}
