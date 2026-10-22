import { computed, ref, unref, watch, type Ref } from 'vue'
import type { PaginationProps } from 'naive-ui/es/pagination'

import type {
  AxisTablePaginationState,
  AxisTableLocalFilter,
  AxisTableQuery,
  AxisTableRequest,
  AxisTableRequestAdapter,
  AxisTableRequestResult,
  AxisTableRow
} from '../types'

export interface UseAxisTableRequestOptions<Row extends AxisTableRow = AxisTableRow> {
  /** 静态数据源。 */
  data: Ref<Row[] | undefined>
  /** 远程请求函数。 */
  request: Ref<AxisTableRequest | undefined>
  /** 后端响应适配器。 */
  requestAdapter: Ref<AxisTableRequestAdapter<Row> | undefined>
  /** 静态数据模式下的本地过滤函数。 */
  localFilter: Ref<AxisTableLocalFilter<Row> | undefined>
  /** 查询参数。 */
  queryParams: Ref<AxisTableQuery | undefined>
  /** 分页配置。 */
  pagination: Ref<PaginationProps | false | undefined>
  /** 页码字段名。 */
  pageField: Ref<string>
  /** 每页数量字段名。 */
  pageSizeField: Ref<string>
  /** 请求前参数处理。 */
  beforeRequest: Ref<
    ((params: AxisTableQuery) => AxisTableQuery | Promise<AxisTableQuery>) | undefined
  >
  /** 请求后列表处理。 */
  afterRequest: Ref<((items: Row[]) => Row[] | Promise<Row[]>) | undefined>
  /** 请求失败后是否保留旧数据。 */
  keepDataOnError: Ref<boolean | undefined>
  /** 是否按远程模式工作；静态 data 场景下 true 表示组件自行分页切片。 */
  remote: Ref<boolean | undefined>
}

const DEFAULT_PAGE_SIZE = 20

function readNumber(value: unknown, fallback = 0): number {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value)
    if (Number.isFinite(parsed)) return parsed
  }
  return fallback
}

function readOptionalNumber(value: unknown): number | undefined {
  return typeof value === 'number' && Number.isFinite(value) ? value : undefined
}

function readRecord(value: unknown): Record<string, unknown> {
  return value && typeof value === 'object' ? (value as Record<string, unknown>) : {}
}

function readArray<Row extends AxisTableRow>(value: unknown): Row[] | null {
  return Array.isArray(value) ? (value as Row[]) : null
}

function resolvePageSize(pagination: PaginationProps | false | undefined): number {
  if (pagination && typeof pagination === 'object') {
    const size = pagination.pageSize
    if (typeof size === 'number' && size > 0) return size
  }
  return DEFAULT_PAGE_SIZE
}

/** 默认响应适配器，兼容常见的 items/list/records/rows/data 分页字段。 */
export function defaultTableRequestAdapter<Row extends AxisTableRow>(
  response: unknown
): AxisTableRequestResult<Row> {
  if (Array.isArray(response)) {
    return {
      items: response as Row[],
      total: response.length
    }
  }

  const responseRecord = readRecord(response)
  const payload = Array.isArray(responseRecord.data)
    ? responseRecord
    : readRecord(responseRecord.data)
  const source = Object.keys(payload).length ? payload : responseRecord
  const items =
    readArray<Row>(source.items) ||
    readArray<Row>(source.list) ||
    readArray<Row>(source.records) ||
    readArray<Row>(source.rows) ||
    readArray<Row>(source.data) ||
    []

  return {
    items,
    total: readNumber(source.total ?? source.Total ?? source.itemCount ?? source.count, items.length),
    page: readOptionalNumber(source.page ?? source.current),
    pageCount: readOptionalNumber(source.pageCount ?? source.pages ?? source.totalPages)
  }
}

