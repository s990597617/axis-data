import { h, ref } from 'vue'
import { NTag } from 'naive-ui'
import type {
  SourceDto,
  SaveSourceInput,
  SecretRefOptionItem,
  TestSourceConnectionInput
} from '@/modules/datahub/api/source'
import {
  createSource,
  deleteSource,
  getSourcePage,
  listSecretRefOptions,
  testSourceById,
  testSourceConnection,
  updateSource
} from '@/modules/datahub/api/source'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  AxisCrudPage,
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormFieldContext, AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'
import { message } from '@/shared/utils/message'

const sourceTypeOptions: AxisFormOption[] = [
  { label: 'PostgreSQL', value: 2 },
  { label: 'MySQL', value: 3 },
  { label: 'SQL Server', value: 1 },
  { label: 'HTTP API', value: 4 }
]

const sourceTypeDict: AxisFormOption[] = [
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

const connectionModeOptions: AxisFormOption[] = [
  { label: '密钥引用（推荐）', value: 'secret' },
  { label: '结构化连接串', value: 'inline' }
]

const lastTestStatusOptions: AxisFormOption[] = [
  { label: '未测试', value: 0 },
  { label: '成功', value: 1 },
  { label: '失败', value: 2 }
]

type SecretKind = 'config' | 'env' | 'none'
type ConnectionMode = 'secret' | 'inline'

function toFormOptions(items: SecretRefOptionItem[] | undefined): AxisFormOption[] {
  return (items ?? []).map(item => ({
    label: item.label,
    value: item.value
  }))
}

function parseSecretRef(secretRef: unknown): {
  secretKind: SecretKind
  secretConfigKey: string
  secretEnvKey: string
} {
  const raw = String(secretRef ?? '').trim()
  if (!raw) {
    return { secretKind: 'none', secretConfigKey: '', secretEnvKey: '' }
  }
  if (raw.startsWith('env:')) {
    return { secretKind: 'env', secretConfigKey: '', secretEnvKey: raw.slice(4) }
  }
  const key = raw.startsWith('config:') ? raw.slice(7) : raw
  return { secretKind: 'config', secretConfigKey: key, secretEnvKey: '' }
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

function defaultPort(sourceType: unknown): number {
  const t = Number(sourceType)
  if (t === 2) return 5432
  if (t === 3) return 3306
  if (t === 1) return 1433
  return 0
}

function isHttpType(sourceType: unknown): boolean {
  return Number(sourceType) === 4
}

function isDbType(sourceType: unknown): boolean {
  const t = Number(sourceType)
  return t === 1 || t === 2 || t === 3
}

function formatConnection(row: SourceDto): string {
  if (row.hasConnectionString) return '连接串已配置'
  const raw = String(row.secretRef ?? '').trim()
  if (!raw) return '未配置'
  if (raw.startsWith('config:')) return `配置密钥 · ${raw.slice(7)}`
  if (raw.startsWith('env:')) return `环境变量 · ${raw.slice(4)}`
  return '配置密钥'
}

function renderLastTestStatus(status: unknown) {
  const value = Number(status ?? 0)
  if (value === 1) {
    return h(NTag, { type: 'success', size: 'small', bordered: false }, { default: () => '成功' })
  }
  if (value === 2) {
    return h(NTag, { type: 'error', size: 'small', bordered: false }, { default: () => '失败' })
  }
  return h(NTag, { type: 'default', size: 'small', bordered: false }, { default: () => '未测试' })
}

function buildInlineConnectionString(data: Record<string, unknown>): string | null {
  const sourceType = Number(data.sourceType)
  if (sourceType === 4) {
    const baseUrl = String(data.baseUrl ?? '').trim()
    return baseUrl || null
  }

  const host = String(data.host ?? '').trim()
  const database = String(data.database ?? '').trim()
  const username = String(data.username ?? '').trim()
  const password = String(data.password ?? '')
  if (!host || !database) return null

  const portRaw = data.port
  const port =
    portRaw === null || portRaw === undefined || portRaw === ''
      ? defaultPort(sourceType)
      : Number(portRaw)

  if (sourceType === 2) {
    return `Host=${host};Port=${port};Database=${database};Username=${username};Password=${password}`
  }
  if (sourceType === 3) {
    return `Server=${host};Port=${port};Database=${database};Uid=${username};Pwd=${password};CharSet=utf8mb4`
  }
  if (sourceType === 1) {
    return `Server=${host},${port};Database=${database};User Id=${username};Password=${password};TrustServerCertificate=True`
  }
  return null
}

function buildTestPayload(data: Record<string, unknown>): TestSourceConnectionInput {
  const sourceType = Number(data.sourceType)
  const mode = String(data.connectionMode ?? 'secret') as ConnectionMode

  if (mode === 'inline') {
    const connectionString = buildInlineConnectionString(data)
    return {
      sourceType,
      connectionString: connectionString ?? undefined,
      host: String(data.host ?? '').trim() || undefined,
      port:
        data.port === null || data.port === undefined || data.port === ''
          ? null
          : Number(data.port),
      database: String(data.database ?? '').trim() || undefined,
      username: String(data.username ?? '').trim() || undefined,
      password: String(data.password ?? '') || undefined,
      baseUrl: String(data.baseUrl ?? '').trim() || undefined
    }
  }

  return {
    sourceType,
    secretRef: composeSecretRef(data.secretKind, data.secretConfigKey, data.secretEnvKey)
  }
}

function scrubVirtualFields(data: Record<string, unknown>) {
  delete data.connectionMode
  delete data.host
  delete data.port
  delete data.database
  delete data.username
  delete data.password
  delete data.baseUrl
  delete data.secretKind
  delete data.secretConfigKey
  delete data.secretEnvKey
  delete data.runTest
  delete data.testHint
  delete data.hasConnectionString
  delete data.lastTestStatus
  delete data.lastTestTime
  delete data.lastTestMessage
}

export function useSourcePage() {
  const pageRef = ref<InstanceType<typeof AxisCrudPage> | null>(null)
  const configKeyOptions = ref<AxisFormOption[]>([])
  const envKeyOptions = ref<AxisFormOption[]>([])

  async function loadSecretOptions() {
    try {
      const data = await listSecretRefOptions()
      configKeyOptions.value = toFormOptions(data?.configKeys)
      envKeyOptions.value = toFormOptions(data?.envKeys)
    } catch {
      configKeyOptions.value = []
      envKeyOptions.value = []
    }
  }

  async function handleTestRow(row: SourceDto) {
    const [result, error] = await tryRequest(testSourceById(row.id), {
      fallbackMessage: '连通性测试失败'
    })
    if (error) return
    if (result?.success) {
      message.success(result.message || '连接成功')
    } else {
      message.error(result?.message || '连接失败')
    }
    await pageRef.value?.reload()
  }

  const columns: AxisTableColumn<SourceDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    { key: 'sourceType', title: '类型', dict: sourceTypeDict, width: 110 },
    {
      key: 'secretRef',
      title: '连接',
      ellipsis: { tooltip: true },
      minWidth: 160,
      render: row => formatConnection(row)
    },
    {
      key: 'lastTestStatus',
      title: '连通性',
      width: 90,
      dict: lastTestStatusOptions,
      render: row => renderLastTestStatus(row.lastTestStatus)
    },
    {
      key: 'lastTestTime',
      title: '最近测试',
      width: 170,
      render: row => formatDateTime(row.lastTestTime)
    },
    { key: 'isReadOnly', title: '只读', width: 70 },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', width: 170, render: row => formatDateTime(row.createdTime) }
  ]

  const resource: AxisCrudResource<SourceDto> = {
    title: '',
    entityName: '数据源',
    permissions: dataHubPermissionCodes.source,
    columns,
    formPresentation: 'drawer',
    formWidth: 760,
    formExtraClass: 'dh-source-form-drawer',
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑「${String((model as Record<string, unknown>).name || '-')}」`
        : '新建数据源',
    formSteps: [
      { key: 'type', title: '选择类型' },
      { key: 'connect', title: '配置连接' },
      { key: 'test', title: '测试连通' },
      { key: 'basic', title: '基本信息' }
    ],
    actionColumn: {
      width: 180,
      maxInline: 3
    },
    extraActions: [
      {
        key: 'test',
        label: '测试连接',
        icon: 'ant-design:api-outlined',
        type: 'primary',
        permissions: [dataHubPermissionCodes.source.edit],
        onClick: row => handleTestRow(row as SourceDto)
      }
    ],
    formSections: [
      {
        key: 'type',
        title: '选择类型',
        icon: 'ant-design:appstore-outlined',
        description: '先选类型，后续连接字段会随之变化。',
        columns: 1,
        fields: [
          {
            field: 'sourceType',
            label: '数据源类型',
            component: 'radio',
            options: sourceTypeOptions,
            required: true,
            defaultValue: 2,
            help: '关系库可拉表/字段元数据；HTTP 请在数据集中用「查询」+ SQL。',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              onUpdateValue: (value: number) => {
                if (isDbType(value)) {
                  ctx.model.port = defaultPort(value)
                }
              }
            })) as unknown as Record<string, unknown>
          }
        ]
      },
      {
        key: 'connect',
        title: '配置连接',
        icon: 'ant-design:link-outlined',
        description: '推荐密钥引用；也可临时填写结构化连接信息。',
        columns: 2,
        fields: [
          {
            field: 'connectionMode',
            label: '连接方式',
            component: 'radio',
            options: connectionModeOptions,
            required: true,
            defaultValue: 'secret',
            fullWidth: true,
            help: '密钥引用：真实连接串写在服务器配置，这里只选配置键。'
          },
          {
            field: 'secretKind',
            label: '密钥来源',
            component: 'radio',
            options: secretKindOptions,
            required: true,
            defaultValue: 'config',
            fullWidth: true,
            ifShow: model => model.connectionMode === 'secret'
          },
          {
            field: 'secretConfigKey',
            label: '配置键',
            component: 'select',
            options: () => configKeyOptions.value,
            required: true,
            defaultValue: '',
            fullWidth: true,
            ifShow: model => model.connectionMode === 'secret' && model.secretKind === 'config',
            componentProps: {
              filterable: true,
              tag: true,
              clearable: true,
              placeholder: '从配置加载，或输入新键如 ConnectionStrings:MyDb'
            },
            help: '在 14-DataHub.json 等配置中增加键后，重启 API 即可出现在列表。'
          },
          {
            field: 'secretEnvKey',
            label: '环境变量名',
            component: 'select',
            options: () => envKeyOptions.value,
            required: true,
            defaultValue: '',
            fullWidth: true,
            ifShow: model => model.connectionMode === 'secret' && model.secretKind === 'env',
            componentProps: {
              filterable: true,
              tag: true,
              clearable: true,
              placeholder: '选择已有 DH_SOURCE_* 变量，或输入新变量名'
            },
            help: '仅自动列出以 DH_SOURCE_ 开头的环境变量。'
          },
          {
            field: 'host',
            label: '主机',
            defaultValue: '',
            ifShow: model => model.connectionMode === 'inline' && isDbType(model.sourceType),
            componentProps: { maxlength: 256, placeholder: '如 127.0.0.1' }
          },
          {
            field: 'port',
            label: '端口',
            component: 'inputNumber',
            defaultValue: 5432,
            ifShow: model => model.connectionMode === 'inline' && isDbType(model.sourceType),
            componentProps: { min: 1, max: 65535, placeholder: '端口' }
          },
          {
            field: 'database',
            label: '数据库',
            defaultValue: '',
            ifShow: model => model.connectionMode === 'inline' && isDbType(model.sourceType),
            componentProps: { maxlength: 128, placeholder: '数据库名' }
          },
          {
            field: 'username',
            label: '用户名',
            defaultValue: '',
            ifShow: model => model.connectionMode === 'inline' && isDbType(model.sourceType),
            componentProps: { maxlength: 128, placeholder: '登录用户' }
          },
          {
            field: 'password',
            label: '密码',
            component: 'password',
            defaultValue: '',
            fullWidth: true,
            ifShow: model => model.connectionMode === 'inline' && isDbType(model.sourceType),
            componentProps: { maxlength: 256, placeholder: '编辑时请重新填写密码' },
            help: '密码不会回显；保存后列表仅显示「连接串已配置」。'
          },
          {
            field: 'baseUrl',
            label: '基地址',
            defaultValue: '',
            fullWidth: true,
            ifShow: model => model.connectionMode === 'inline' && isHttpType(model.sourceType),
            componentProps: {
              maxlength: 1024,
              placeholder: 'https://api.example.com'
            },
            help: '须为绝对 http/https URL。'
          }
        ]
      },
      {
        key: 'test',
        title: '测试连通',
        icon: 'ant-design:api-outlined',
        description: '建议保存前先测通，失败会阻止保存。',
        columns: 1,
        fields: [
          {
            field: 'runTest',
            label: '保存前先测试连接',
            component: 'switch',
            defaultValue: true,
            help: '开启后保存前会先调用测试接口；失败则阻止保存。'
          },
          {
            field: 'testHint',
            label: '最近测试结果',
            component: 'textarea',
            defaultValue: '',
            disabled: () => true,
            componentProps: { rows: 3, placeholder: '保存或行内「测试连接」后可在此看到说明' }
          }
        ]
      },
      {
        key: 'basic',
        title: '基本信息',
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
      }
    ],
    onFormOpen: async ({ model }) => {
      await loadSecretOptions()
      const parsed = parseSecretRef(model.secretRef)
      model.secretKind = parsed.secretKind
      model.secretConfigKey = parsed.secretConfigKey
      model.secretEnvKey = parsed.secretEnvKey

      const hasCs = Boolean(model.hasConnectionString)
      if (hasCs) {
        model.connectionMode = 'inline'
      } else if (parsed.secretKind !== 'none') {
        model.connectionMode = 'secret'
      } else {
        model.connectionMode = 'secret'
      }

      model.host = ''
      model.database = ''
      model.username = ''
      model.password = ''
      model.baseUrl = ''
      model.port = defaultPort(model.sourceType)
      model.runTest = true

      const lastMsg = String(model.lastTestMessage ?? '').trim()
      const lastStatus = Number(model.lastTestStatus ?? 0)
      if (lastMsg) {
        const tag = lastStatus === 1 ? '成功' : lastStatus === 2 ? '失败' : '未测试'
        const time = model.lastTestTime ? formatDateTime(model.lastTestTime as string) : ''
        model.testHint = `[${tag}] ${lastMsg}${time ? `（${time}）` : ''}`
      } else {
        model.testHint = ''
      }

      if (
        parsed.secretConfigKey &&
        !configKeyOptions.value.some(o => o.value === parsed.secretConfigKey)
      ) {
        configKeyOptions.value = [
          { label: parsed.secretConfigKey, value: parsed.secretConfigKey },
          ...configKeyOptions.value
        ]
      }
      if (
        parsed.secretEnvKey &&
        !envKeyOptions.value.some(o => o.value === parsed.secretEnvKey)
      ) {
        envKeyOptions.value = [
          { label: parsed.secretEnvKey, value: parsed.secretEnvKey },
          ...envKeyOptions.value
        ]
      }
    },
    beforeSubmit: async (data, editing) => {
      const mode = String(data.connectionMode ?? 'secret') as ConnectionMode

      if (mode === 'inline') {
        data.secretRef = null
        const cs = buildInlineConnectionString(data)
        if (cs) {
          data.connectionString = cs
        } else if (!editing) {
          throw new Error(
            isHttpType(data.sourceType) ? '请填写 HTTP 基地址' : '请填写完整的主机、数据库等连接信息'
          )
        } else {
          // 编辑时未重填连接信息：保留服务端已有连接串
          delete data.connectionString
        }
      } else {
        data.secretRef = composeSecretRef(data.secretKind, data.secretConfigKey, data.secretEnvKey)
        data.connectionString = ''
        if (!editing && !data.secretRef && Number(data.status) === 1) {
          throw new Error('请配置密钥引用，或改用结构化连接串')
        }
      }

      if (data.runTest) {
        const payload = buildTestPayload(data)
        // 编辑 + 内联但未重填时，跳过以免误报
        if (
          mode === 'inline' &&
          editing &&
          !payload.connectionString &&
          !payload.host &&
          !payload.baseUrl
        ) {
          // no-op
        } else {
          const [result, error] = await tryRequest(testSourceConnection(payload), {
            fallbackMessage: '连通性测试失败',
            silent: true
          })
          if (error) {
            throw new Error(error.message || '连通性测试失败')
          }
          if (!result?.success) {
            throw new Error(result?.message || '连通性测试未通过，请检查连接配置')
          }
          data.testHint = `[成功] ${result.message}`
        }
      }

      scrubVirtualFields(data)
      return data
    },
    request: getSourcePage,
    create: data => createSource(data as unknown as SaveSourceInput),
    update: (id, data) => updateSource(id, data as unknown as SaveSourceInput),
    remove: deleteSource
  }

  return { resource, pageRef }
}