import { computed, ref, watch, type Ref } from 'vue'
import type { DataTableRowKey } from 'naive-ui/es/data-table'

import { canAccessByMeta } from '@/shared/access'
import type { AccessSubject } from '@/shared/access'

import type { AxisTableColumn, AxisTableRow } from '../types'

export interface UseAxisTableColumnsOptions<Row extends AxisTableRow = AxisTableRow> {
  /** 原始列配置。 */
  columns: Ref<AxisTableColumn<Row>[]>
  /** 当前用户权限主体。 */
  subject: Ref<AccessSubject>
  /** 列设置持久化 key。 */
  storageKey: Ref<string | undefined>
}

export interface AxisTableColumnSettingOption {
  /** 列唯一标识。 */
  key: DataTableRowKey
  /** 列设置里展示的标题。 */
  title: string
}

function getColumnKey<Row extends AxisTableRow>(column: AxisTableColumn<Row>): DataTableRowKey | null {
  const key = 'key' in column ? column.key : undefined
  return typeof key === 'string' || typeof key === 'number' ? key : null
}

function getColumnTitle<Row extends AxisTableRow>(column: AxisTableColumn<Row>): string {
  const title = 'title' in column ? column.title : undefined
  return typeof title === 'string' || typeof title === 'number' ? String(title) : '未命名列'
}

function canShowColumn<Row extends AxisTableRow>(column: AxisTableColumn<Row>): boolean {
  if (typeof column.ifShow === 'boolean') return column.ifShow
  if (typeof column.ifShow === 'function') return column.ifShow(column)
  return true
}

/** 管理表格列权限、列显隐和列设置状态。 */
export function useAxisTableColumns<Row extends AxisTableRow = AxisTableRow>(
  options: UseAxisTableColumnsOptions<Row>
) {
  const checkedColumnKeys = ref<DataTableRowKey[]>([])

  function readStorageColumns(): DataTableRowKey[] | null {
    if (!options.storageKey.value) return null
    try {
      const rawValue = localStorage.getItem(options.storageKey.value)
      const parsedValue = rawValue ? JSON.parse(rawValue) : null
      return Array.isArray(parsedValue) ? (parsedValue as DataTableRowKey[]) : null
    } catch {
      return null
    }
  }

  function writeStorageColumns(keys: DataTableRowKey[]) {
    if (!options.storageKey.value) return
    localStorage.setItem(options.storageKey.value, JSON.stringify(keys))
  }

  const configurableColumns = computed(() =>
    options.columns.value.filter(column => {
      const key = getColumnKey(column)
      return key !== null && column.configurable !== false && !column.hideInColumnSetting
    })
  )

  const columnSettingOptions = computed<AxisTableColumnSettingOption[]>(() =>
    configurableColumns.value
      .map(column => {
        const key = getColumnKey(column)
        return key
          ? {
              key,
              title: getColumnTitle(column)
            }
          : null
      })
      .filter((item): item is AxisTableColumnSettingOption => item !== null)
  )

  function resetColumns() {
    const defaultKeys = configurableColumns.value
      .filter(column => !column.hideInTable)
      .map(column => getColumnKey(column))
      .filter((key): key is DataTableRowKey => key !== null)
    checkedColumnKeys.value = defaultKeys
    writeStorageColumns(defaultKeys)
  }

  watch(
    () => options.columns.value,
    () => {
      const storageKeys = readStorageColumns()
      if (storageKeys) {
        const availableKeys = new Set(
          configurableColumns.value
            .map(column => getColumnKey(column))
            .filter((key): key is DataTableRowKey => key !== null)
        )
        checkedColumnKeys.value = storageKeys.filter(key => availableKeys.has(key))
      } else {
        resetColumns()
      }
    },
    { immediate: true }
  )

  watch(
    checkedColumnKeys,
    keys => {
      writeStorageColumns(keys)
    },
    { deep: true }
  )

  const visibleColumns = computed(() => {
    const visibleKeySet = new Set(checkedColumnKeys.value)

    return options.columns.value.filter(column => {
      const key = getColumnKey(column)
      const isConfigurable = key !== null && column.configurable !== false
      const visibleBySetting = !isConfigurable || visibleKeySet.has(key)
      const visibleByAccess = canAccessByMeta(
        {
          roles: column.roles,
          permissions: column.permissions
        },
        options.subject.value
      )

      return visibleBySetting && visibleByAccess && canShowColumn(column)
    })
  })

  function setColumns(keys: DataTableRowKey[]) {
    checkedColumnKeys.value = keys
    writeStorageColumns(keys)
  }

  return {
    checkedColumnKeys,
    configurableColumns,
    columnSettingOptions,
    visibleColumns,
    setColumns,
    resetColumns
  }
}
