import { computed, reactive, ref, watch, type Ref } from 'vue'
import type { SelectOption } from 'naive-ui/es/select'

import type {
  AxisTableColumn,
  AxisTableDictResponse,
  AxisTableQuery,
  AxisTableRow,
  AxisTableSearchSchema
} from '../types'

export interface UseAxisTableSearchOptions<Row extends AxisTableRow = AxisTableRow> {
  /** 搜索项配置。 */
  schemas: Ref<AxisTableSearchSchema[] | undefined>
  /** 表格列配置，用于从列级 search 配置里推导搜索项。 */
  columns: Ref<AxisTableColumn<Row>[]>
}

function cloneSearchValue(value: unknown): unknown {
  if (Array.isArray(value)) return [...value]
  if (value && typeof value === 'object') return { ...(value as Record<string, unknown>) }
  return value
}

function readColumnKey<Row extends AxisTableRow>(column: AxisTableColumn<Row>): string | null {
  const key = 'key' in column ? column.key : undefined
  return typeof key === 'string' || typeof key === 'number' ? String(key) : null
}

function readColumnTitle<Row extends AxisTableRow>(column: AxisTableColumn<Row>): string {
  const title = 'title' in column ? column.title : undefined
  return typeof title === 'string' || typeof title === 'number' ? String(title) : ''
}

function normalizeDictItem(item: unknown, labelField: string, valueField: string): SelectOption {
  if (item && typeof item === 'object') {
    const record = item as Record<string, unknown>
    const label = record.label ?? record[labelField]
    const value = record.value ?? record[valueField]

    return {
      ...record,
      label: String(label ?? ''),
      value: typeof value === 'string' || typeof value === 'number' ? value : String(value ?? '')
    }
  }

  return {
    label: String(item),
    value: typeof item === 'number' ? item : String(item)
  }
}

function readDictArray(response: AxisTableDictResponse): unknown[] {
  if (Array.isArray(response)) return response

  const payload = response.data ?? response.result ?? response.list ?? response.options
  if (Array.isArray(payload)) return payload
  if (payload && typeof payload === 'object') {
    const record = payload as Record<string, unknown>
    const nested = record.list ?? record.options ?? record.records ?? record.rows
    return Array.isArray(nested) ? nested : []
  }

  return []
}

function resolveSchemaField<Row extends AxisTableRow>(
  column: AxisTableColumn<Row>,
  schema: AxisTableSearchSchema
): AxisTableSearchSchema {
  const columnKey = readColumnKey(column)
  return {
    ...schema,
    field: schema.key ?? schema.field ?? columnKey ?? '',
    label: schema.label ?? readColumnTitle(column)
  }
}

function createColumnSearchSchema<Row extends AxisTableRow>(
  column: AxisTableColumn<Row>,
  order: number
): AxisTableSearchSchema | null {
  if (column.hideInSearch) return null
  if (!column.search) return null

  const columnKey = readColumnKey(column)
  if (!columnKey && typeof column.search === 'boolean') return null

  const columnDict = column.dict ?? column.enum
  const baseSchema: AxisTableSearchSchema = {
    field: columnKey ?? '',
    label: readColumnTitle(column),
    component: columnDict ? 'select' : 'input',
    dict: columnDict,
    labelField: column.labelField,
    valueField: column.valueField,
    order
  }

  if (typeof column.search === 'boolean') return baseSchema

  if (typeof column.search === 'string') {
    return {
      ...baseSchema,
      component: column.search
    }
  }

  return resolveSchemaField(column, {
    ...baseSchema,
    ...column.search,
    dict: column.search.dict ?? column.search.enum ?? baseSchema.dict,
    labelField: column.search.labelField ?? baseSchema.labelField,
    valueField: column.search.valueField ?? baseSchema.valueField
  })
}

