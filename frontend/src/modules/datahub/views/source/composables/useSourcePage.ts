import type { SourceDto, SaveSourceInput } from '@/modules/datahub/api/source'
import {
  createSource,
  deleteSource,
  getSourcePage,
  updateSource
} from '@/modules/datahub/api/source'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormOption } from '@/shared/components/axis-form'

const sourceTypeOptions: AxisFormOption[] = [
  { label: 'SQL Server', value: 1 },
  { label: 'PostgreSQL', value: 2 },
  { label: 'MySQL', value: 3 },
  { label: 'HTTP', value: 4 }
]

const secretKindOptions: AxisFormOption[] = [
  { label: '应用配置', value: 'config' },
  { label: '环境变量', value: 'env' },
  { label: '暂不配置', value: 'none' }
]

const configKeyPresets: AxisFormOption[] = [
  { label: 'ConnectionStrings:U8Readonly（用友 U8 只读）', value: 'ConnectionStrings:U8Readonly' },
  { label: 'ConnectionStrings:ErpReadonly', value: 'ConnectionStrings:ErpReadonly' },
  { label: 'DataHub:Sources:Default', value: 'DataHub:Sources:Default' },
  { label: 'DataHub:Sources:Ods', value: 'DataHub:Sources:Ods' }
]

const envKeyPresets: AxisFormOption[] = [
  { label: 'DH_SOURCE_U8_READONLY', value: 'DH_SOURCE_U8_READONLY' },
  { label: 'DH_SOURCE_ERP_READONLY', value: 'DH_SOURCE_ERP_READONLY' },
  { label: 'DH_SOURCE_DEFAULT', value: 'DH_SOURCE_DEFAULT' }
]

type SecretKind = 'config' | 'env' | 'none'

function parseSecretRef(secretRef: unknown): {
  secretKind: SecretKind
  secretConfigKey: string
  secretEnvKey: string
} {
  const raw = String(secretRef ?? '').trim()
  if (!raw) {
    return {
      secretKind: 'none',
      secretConfigKey: 'ConnectionStrings:U8Readonly',
      secretEnvKey: 'DH_SOURCE_U8_READONLY'
    }
  }
  if (raw.startsWith('env:')) {
    return {
      secretKind: 'env',
      secretConfigKey: 'ConnectionStrings:U8Readonly',
      secretEnvKey: raw.slice(4)
    }
  }
  const key = raw.startsWith('config:') ? raw.slice(7) : raw
  return {
    secretKind: 'config',
    secretConfigKey: key,
    secretEnvKey: 'DH_SOURCE_U8_READONLY'
  }
}

function composeSecretRef(
  kind: unknown,
  configKey: unknown,
  envKey: unknown
): string | null {
  const k = String(kind ?? 'none') as SecretKind
  if (k === 'none') return null
  if (k === 'env') {
    const name = String(envKey ?? '').trim()
    return name ? `env:${name}` : null
  }
  const name = String(configKey ?? '').trim()
  return name ? `config:${name}` : null
}

function formatSecretRef(secretRef: unknown): string {
  const raw = String(secretRef ?? '').trim()
  if (!raw) return '—'
  if (raw.startsWith('config:')) return `配置 · ${raw.slice(7)}`
  if (raw.startsWith('env:')) return `环境变量 · ${raw.slice(4)}`
  return raw
}

export function useSourcePage() {
  const columns: AxisTableColumn<SourceDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'sourceType', title: '数据源类型', dict: sourceTypeOptions },
    {
      key: 'secretRef',
      title: '密钥引用',
      ellipsis: { tooltip: true },
      render: row => formatSecretRef(row.secretRef)
    },
    { key: 'isReadOnly', title: '只读' },
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

  const resource: AxisCrudResource<SourceDto> = {
    title: '',
    entityName: '数据源',
    permissions: dataHubPermissionCodes.source,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据源「${String((model as Record<string, unknown>).name || '-')}」`
        : '新增数据源',
    formWidth: 820,
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
            field: 'name',
            label: '名称',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入名称' }
          },
          {
            field: 'code',
            label: '编码',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入编码，如 u8-readonly' }
          },
          {
            field: 'sourceType',
            label: '数据源类型',
            component: 'select',
            options: sourceTypeOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'isReadOnly',
            label: '只读',
            component: 'switch',
            defaultValue: true,
            help: '生产建议开启，避免采集任务误写源库'
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
          }
        ]
      },
      {
        key: 'secret',
        title: '连接密钥',
        icon: 'ant-design:safety-certificate-outlined',
        description:
          '连接串不入库。先选来源，再选或输入键名；保存时自动生成引用（config:… / env:…）。',
        columns: 1,
        fields: [
          {
            field: 'secretKind',
            label: '密钥来源',
            component: 'radio',
            options: secretKindOptions,
            required: true,
            defaultValue: 'config',
            help: '推荐「应用配置」：真实连接串写在服务器 appsettings（或配置中心），这里只填配置键。'
          },
          {
            field: 'secretConfigKey',
            label: '配置键',
            component: 'select',
            options: configKeyPresets,
            required: true,
            defaultValue: 'ConnectionStrings:U8Readonly',
            ifShow: model => model.secretKind === 'config',
            componentProps: {
              filterable: true,
              tag: true,
              clearable: true,
              placeholder: '选择预设，或直接输入自定义配置键'
            },
            help: '例：在配置里增加 ConnectionStrings:U8Readonly = 真实连接串，这里选中该项即可。保存值为 config:配置键。'
          },
          {
            field: 'secretEnvKey',
            label: '环境变量名',
            component: 'select',
            options: envKeyPresets,
            required: true,
            defaultValue: 'DH_SOURCE_U8_READONLY',
            ifShow: model => model.secretKind === 'env',
            componentProps: {
              filterable: true,
              tag: true,
              clearable: true,
              placeholder: '选择预设，或直接输入变量名'
            },
            help: '在运行环境设置该变量为完整连接串。保存值为 env:变量名。'
          }
        ]
      }
    ],
    onFormOpen: ({ model }) => {
      const parsed = parseSecretRef(model.secretRef)
      model.secretKind = parsed.secretKind
      model.secretConfigKey = parsed.secretConfigKey
      model.secretEnvKey = parsed.secretEnvKey
    },
    beforeSubmit: data => {
      data.secretRef = composeSecretRef(
        data.secretKind,
        data.secretConfigKey,
        data.secretEnvKey
      )
      delete data.secretKind
      delete data.secretConfigKey
      delete data.secretEnvKey
      return data
    },
    request: getSourcePage,
    create: data => createSource(data as unknown as SaveSourceInput),
    update: (id, data) => updateSource(id, data as unknown as SaveSourceInput),
    remove: deleteSource
  }

  return { resource }
}
