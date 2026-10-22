import type { DatasetDto, SaveDatasetInput, DataHubLookupItem } from '@/modules/datahub/api/dataset'
import {
  createDataset,
  deleteDataset,
  getDatasetPage,
  updateDataset
} from '@/modules/datahub/api/dataset'
import {
  listSourceLookup,
  listSourceObjects,
  listSourceColumns
} from '@/modules/datahub/api/source'
import { dataHubPermissionCodes } from '@/modules/datahub/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import type { AxisFormFieldContext, AxisFormOption } from '@/shared/components/axis-form'
import { ref } from 'vue'

const objectTypeOptions: AxisFormOption[] = [
  { label: '表', value: 1 },
  { label: '视图', value: 2 },
  { label: '查询', value: 3 }
]

const updateStrategyOptions: AxisFormOption[] = [
  { label: '全量', value: 1 },
  { label: '增量', value: 2 },
  { label: '差异检测', value: 3 }
]

function toLookupOptions(items: DataHubLookupItem[] | undefined): AxisFormOption[] {
  return (items ?? []).map(item => ({ label: item.label, value: item.value }))
}

function asIdString(value: unknown): string {
  if (value == null || value === '') return ''
  return String(value)
}

function splitFields(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.map(item => String(item).trim()).filter(Boolean)
  }
  if (typeof value === 'string') {
    return value
      .split(/[,，;；\s]+/)
      .map(item => item.trim())
      .filter(Boolean)
  }
  return []
}

function joinFields(value: unknown): string {
  if (Array.isArray(value)) {
    return value.map(item => String(item).trim()).filter(Boolean).join(',')
  }
  return typeof value === 'string' ? value.trim() : ''
}