/** 管理表格远程请求、分页和 loading 状态。 */
export function useAxisTableRequest<Row extends AxisTableRow = AxisTableRow>(
  options: UseAxisTableRequestOptions<Row>
) {
  const loading = ref(false)
  const error = ref<unknown>(null)
  const tableData = ref([]) as Ref<Row[]>
  const currentPage = ref(1)
  const currentPageSize = ref(resolvePageSize(options.pagination.value))
  const itemCount = ref(0)
  const pageCount = ref(1)
  /** 最近一次有效查询条件，翻页、切换分页大小和静态数据更新时继续复用。 */
  const activeQueryParams = ref<AxisTableQuery>({})
  /** 只允许最后发起的远程请求更新表格状态，避免旧响应覆盖新查询。 */
  let latestRequestId = 0

  function applyStaticData(params: AxisTableQuery = activeQueryParams.value) {
    if (options.request.value) return
    const source = (options.data.value ?? []) as Row[]
    const filterParams = { ...unref(options.queryParams), ...params }
    const filteredSource = options.localFilter.value
      ? options.localFilter.value([...source], filterParams)
      : source
    itemCount.value = filteredSource.length
    pageCount.value = Math.max(
      1,
      Math.ceil(filteredSource.length / Math.max(currentPageSize.value, 1))
    )

    // 关闭分页或用户显式关闭远程模式时，交给 naive-ui 本地分页 / 全量展示。
    if (options.pagination.value === false || options.remote.value === false) {
      tableData.value = filteredSource
      return
    }

    const totalPages = pageCount.value
    if (currentPage.value > totalPages) currentPage.value = totalPages

    const start = (currentPage.value - 1) * currentPageSize.value
    tableData.value = filteredSource.slice(start, start + currentPageSize.value)
  }

  watch([() => options.data.value, () => options.pagination.value], () => applyStaticData(), {
    immediate: true
  })

  const mergedPagination = computed<PaginationProps | false>(() => {
    const pagination = options.pagination.value
    if (pagination === false) return false

    return {
      show: true,
      hideOnSinglePage: false,
      showSizePicker: true,
      pageSizes: [10, 20, 30, 50, 100],
      ...(pagination ?? {}),
      // 展示必须跟实际请求页大小一致，避免 UI 显示 20/页却按 10 条拉取。
      page: currentPage.value,
      pageSize: currentPageSize.value,
      itemCount: itemCount.value,
      pageCount: pageCount.value,
      prefix: info => `共 ${info.itemCount ?? 0} 条`
    }
  })

  function buildPageParams(params: AxisTableQuery = {}): AxisTableQuery {
    if (options.pagination.value === false) return { ...unref(options.queryParams), ...params }

    return {
      [options.pageField.value]: currentPage.value,
      [options.pageSizeField.value]: currentPageSize.value,
      ...unref(options.queryParams),
      ...params
    }
  }

  async function fetch(params?: AxisTableQuery) {
    if (params !== undefined) {
      activeQueryParams.value = { ...params }
    }

    const currentQueryParams = activeQueryParams.value
    const request = options.request.value
    if (!request) {
      applyStaticData(currentQueryParams)
      return
    }

    const requestId = ++latestRequestId
    loading.value = true
    error.value = null
    try {
      const requestParams = options.beforeRequest.value
        ? await options.beforeRequest.value(buildPageParams(currentQueryParams))
        : buildPageParams(currentQueryParams)
      const response = await request(requestParams)
      const adapter = options.requestAdapter.value ?? defaultTableRequestAdapter
      const result = adapter(response)
      const nextItems = options.afterRequest.value
        ? await options.afterRequest.value(result.items)
        : result.items

      if (requestId !== latestRequestId) return

      tableData.value = nextItems
      itemCount.value = result.total
      if (result.page) currentPage.value = result.page
      pageCount.value =
        result.pageCount ?? Math.max(1, Math.ceil(result.total / currentPageSize.value))
    } catch (fetchError) {
      if (requestId !== latestRequestId) return
      error.value = fetchError
      if (!options.keepDataOnError.value) {
        tableData.value = []
        itemCount.value = 0
        pageCount.value = 1
      }
      throw fetchError
    } finally {
      if (requestId === latestRequestId) {
        loading.value = false
      }
    }
  }

  function clearError() {
    error.value = null
  }

  async function reload(params?: AxisTableQuery) {
    await fetch(params)
  }

  async function search(params?: AxisTableQuery) {
    currentPage.value = 1
    await fetch(params)
  }

  async function updatePage(page: number) {
    currentPage.value = page
    await fetch()
  }

  async function updatePageSize(pageSize: number) {
    currentPage.value = 1
    currentPageSize.value = pageSize
    await fetch()
  }

  function getPaginationState(): AxisTablePaginationState {
    return {
      page: currentPage.value,
      pageSize: currentPageSize.value
    }
  }

  function setPaginationState(state: Partial<AxisTablePaginationState>) {
    if (state.page && state.page > 0) currentPage.value = state.page
    if (state.pageSize && state.pageSize > 0) currentPageSize.value = state.pageSize
  }

  return {
    loading,
    error,
    tableData,
    mergedPagination,
    currentPage,
    currentPageSize,
    itemCount,
    fetch,
    reload,
    search,
    updatePage,
    updatePageSize,
    getPaginationState,
    setPaginationState,
    clearError
  }
}