/** 管理内置搜索表单状态。 */
export function useAxisTableSearch<Row extends AxisTableRow = AxisTableRow>(
  options: UseAxisTableSearchOptions<Row>
) {
  const searchModel = reactive<AxisTableQuery>({})
  const dictOptions = ref<Record<string, SelectOption[]>>({})
  const columnDictOptions = ref<Record<string, SelectOption[]>>({})
  const dictLoading = ref<Record<string, boolean>>({})
  const dictSourceCache = new WeakMap<
    () => Promise<AxisTableDictResponse>,
    Promise<SelectOption[]>
  >()

  const normalizedSchemas = computed(() => {
    const columnSchemas = options.columns.value
      .map((column, index) => createColumnSearchSchema(column, index + 100))
      .filter((schema): schema is AxisTableSearchSchema => Boolean(schema?.field))

    const mergedSchemas = new Map<string, AxisTableSearchSchema>()

    ;[...columnSchemas, ...(options.schemas.value ?? [])]
      .map((schema, index) => ({
        ...schema,
        label: schema.label ?? schema.field,
        order: schema.order ?? index + 1
      }))
      .forEach(schema => {
        const existingSchema = mergedSchemas.get(schema.field)
        mergedSchemas.set(schema.field, {
          ...existingSchema,
          ...schema,
          order: schema.order ?? existingSchema?.order
        })
      })

    return Array.from(mergedSchemas.values()).sort((a, b) => (a.order ?? 0) - (b.order ?? 0))
  })

  const visibleSchemas = computed(() => normalizedSchemas.value.filter(schema => !schema.hidden))

  function createDefaultModel(): AxisTableQuery {
    return visibleSchemas.value.reduce<AxisTableQuery>((model, schema) => {
      model[schema.field] = cloneSearchValue(schema.defaultValue ?? null)
      return model
    }, {})
  }

  function resetSearchModel() {
    const defaultModel = createDefaultModel()
    Object.keys(searchModel).forEach(key => {
      delete searchModel[key]
    })
    Object.assign(searchModel, defaultModel)
  }

  /** 列或 schema 变更时，仅补齐新字段、移除已消失字段，保留用户已填值。 */
  function syncSearchModel() {
    const nextFields = new Set(visibleSchemas.value.map(schema => schema.field))

    Object.keys(searchModel).forEach(key => {
      if (!nextFields.has(key)) delete searchModel[key]
    })

    visibleSchemas.value.forEach(schema => {
      if (!(schema.field in searchModel)) {
        searchModel[schema.field] = cloneSearchValue(schema.defaultValue ?? null)
      }
    })
  }

  function setSearchParams(params: AxisTableQuery) {
    Object.assign(searchModel, params)
  }

  function setSearchValue(field: string, value: unknown) {
    searchModel[field] = value
  }

  function getSearchValue(field: string) {
    return searchModel[field]
  }

  function getSearchParams(): AxisTableQuery {
    return Object.entries(searchModel).reduce<AxisTableQuery>((params, [key, value]) => {
      if (value === null || value === undefined || value === '') return params
      if (Array.isArray(value) && !value.length) return params
      params[key] = value
      return params
    }, {})
  }

  function getStringValue(field: string): string | null {
    const value = searchModel[field]
    return typeof value === 'string' ? value : null
  }

  function setStringValue(field: string, value: string | null) {
    searchModel[field] = value
  }

  function getSelectValue(field: string): string | number | null {
    const value = searchModel[field]
    return typeof value === 'string' || typeof value === 'number' ? value : null
  }

  function setSelectValue(field: string, value: string | number | null) {
    searchModel[field] = value
  }

  function getNumberValue(field: string): number | null {
    const value = searchModel[field]
    return typeof value === 'number' ? value : null
  }

  function setNumberValue(field: string, value: number | null) {
    searchModel[field] = value
  }

  function getNumberRangeValue(field: string): [number, number] | null {
    const value = searchModel[field]
    return Array.isArray(value) && value.length === 2 ? (value as [number, number]) : null
  }

  function setNumberRangeValue(field: string, value: [number, number] | null) {
    searchModel[field] = value
  }

  function readSchemaOptions(schema: AxisTableSearchSchema) {
    return schema.options ?? dictOptions.value[schema.field] ?? []
  }

  async function resolveDictOptions(
    source: AxisTableSearchSchema['dict'],
    labelField: string,
    valueField: string,
    loadingKey?: string
  ): Promise<SelectOption[] | null> {
    if (!source) return null

    if (Array.isArray(source)) {
      return source.map(item => normalizeDictItem(item, labelField, valueField))
    }

    if (typeof source !== 'function') return null

    const cached = dictSourceCache.get(source)
    if (cached) return cached

    if (loadingKey) {
      dictLoading.value = { ...dictLoading.value, [loadingKey]: true }
    }

    const pending = (async () => {
      try {
        const response = await source()
        return readDictArray(response).map(item => normalizeDictItem(item, labelField, valueField))
      } finally {
        if (loadingKey) {
          dictLoading.value = { ...dictLoading.value, [loadingKey]: false }
        }
      }
    })()

    dictSourceCache.set(source, pending)
    return pending
  }

  async function loadSchemaDict(schema: AxisTableSearchSchema) {
    if (schema.options) return
    const dict = schema.dict ?? schema.enum
    const labelField = schema.labelField ?? 'label'
    const valueField = schema.valueField ?? 'value'
    const options = await resolveDictOptions(dict, labelField, valueField, schema.field)
    if (!options) return
    dictOptions.value = { ...dictOptions.value, [schema.field]: options }
  }

  async function loadColumnDict(column: AxisTableColumn<Row>) {
    const key = readColumnKey(column)
    if (!key) return
    const dict = column.dict ?? column.enum
    const labelField = column.labelField ?? 'label'
    const valueField = column.valueField ?? 'value'
    const options = await resolveDictOptions(dict, labelField, valueField)
    if (!options) return
    columnDictOptions.value = { ...columnDictOptions.value, [key]: options }
  }

  function loadDictionaries() {
    visibleSchemas.value.forEach(schema => {
      void loadSchemaDict(schema)
    })
    options.columns.value.forEach(column => {
      void loadColumnDict(column)
    })
  }

  watch(
    normalizedSchemas,
    () => {
      syncSearchModel()
      loadDictionaries()
    },
    { immediate: true }
  )

  watch(
    () => options.columns.value,
    () => {
      options.columns.value.forEach(column => {
        void loadColumnDict(column)
      })
    },
    { deep: true }
  )

  return {
    searchModel,
    normalizedSchemas,
    visibleSchemas,
    dictOptions,
    columnDictOptions,
    dictLoading,
    readSchemaOptions,
    getSearchParams,
    setSearchParams,
    getSearchValue,
    setSearchValue,
    getStringValue,
    setStringValue,
    getSelectValue,
    setSelectValue,
    getNumberValue,
    setNumberValue,
    getNumberRangeValue,
    setNumberRangeValue,
    resetSearchModel
  }
}
