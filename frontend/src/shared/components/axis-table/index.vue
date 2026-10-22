<script setup lang="ts" generic="Row extends AxisTableRow = AxisTableRow">
  import { computed, h, onMounted, ref, toRef, useSlots, watch } from 'vue'
  import { NButton, NDropdown, NSpace, NTooltip } from 'naive-ui'
  import type {
    DataTableColumn,
    DataTableColumns,
    DataTableInst,
    DataTableProps,
    DataTableRowKey
  } from 'naive-ui/es/data-table'

  import { Icon } from '@/shared/icons'
  import { useUserStore } from '@/store/modules/user.store'
  import { canAccessByMeta } from '@/shared/access'
  import { dialog } from '@/shared/utils/message'

  import AxisTableSearchPanel from './components/axis-table-search-panel.vue'
  import AxisTableToolbar from './components/axis-table-toolbar.vue'
  import { useAxisTableColumns } from './hooks/use-axis-table-columns'
  import { useAxisTableRequest } from './hooks/use-axis-table-request'
  import { useAxisTableSearch } from './hooks/use-axis-table-search'
  import { useAxisTableSelection } from './hooks/use-axis-table-selection'
  import { useAxisTableViewportMaxHeight } from './hooks/use-axis-table-viewport-max-height'
  import type {
    AxisTableActionColumn,
    AxisTableAction,
    AxisTableCacheState,
    AxisTablePaginationState,
    AxisTableRowAction,
    AxisTableProps,
    AxisTableQuery,
    AxisTableRow,
    AxisTableRowKeyResolver,
    AxisTableSearchConfig,
    AxisTableSize
  } from './types'

  defineOptions({
    name: 'AxisTable'
  })

  const props = withDefaults(defineProps<AxisTableProps<Row>>(), {
    data: () => [],
    queryParams: () => ({}),
    immediate: true,
    pageField: 'page',
    pageSizeField: 'pageSize',
    size: 'small',
    striped: false,
    showToolbar: true,
    showSearch: true,
    searchCollapsible: true,
    defaultSearchCollapsed: true,
    searchCollapsedRows: 1,
    searchCols: 4,
    selection: false,
    reserveSelection: true,
    showRefresh: true,
    showDensity: true,
    showColumnSetting: true,
    showSkeleton: true,
    skeletonRows: 5,
    emptyDescription: '暂无数据',
    errorDescription: '数据加载失败',
    keepDataOnError: true,
    showErrorState: true,
    autoHeight: true,
    heightOffset: 280,
    remote: true
  })

  const emit = defineEmits<{
    /** 请求成功后触发。 */
    fetchSuccess: [items: Row[]]
    /** 请求失败后触发。 */
    fetchError: [error: unknown]
  }>()

  const tableRef = ref<DataTableInst | null>(null)
  const rootRef = ref<HTMLElement | null>(null)
  const bodyRef = ref<HTMLElement | null>(null)
  const tableSize = ref<AxisTableSize>(props.size)
  const slots = useSlots()
  const actionTypeColors: Record<string, string> = {
    primary: 'var(--axis-primary-color)',
    error: '#d03050',
    warning: '#f0a020',
    success: '#18a058',
    info: '#2080f0'
  }
  const userStore = useUserStore()

  const subject = computed(() => ({
    roles: userStore.roles,
    permissions: userStore.permissions
  }))

  const tableRequest = useAxisTableRequest<Row>({
    data: toRef(() => props.data),
    request: toRef(() => props.request),
    requestAdapter: toRef(() => props.requestAdapter),
    localFilter: toRef(() => props.localFilter),
    queryParams: toRef(() => props.queryParams),
    pagination: toRef(() => props.pagination),
    pageField: toRef(() => props.pageField),
    pageSizeField: toRef(() => props.pageSizeField),
    beforeRequest: toRef(() => props.beforeRequest),
    afterRequest: toRef(() => props.afterRequest),
    keepDataOnError: toRef(() => props.keepDataOnError),
    remote: toRef(() => props.remote)
  })

  const tableColumns = useAxisTableColumns<Row>({
    columns: toRef(() => props.columns),
    subject,
    storageKey: toRef(() => props.columnSettingKey)
  })

  const tableSearch = useAxisTableSearch<Row>({
    schemas: toRef(() => props.searchSchemas),
    columns: toRef(() => props.columns)
  })

  const tableSelection = useAxisTableSelection<Row>({
    data: tableRequest.tableData,
    rowKey: toRef(() => props.rowKey),
    mode: toRef(() => props.selection),
    reserveSelection: toRef(() => props.reserveSelection),
    defaultSelectedRowKeys: toRef(() => props.defaultSelectedRowKeys),
    onSelectionChange: toRef(() => props.onSelectionChange)
  })

  const dataTableSlotNames = computed(() => {
    const axisSlotNames = new Set([
      'search',
      'search-item',
      'title',
      'toolbar',
      'toolbar-extra',
      'empty',
      'error'
    ])

    return Object.keys(slots).filter(slotName => {
      if (axisSlotNames.has(slotName)) return false
      return !slotName.startsWith('search-')
    })
  })

  const searchSlotNames = computed(() =>
    Object.keys(slots).filter(
      slotName =>
        slotName === 'search' || slotName === 'search-item' || slotName.startsWith('search-')
    )
  )

  const actionColumnConfig = computed<AxisTableActionColumn<Row> | null>(() => {
    const config = props.actionColumn
    return config?.enabled === false || !config?.actions?.length ? null : config
  })

  const tableActionColumn = computed<DataTableColumn<Row> | null>(() => {
    const config = actionColumnConfig.value
    if (!config) return null

    return {
      key: config.key ?? '__axis_actions__',
      title: config.title ?? '操作',
      width: config.width ?? 140,
      fixed: config.fixed ?? 'right',
      align: 'center',
      configurable: false,
      render: (row, rowIndex) => renderRowActions(row, rowIndex, config)
    }
  })

  const displayColumns = computed<DataTableColumns<Row>>(() =>
    (tableColumns.visibleColumns.value as DataTableColumns<Row>).map(column => {
      if (!shouldRenderDictColumn(column)) return column

      return {
        ...column,
        render: row => readDictLabel(column, row)
      }
    })
  )

  const mergedColumns = computed<DataTableColumns<Row>>(() => {
    const columns: DataTableColumns<Row> = []

    if (tableSelection.enabled.value) {
      columns.push({
        key: '__axis_selection__',
        type: 'selection',
        multiple: tableSelection.multiple.value,
        fixed: 'left',
        configurable: false
      } as DataTableColumn<Row>)
    }

    columns.push(...displayColumns.value)

    if (tableActionColumn.value) {
      columns.push(tableActionColumn.value)
    }

    return columns
  })

  const resolvedRowKey = computed<NonNullable<DataTableProps['rowKey']>>(() =>
    createRowKeyResolver(props.rowKey)
  )

  const fitViewport = computed(
    () => props.autoHeight !== false && props.maxHeight == null && !props.tableProps?.flexHeight
  )
  const { maxHeight: viewportMaxHeight, measure: measureViewportMaxHeight } =
    useAxisTableViewportMaxHeight({
      enabled: () => fitViewport.value,
      rootRef,
      bodyRef
    })

  watch(
    () => tableRequest.loading.value,
    () => {
      void measureViewportMaxHeight()
    }
  )

  // 默认按剩余主屏高度封顶；抽屉 flexHeight 或显式 maxHeight 时不干预。
  // generic + withDefaults 对 autoHeight 可能不生效，故用 !== false。
  const resolvedMaxHeight = computed(() => {
    if (props.maxHeight != null) return props.maxHeight
    if (props.tableProps?.flexHeight) return props.tableProps.maxHeight
    if (props.autoHeight !== false) return viewportMaxHeight.value
    return props.tableProps?.maxHeight
  })

  const tableBindValues = computed(() => ({
    rowKey: resolvedRowKey.value,
    // 有 request 或 remote !== false 时我们自行处理分页，避免 naive-ui 二次切片。
    remote: Boolean(props.request) || props.remote !== false,
    striped: props.striped,
    ...props.tableProps,
    size: tableSize.value,
    loading: tableRequest.loading.value,
    columns: mergedColumns.value,
    data: tableRequest.tableData.value,
    checkedRowKeys: tableSelection.selectedRowKeys.value,
    pagination: tableRequest.mergedPagination.value,
    maxHeight: resolvedMaxHeight.value,
    minHeight: props.tableProps?.minHeight
  }))

  const showTableSkeleton = computed(
    () =>
      props.showSkeleton && tableRequest.loading.value && tableRequest.tableData.value.length === 0
  )

  const resolvedSearchConfig = computed<Required<AxisTableSearchConfig>>(() => ({
    minItemWidth: props.searchConfig?.minItemWidth ?? 220,
    maxColumns: props.searchConfig?.maxColumns ?? props.searchCols,
    collapsedRows: props.searchConfig?.collapsedRows ?? props.searchCollapsedRows,
    labelWidth: props.searchConfig?.labelWidth ?? 'auto'
  }))

  const skeletonColumnCount = computed(() =>
    Math.max(1, Math.min(mergedColumns.value.length || props.columns.length || 4, 6))
  )

  watch(
    () => props.size,
    value => {
      tableSize.value = value
    }
  )

  watch(
    () => [
      tableSearch.getSearchParams(),
      tableSize.value,
      tableRequest.currentPage.value,
      tableRequest.currentPageSize.value
    ],
    () => {
      writeTableStateCache()
    }
  )

  onMounted(() => {
    restoreTableStateCache()

    if (props.immediate) {
      tableRequest
        .fetch(tableSearch.getSearchParams())
        .then(() => emit('fetchSuccess', tableRequest.tableData.value))
        .catch(error => emit('fetchError', error))
    }
  })

  async function reload(params?: AxisTableQuery) {
    try {
      await tableRequest.reload({
        ...tableSearch.getSearchParams(),
        ...params
      })
      emit('fetchSuccess', tableRequest.tableData.value)
    } catch (error) {
      emit('fetchError', error)
    }
  }

  async function search(params?: AxisTableQuery) {
    try {
      await tableRequest.search({
        ...tableSearch.getSearchParams(),
        ...params
      })
      emit('fetchSuccess', tableRequest.tableData.value)
    } catch (error) {
      emit('fetchError', error)
    }
  }

  function setLoading(loading: boolean) {
    tableRequest.loading.value = loading
  }

  function setData(rows: Row[]) {
    tableRequest.tableData.value = rows
  }

  function getData() {
    return tableRequest.tableData.value
  }

  function exportCsv(filename = `${props.title || 'table'}.csv`) {
    const exportColumns = displayColumns.value
      .map(column => {
        const key = readColumnKey(column)
        const title = 'title' in column ? column.title : key
        return key && typeof title !== 'function'
          ? {
              key,
              title: String(title ?? key)
            }
          : null
      })
      .filter((column): column is { key: string; title: string } => Boolean(column))

    const rows = tableRequest.tableData.value
    const csvRows = [
      exportColumns.map(column => escapeCsvValue(column.title)).join(','),
      ...rows.map(row =>
        exportColumns.map(column => escapeCsvValue(readExportCellValue(column.key, row))).join(',')
      )
    ]

    const blob = new Blob([`\uFEFF${csvRows.join('\n')}`], {
      type: 'text/csv;charset=utf-8;'
    })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = filename
    link.click()
    URL.revokeObjectURL(url)
  }

  function getSelectedRows() {
    return tableSelection.selectedRows.value
  }

  function getSelectedRowKeys() {
    return tableSelection.selectedRowKeys.value
  }

  function setSelectedRowKeys(keys: DataTableRowKey[]) {
    tableSelection.setSelectedRowKeys(keys)
  }

  function clearSelectedRows() {
    tableSelection.clearSelectedRows()
  }

  function setColumns(keys: DataTableRowKey[]) {
    tableColumns.setColumns(keys)
  }

  function setSearchParams(params: AxisTableQuery) {
    tableSearch.setSearchParams(params)
  }

  function getSearchParams() {
    return tableSearch.getSearchParams()
  }

  async function resetSearch() {
    tableSearch.resetSearchModel()
    await search()
  }

  function getPaginationState() {
    return tableRequest.getPaginationState()
  }

  function setPaginationState(state: Partial<AxisTablePaginationState>) {
    tableRequest.setPaginationState(state)
  }

  function getTableInstance() {
    return tableRef.value
  }

  function handleDensitySelect(key: string | number) {
    tableSize.value = key as AxisTableSize
  }

  function readColumnKey(column: DataTableColumn<Row>) {
    const key = 'key' in column ? column.key : undefined
    return typeof key === 'string' || typeof key === 'number' ? String(key) : ''
  }

  function createRowKeyResolver(rowKey: AxisTableRowKeyResolver<Row> | undefined) {
    return (row: Row): DataTableRowKey => {
      if (typeof rowKey === 'function') return rowKey(row)

      if (typeof rowKey === 'string' || typeof rowKey === 'number') {
        const value = row[String(rowKey)]
        if (typeof value === 'string' || typeof value === 'number') return value
      }

      const fallbackValue = row.id
      return typeof fallbackValue === 'string' || typeof fallbackValue === 'number'
        ? fallbackValue
        : JSON.stringify(row)
    }
  }

  function shouldRenderDictColumn(column: DataTableColumn<Row>) {
    const axisColumn = column as AxisTableProps<Row>['columns'][number]
    return Boolean(
      axisColumn.dictRender !== false &&
      !('render' in column && column.render) &&
      readColumnKey(column) &&
      (axisColumn.dict || axisColumn.enum)
    )
  }

  function readDictLabel(column: DataTableColumn<Row>, row: Row) {
    const key = readColumnKey(column)
    const value = row[key]
    const option = tableSearch.columnDictOptions.value[key]?.find(
      item => item.value === value || String(item.value) === String(value ?? '')
    )
    const label = option?.label
    return typeof label === 'function' ? String(value ?? '') : String(label ?? value ?? '')
  }

  function readExportCellValue(key: string, row: Row) {
    const column = displayColumns.value.find(item => readColumnKey(item) === key)
    return column ? readDictLabel(column, row) || row[key] : row[key]
  }

  function escapeCsvValue(value: unknown) {
    const text = String(value ?? '')
    return /[",\n\r]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text
  }

  function readTableStateCache(): AxisTableCacheState | null {
    if (!props.stateStorageKey) return null

    try {
      const rawValue = localStorage.getItem(props.stateStorageKey)
      return rawValue ? (JSON.parse(rawValue) as AxisTableCacheState) : null
    } catch {
      return null
    }
  }

  function restoreTableStateCache() {
    const cache = readTableStateCache()
    if (!cache) return

    if (cache.searchParams) tableSearch.setSearchParams(cache.searchParams)
    if (cache.size) tableSize.value = cache.size
    if (cache.pagination) tableRequest.setPaginationState(cache.pagination)
  }

  function writeTableStateCache() {
    if (!props.stateStorageKey) return

    const cache: AxisTableCacheState = {
      searchParams: tableSearch.getSearchParams(),
      size: tableSize.value,
      pagination: tableRequest.getPaginationState()
    }

    localStorage.setItem(props.stateStorageKey, JSON.stringify(cache))
  }

  function readActionLabel(action: AxisTableRowAction<Row>, row: Row) {
    return typeof action.label === 'function' ? action.label(row) : action.label
  }

  function readActionType(action: AxisTableRowAction<Row>, row: Row) {
    return typeof action.type === 'function' ? action.type(row) : action.type
  }

  function canShowAction(action: AxisTableRowAction<Row>, row: Row) {
    const visibleByAccess = canAccessByMeta(
      {
        roles: action.roles,
        permissions: action.permissions
      },
      subject.value
    )
    const visibleByBusiness =
      typeof action.ifShow === 'function' ? action.ifShow(row) : action.ifShow !== false

    return visibleByAccess && visibleByBusiness
  }

  function isActionDisabled(action: AxisTableRowAction<Row>, row: Row) {
    return typeof action.disabled === 'function' ? action.disabled(row) : Boolean(action.disabled)
  }

  function runRowAction(action: AxisTableRowAction<Row>, row: Row, rowIndex: number) {
    const confirmText = typeof action.confirm === 'function' ? action.confirm(row) : action.confirm

    if (!confirmText) {
      void action.onClick?.(row, rowIndex)
      return
    }

    dialog.warning({
      title: '操作确认',
      content: confirmText,
      positiveText: '确认',
      negativeText: '取消',
      onPositiveClick: () => action.onClick?.(row, rowIndex)
    })
  }

  function renderActionButton(action: AxisTableRowAction<Row>, row: Row, rowIndex: number) {
    const label = readActionLabel(action, row)
    const disabled = isActionDisabled(action, row)
    const actionType = readActionType(action, row)

    if (action.icon) {
      const typeColor = actionType ? actionTypeColors[actionType] : undefined
      const baseColor = action.color ?? typeColor ?? 'var(--axis-text-color-2)'
      const hoverColor = action.color ?? typeColor ?? 'var(--axis-primary-color)'

      return h(
        NTooltip,
        { trigger: 'hover' },
        {
          trigger: () =>
            h(
              'button',
              {
                class: 'axis-table__action-icon',
                disabled,
                style: {
                  '--action-color': baseColor,
                  '--action-hover-color': hoverColor
                },
                onClick: (e: Event) => {
                  e.stopPropagation()
                  runRowAction(action, row, rowIndex)
                }
              },
              [h(Icon, { icon: action.icon! })]
            ),
          default: () => label
        }
      )
    }

    return h(
      NButton,
      {
        quaternary: true,
        size: 'small' as const,
        type: actionType,
        class: 'axis-table__action-text',
        disabled,
        onClick: (event: Event) => {
          event.stopPropagation()
          runRowAction(action, row, rowIndex)
        }
      },
      { default: () => label }
    )
  }

  function renderMoreActions(actions: AxisTableRowAction<Row>[], row: Row, rowIndex: number) {
    return h(
      NDropdown,
      {
        trigger: 'click',
        placement: 'bottom-end',
        options: actions.map(action => {
          const actionType = readActionType(action, row)
          const actionColor =
            action.color ?? (actionType ? actionTypeColors[actionType] : undefined)
          const label = readActionLabel(action, row)

          return {
            key: action.key,
            label:
              actionType === 'error'
                ? () => h('span', { style: { color: actionTypeColors.error } }, label)
                : label,
            icon: action.icon
              ? () =>
                  h(Icon, {
                    icon: action.icon!,
                    style: actionColor ? { color: actionColor } : undefined
                  })
              : undefined,
            disabled: isActionDisabled(action, row)
          }
        }),
        onSelect: (key: string) => {
          const action = actions.find(item => item.key === key)
          if (action) runRowAction(action, row, rowIndex)
        }
      },
      {
        default: () =>
          h(
            'button',
            {
              type: 'button',
              class: 'axis-table__action-more',
              onClick: (event: Event) => event.stopPropagation()
            },
            [h('span', null, '更多'), h(Icon, { icon: 'ant-design:down-outlined' })]
          )
      }
    )
  }

  function renderRowActions(row: Row, rowIndex: number, config: AxisTableActionColumn<Row>) {
    const visibleActions = config.actions.filter(action => canShowAction(action, row))
    const maxInline = config.maxInline ?? 1
    const inlineActions = visibleActions.filter(action => !action.more).slice(0, maxInline)
    const moreActions = visibleActions.filter(action => !inlineActions.includes(action))

    const children = [
      ...inlineActions.map(action => renderActionButton(action, row, rowIndex)),
      ...(moreActions.length ? [renderMoreActions(moreActions, row, rowIndex)] : [])
    ]

    return h('div', { class: 'axis-table__action-bar' }, children)
  }

  defineExpose<AxisTableAction<Row>>({
    reload,
    search,
    setLoading,
    setData,
    getData,
    exportCsv,
    getSelectedRows,
    getSelectedRowKeys,
    setSelectedRowKeys,
    clearSelectedRows,
    setColumns,
    resetColumns: tableColumns.resetColumns,
    getSearchParams,
    setSearchParams,
    resetSearch,
    getPaginationState,
    setPaginationState,
    getTableInstance
  })
</script>

<template>
  <div ref="rootRef" class="axis-table">
    <AxisTableSearchPanel
      v-if="showSearch && (tableSearch.visibleSchemas.value.length || $slots.search)"
      :schemas="tableSearch.visibleSchemas.value"
      :model="tableSearch.searchModel"
      :config="resolvedSearchConfig"
      :form-props="searchFormProps"
      :collapsible="searchCollapsible"
      :default-collapsed="defaultSearchCollapsed"
      :dict-loading="tableSearch.dictLoading.value"
      :read-schema-options="tableSearch.readSchemaOptions"
      :get-value="tableSearch.getSearchValue"
      :set-value="tableSearch.setSearchValue"
      @search="search()"
      @reset="resetSearch"
    >
      <template v-for="slotName in searchSlotNames" :key="slotName" #[slotName]="slotProps">
        <slot :name="slotName" v-bind="slotProps" />
      </template>
    </AxisTableSearchPanel>

    <div class="axis-table__main">
      <AxisTableToolbar
        v-if="showToolbar"
        :title="title"
        :title-tooltip="titleTooltip"
        :show-refresh="showRefresh"
        :show-density="showDensity"
        :show-column-setting="showColumnSetting"
        :size="tableSize"
        :column-options="tableColumns.columnSettingOptions.value"
        :checked-column-keys="tableColumns.checkedColumnKeys.value"
        @refresh="reload()"
        @update:size="handleDensitySelect"
        @update:checked-column-keys="tableColumns.setColumns"
        @reset-columns="tableColumns.resetColumns"
      >
        <template v-if="$slots.title" #title>
          <slot name="title" />
        </template>
        <template v-if="$slots.toolbar" #toolbar>
          <slot name="toolbar" />
        </template>
        <template v-if="$slots['toolbar-extra']" #extra>
          <slot name="toolbar-extra" />
        </template>
      </AxisTableToolbar>

      <div v-if="tableSelection.multiple.value" class="axis-table__selection-bar">
        <div class="axis-table__selection-summary">
          <span>
            已选择
            <strong>{{ tableSelection.selectedRowKeys.value.length }}</strong>
            项
          </span>
          <n-button
            v-if="tableSelection.selectedRowKeys.value.length"
            text
            size="small"
            type="primary"
            @click="tableSelection.clearSelectedRows"
          >
            清空
          </n-button>
        </div>
        <div class="axis-table__selection-actions">
          <slot
            name="selection"
            :selected-row-keys="tableSelection.selectedRowKeys.value"
            :selected-rows="tableSelection.selectedRows.value"
            :clear="tableSelection.clearSelectedRows"
          />
        </div>
      </div>

      <div ref="bodyRef" class="axis-table__body">
        <n-data-table
          ref="tableRef"
          v-bind="tableBindValues"
          @update:page="tableRequest.updatePage"
          @update:page-size="tableRequest.updatePageSize"
          @update:checked-row-keys="
            (keys, rows) => tableSelection.handleCheckedRowKeysChange(keys, rows as Row[])
          "
        >
          <template v-for="slotName in dataTableSlotNames" #[slotName]="slotProps" :key="slotName">
            <slot :name="slotName" v-bind="slotProps" />
          </template>

          <template #empty>
            <slot
              v-if="showErrorState && tableRequest.error.value"
              name="error"
              :error="tableRequest.error.value"
              :reload="reload"
            >
              <n-result
                status="error"
                title="加载失败"
                :description="errorDescription"
                size="small"
              >
                <template #footer>
                  <n-button size="small" type="primary" @click="reload()">重试</n-button>
                </template>
              </n-result>
            </slot>

            <slot v-else name="empty">
              <n-empty :description="emptyDescription" />
            </slot>
          </template>
        </n-data-table>

        <div v-if="showTableSkeleton" class="axis-table__skeleton">
          <div v-for="rowIndex in skeletonRows" :key="rowIndex" class="axis-table__skeleton-row">
            <span
              v-for="columnIndex in skeletonColumnCount"
              :key="columnIndex"
              class="axis-table__skeleton-line"
              :style="{ width: `${88 - ((rowIndex + columnIndex) % 4) * 10}%` }"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss" src="./styles.scss"></style>