export function useDatasetPage() {
  const sourceOptions = ref<AxisFormOption[]>([])
  const objectOptions = ref<AxisFormOption[]>([])
  const columnOptions = ref<AxisFormOption[]>([])
  const formModel = ref<Record<string, unknown> | null>(null)
  let objectMetaSeq = 0
  let columnMetaSeq = 0

  async function loadSourceOptions(currentId?: unknown) {
    try {
      sourceOptions.value = toLookupOptions(await listSourceLookup())
    } catch {
      sourceOptions.value = []
    }
    const id = asIdString(currentId)
    if (id && !sourceOptions.value.some(o => String(o.value) === id)) {
      sourceOptions.value = [{ label: `当前数据源（${id}）`, value: id }, ...sourceOptions.value]
    }
  }

  async function loadObjectOptions(sourceId: unknown, objectType: unknown) {
    const seq = ++objectMetaSeq
    const id = asIdString(sourceId)
    const type = Number(objectType)
    if (!id || type === 3 || !type) {
      objectOptions.value = []
      return
    }
    try {
      const items = await listSourceObjects(id, type)
      if (seq !== objectMetaSeq) return
      objectOptions.value = toLookupOptions(items)
    } catch {
      if (seq !== objectMetaSeq) return
      objectOptions.value = []
    }
  }

  async function loadColumnOptions(sourceId: unknown, objectName: unknown) {
    const seq = ++columnMetaSeq
    const id = asIdString(sourceId)
    const name = String(objectName ?? '').trim()
    if (!id || !name) {
      columnOptions.value = []
      return
    }
    try {
      const items = await listSourceColumns(id, name)
      if (seq !== columnMetaSeq) return
      columnOptions.value = toLookupOptions(items)
    } catch {
      if (seq !== columnMetaSeq) return
      columnOptions.value = []
    }
  }

  function ensureOption(list: { value: AxisFormOption[] }, value: unknown, labelPrefix: string) {
    const raw = String(value ?? '').trim()
    if (!raw) return
    if (!list.value.some(o => String(o.value) === raw)) {
      list.value = [{ label: `${labelPrefix}${raw}`, value: raw }, ...list.value]
    }
  }

  function clearDependentObjectFields(model: Record<string, unknown>) {
    model.objectName = ''
    model.primaryKeyFields = []
    model.incrementalField = ''
    columnOptions.value = []
  }

  function clearDependentColumnFields(model: Record<string, unknown>) {
    model.primaryKeyFields = []
    model.incrementalField = ''
  }

  const columns: AxisTableColumn<DatasetDto>[] = [
    { key: 'name', title: '名称', search: { field: 'keyword', placeholder: '名称/编码' } },
    { key: 'code', title: '编码' },
    {
      key: 'sourceName',
      title: '数据源',
      render: row => row.sourceName || row.sourceCode || asIdString(row.sourceId) || '-'
    },
    { key: 'objectType', title: '对象类型', dict: objectTypeOptions },
    { key: 'objectName', title: '对象名', ellipsis: { tooltip: true } },
    { key: 'primaryKeyFields', title: '主键字段', ellipsis: { tooltip: true } },
    { key: 'incrementalField', title: '增量字段' },
    { key: 'updateStrategy', title: '更新策略', dict: updateStrategyOptions },
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

  const resource: AxisCrudResource<DatasetDto> = {
    title: '',
    entityName: '数据集',
    permissions: dataHubPermissionCodes.dataset,
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑数据集「${String((model as Record<string, unknown>).name || '-')}」`
        : '新建数据集',
    actionColumn: {
      width: 120,
      maxInline: 2
    },
    formSections: [
      {
        key: 'basic',
        title: '基本信息',
        icon: 'ant-design:idcard-outlined',
        columns: 2,
        fields: [
          {
            field: 'sourceId',
            label: '数据源',
            component: 'select',
            required: true,
            defaultValue: null,
            options: () => sourceOptions.value,
            help: '切换数据源后将重新加载可选表/视图',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              filterable: true,
              clearable: true,
              placeholder: '请选择数据源',
              onUpdateValue: (value: unknown) => {
                ctx.setValue(value)
                const model = formModel.value ?? ctx.model
                model.sourceId = value
                clearDependentObjectFields(model)
                objectOptions.value = []
                void loadObjectOptions(value, model.objectType)
              }
            })) as unknown as Record<string, unknown>
          },
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
            componentProps: { maxlength: 64, placeholder: '请输入编码' }
          },
          {
            field: 'objectType',
            label: '对象类型',
            component: 'select',
            options: objectTypeOptions,
            required: true,
            defaultValue: 1,
            help: '表/视图可从数据源浏览选择；查询类型请手写 SQL',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              onUpdateValue: (value: unknown) => {
                ctx.setValue(value)
                const model = formModel.value ?? ctx.model
                model.objectType = value
                clearDependentObjectFields(model)
                objectOptions.value = []
                if (Number(value) !== 3) {
                  void loadObjectOptions(model.sourceId, value)
                }
              }
            })) as unknown as Record<string, unknown>
          },
          {
            field: 'objectName',
            label: '对象名',
            component: 'select',
            defaultValue: '',
            ifShow: model => Number(model.objectType) !== 3,
            options: () => objectOptions.value,
            help: '优先从数据源选择；元数据加载失败时可直接输入',
            componentProps: ((ctx: AxisFormFieldContext) => ({
              filterable: true,
              clearable: true,
              tag: true,
              placeholder: objectOptions.value.length
                ? '请选择或输入表/视图名'
                : '可手动输入表/视图名',
              onUpdateValue: (value: unknown) => {
                ctx.setValue(value)
                const model = formModel.value ?? ctx.model
                model.objectName = value
                clearDependentColumnFields(model)
                void loadColumnOptions(model.sourceId, value)
              }
            })) as unknown as Record<string, unknown>
          },
          {
            field: 'customSql',
            label: '自定义SQL',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            ifShow: model => Number(model.objectType) === 3,
            componentProps: { rows: 4, placeholder: '对象类型为查询时填写' }
          },
          {
            field: 'primaryKeyFields',
            label: '主键字段',
            component: 'select',
            defaultValue: [],
            options: () => columnOptions.value,
            help: '可多选；提交时以逗号拼接。也可手动输入',
            componentProps: {
              filterable: true,
              clearable: true,
              multiple: true,
              tag: true,
              placeholder: columnOptions.value.length
                ? '请选择主键字段'
                : '可手动输入，多个用回车添加'
            }
          },
          {
            field: 'updateStrategy',
            label: '更新策略',
            component: 'select',
            options: updateStrategyOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'incrementalField',
            label: '增量字段',
            component: 'select',
            defaultValue: '',
            ifShow: model => Number(model.updateStrategy) === 2,
            options: () => columnOptions.value,
            help: '增量策略时选择时间戳或自增列；也可手动输入',
            componentProps: {
              filterable: true,
              clearable: true,
              tag: true,
              placeholder: columnOptions.value.length ? '请选择增量字段' : '可手动输入字段名'
            }
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
      formModel.value = model
      model.sourceId = asIdString(model.sourceId) || null
      model.primaryKeyFields = splitFields(model.primaryKeyFields)
      model.incrementalField =
        model.incrementalField == null || model.incrementalField === ''
          ? ''
          : String(model.incrementalField)

      await loadSourceOptions(model.sourceId)
      objectOptions.value = []
      columnOptions.value = []

      if (Number(model.objectType) !== 3 && model.sourceId) {
        await loadObjectOptions(model.sourceId, model.objectType)
        ensureOption(objectOptions, model.objectName, '当前对象：')
      }

      if (model.sourceId && String(model.objectName ?? '').trim()) {
        await loadColumnOptions(model.sourceId, model.objectName)
        for (const field of splitFields(model.primaryKeyFields)) {
          ensureOption(columnOptions, field, '当前字段：')
        }
        if (model.incrementalField) {
          ensureOption(columnOptions, model.incrementalField, '当前字段：')
        }
      }
    },
    beforeSubmit: data => {
      data.sourceId = asIdString(data.sourceId)
      if (!data.sourceId) throw new Error('请选择数据源')
      data.primaryKeyFields = joinFields(data.primaryKeyFields)
      if (data.incrementalField != null) {
        data.incrementalField = String(data.incrementalField).trim()
      }
      if (Number(data.objectType) === 3) {
        data.objectName = ''
      } else {
        data.customSql = ''
      }
      if (Number(data.updateStrategy) !== 2) {
        data.incrementalField = ''
      }
      return data
    },
    request: getDatasetPage,
    create: data => createDataset(data as unknown as SaveDatasetInput),
    update: (id, data) => updateDataset(id, data as unknown as SaveDatasetInput),
    remove: deleteDataset
  }

  return { resource }
}
