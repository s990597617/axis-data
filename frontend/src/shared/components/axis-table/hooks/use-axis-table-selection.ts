import { computed, ref, shallowRef, watch, type Ref } from 'vue'
import type { DataTableRowKey } from 'naive-ui/es/data-table'

import type { AxisTableRow, AxisTableRowKeyResolver, AxisTableSelectionMode } from '../types'

export interface UseAxisTableSelectionOptions<Row extends AxisTableRow = AxisTableRow> {
  /** 当前表格行数据。 */
  data: Ref<Row[]>
  /** 行 key 配置。 */
  rowKey: Ref<AxisTableRowKeyResolver<Row> | undefined>
  /** 选择模式。 */
  mode: Ref<AxisTableSelectionMode | undefined>
  /** 是否跨页保留已选行。 */
  reserveSelection: Ref<boolean | undefined>
  /** 默认选中的行 key。 */
  defaultSelectedRowKeys: Ref<DataTableRowKey[] | undefined>
  /** 选中行变化回调。 */
  onSelectionChange: Ref<((keys: DataTableRowKey[], rows: Row[]) => void) | undefined>
}

function getRowKey<Row extends AxisTableRow>(
  row: Row,
  rowKey: AxisTableRowKeyResolver<Row> | undefined
): DataTableRowKey {
  if (typeof rowKey === 'function') return rowKey(row)
  if (typeof rowKey === 'string') {
    const value = row[rowKey]
    if (typeof value === 'string' || typeof value === 'number') return value
  }

  const fallbackValue = row.id
  return typeof fallbackValue === 'string' || typeof fallbackValue === 'number'
    ? fallbackValue
    : JSON.stringify(row)
}

/** 管理表格行选择、跨页缓存和对外选中态。 */
export function useAxisTableSelection<Row extends AxisTableRow = AxisTableRow>(
  options: UseAxisTableSelectionOptions<Row>
) {
  const selectedRowKeys = ref<DataTableRowKey[]>([])
  const selectedRowMap = shallowRef<Map<DataTableRowKey, Row>>(new Map())

  const enabled = computed(() => Boolean(options.mode.value))

  const multiple = computed(() => options.mode.value === 'multiple')

  const selectedRows = computed(() =>
    selectedRowKeys.value.flatMap(key => {
      const row = selectedRowMap.value.get(key)
      return row ? [row] : []
    })
  )

  function syncCurrentPageRows() {
    const nextMap = options.reserveSelection.value
      ? new Map(selectedRowMap.value)
      : new Map<DataTableRowKey, Row>()

    options.data.value.forEach(row => {
      const key = getRowKey(row, options.rowKey.value)
      if (selectedRowKeys.value.includes(key)) {
        nextMap.set(key, row)
      } else if (!options.reserveSelection.value) {
        nextMap.delete(key)
      }
    })

    selectedRowMap.value = nextMap
  }

  function emitSelectionChange() {
    options.onSelectionChange.value?.(selectedRowKeys.value, selectedRows.value)
  }

  function setSelectedRowKeys(keys: DataTableRowKey[]) {
    selectedRowKeys.value = multiple.value ? keys : keys.slice(-1)
    syncCurrentPageRows()
    emitSelectionChange()
  }

  function handleCheckedRowKeysChange(keys: DataTableRowKey[], rows: Row[]) {
    selectedRowKeys.value = multiple.value ? keys : keys.slice(-1)

    const nextMap = options.reserveSelection.value
      ? new Map(selectedRowMap.value)
      : new Map<DataTableRowKey, Row>()

    rows.forEach(row => {
      nextMap.set(getRowKey(row, options.rowKey.value), row)
    })

    if (!options.reserveSelection.value) {
      selectedRowMap.value = nextMap
    } else {
      const selectedKeySet = new Set(selectedRowKeys.value)
      selectedRowMap.value = new Map(
        Array.from(nextMap.entries()).filter(([key]) => selectedKeySet.has(key))
      )
    }

    emitSelectionChange()
  }

  function clearSelectedRows() {
    selectedRowKeys.value = []
    selectedRowMap.value = new Map()
    emitSelectionChange()
  }

  watch(
    () => options.defaultSelectedRowKeys.value,
    keys => {
      selectedRowKeys.value = keys ?? []
      syncCurrentPageRows()
    },
    { immediate: true }
  )

  watch(
    () => options.data.value,
    () => syncCurrentPageRows(),
    { deep: true }
  )

  return {
    enabled,
    multiple,
    selectedRows,
    selectedRowKeys,
    setSelectedRowKeys,
    handleCheckedRowKeysChange,
    clearSelectedRows
  }
}
