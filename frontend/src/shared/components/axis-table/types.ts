import type {
  DataTableColumn,
  DataTableColumns,
  DataTableInst,
  DataTableProps,
  DataTableRowKey
} from 'naive-ui/es/data-table'
import type { FormProps } from 'naive-ui/es/form'
import type { PaginationProps } from 'naive-ui/es/pagination'
import type { SelectOption } from 'naive-ui/es/select'

/** 表格行数据的基础类型，业务侧可以通过泛型继续收窄字段。 */
export type AxisTableRow = Record<string, unknown>

/** 表格查询参数的基础类型。 */
export type AxisTableQuery = Record<string, unknown>

/** 表格请求返回数据经过适配后的统一结构。 */
export interface AxisTableRequestResult<Row extends AxisTableRow = AxisTableRow> {
  /** 当前页列表数据。 */
  items: Row[]
  /** 总数据量。 */
  total: number
  /** 当前页码；不返回时沿用前端当前页码。 */
  page?: number
  /** 总页数；不返回时由 total 和 pageSize 计算。 */
  pageCount?: number
}

/** 表格请求函数，接收查询参数并返回任意后端响应结构。 */
export type AxisTableRequest<Query extends AxisTableQuery = AxisTableQuery> = (
  params: Query
) => Promise<unknown>

/** 表格响应适配器，将不同后端分页结构统一转换为 AxisTableRequestResult。 */
export type AxisTableRequestAdapter<Row extends AxisTableRow = AxisTableRow> = (
  response: unknown
) => AxisTableRequestResult<Row>

/** 静态数据模式下的本地过滤函数。 */
export type AxisTableLocalFilter<Row extends AxisTableRow = AxisTableRow> = (
  rows: Row[],
  params: AxisTableQuery
) => Row[]

/** 字典接口响应结构，兼容常见的 data/result/list/options 包装格式。 */
export type AxisTableDictResponse =
  | SelectOption[]
  | {
      data?: unknown
      result?: unknown
      list?: unknown
      options?: unknown
    }

/** 字典数据源，支持静态数组或异步请求函数。 */
export type AxisTableDictSource = SelectOption[] | (() => Promise<AxisTableDictResponse>)

/** 搜索项组件类型，覆盖后台列表页最常用的轻量查询控件。 */
export type AxisTableSearchComponent = 'input' | 'select' | 'date' | 'dateRange' | 'custom'

/** 搜索区域的自适应网格配置。 */
export interface AxisTableSearchConfig {
  /** 单个搜索项的最小宽度；组件根据实际容器宽度计算列数，默认 220。 */
  minItemWidth?: number
  /** 搜索区最多展示的列数，默认 4。 */
  maxColumns?: number
  /** 收起状态最多展示的行数，默认 1。 */
  collapsedRows?: number
  /** 标签宽度；数字按 px 处理，默认 auto。 */
  labelWidth?: number | string
}

/** 表格搜索项配置。 */
export interface AxisTableSearchSchema {
  /** 查询字段名。 */
  field: string
  /** 表单标签；从列配置推导时可省略，会使用列标题。 */
  label?: string
  /** 搜索控件类型。 */
  component?: AxisTableSearchComponent
  /** 占位文案。 */
  placeholder?: string
  /** 下拉选项，仅 select 生效；优先级高于 dict。 */
  options?: SelectOption[]
  /** 字典数据源，仅 select 生效；支持静态数组或异步函数。 */
  dict?: AxisTableDictSource
  /** dict 的别名，用于兼容 PureTable 一类封装里的 enum 配置。 */
  enum?: AxisTableDictSource
  /** 字典项里作为 label 的字段名。 */
  labelField?: string
  /** 字典项里作为 value 的字段名。 */
  valueField?: string
  /** 查询字段别名，适合表格列字段和接口查询字段不一致的场景。 */
  key?: string
  /** 搜索项排序，数字越小越靠前。 */
  order?: number
  /** 默认值；点击重置时也会恢复到该值。 */
  defaultValue?: unknown
  /** 是否允许清空。 */
  clearable?: boolean
  /** 搜索项宽度。 */
  width?: number | string
  /** 常用条件；收起时优先保留，不受普通条件数量截断。 */
  primary?: boolean
  /** 高级条件；放入“高级筛选”下拉浮层。 */
  advanced?: boolean
  /** 在自适应网格中横跨的列数，默认 1。 */
  span?: number
  /** 是否独占整行。 */
  fullWidth?: boolean
  /** 是否隐藏该搜索项。 */
  hidden?: boolean
  /** 传给具体 Naive UI 控件的额外属性。 */
  componentProps?: Record<string, unknown>
}

/** 列配置里的搜索声明，true 表示按列标题和列 key 自动生成搜索项。 */
export type AxisTableColumnSearch =
  | boolean
  | AxisTableSearchComponent
  | (Omit<AxisTableSearchSchema, 'field' | 'label'> & {
      /** 查询字段名；不传时优先使用 key，再退回到列 key。 */
      field?: string
      /** 表单标签；不传时使用列标题。 */
      label?: string
    })

/** 表格列扩展配置，在 Naive UI 原始列配置上增加权限和业务显隐能力。 */
export type AxisTableColumn<Row extends AxisTableRow = AxisTableRow> = DataTableColumn<Row> & {
  /** 命中任一角色时显示该列；未配置时不限制角色。 */
  roles?: string[]
  /** 命中任一权限码时显示该列；未配置时不限制权限码。 */
  permissions?: string[]
  /** 业务侧动态控制列是否显示。 */
  ifShow?: boolean | ((column: AxisTableColumn<Row>) => boolean)
  /** 是否默认隐藏该列。 */
  hideInTable?: boolean
  /** 是否允许在列设置中控制该列显隐。 */
  configurable?: boolean
  /** 是否从列设置中隐藏该列，语义上等价于 configurable: false。 */
  hideInColumnSetting?: boolean
  /** 是否从搜索表单中隐藏该列级搜索项。 */
  hideInSearch?: boolean
  /** 列级搜索配置，用于直接从 columns 推导搜索表单。 */
  search?: AxisTableColumnSearch
  /** 列级字典配置，可同时用于表格渲染扩展和搜索 select 选项。 */
  dict?: AxisTableDictSource
  /** dict 的别名，用于兼容 PureTable 一类封装里的 enum 配置。 */
  enum?: AxisTableDictSource
  /** 字典项里作为 label 的字段名。 */
  labelField?: string
  /** 字典项里作为 value 的字段名。 */
  valueField?: string
  /** 是否自动把字典值反显为 label。 */
  dictRender?: boolean
}

/** 表格密度，对齐 Naive UI DataTable 的 size。 */
export type AxisTableSize = NonNullable<DataTableProps['size']>

/** 表格行选择模式。 */
export type AxisTableSelectionMode = false | 'single' | 'multiple'

/** 表格行操作按钮风格。 */
export type AxisTableActionType = 'default' | 'primary' | 'info' | 'success' | 'warning' | 'error'

/** 表格行操作按钮配置。 */
export interface AxisTableRowAction<Row extends AxisTableRow = AxisTableRow> {
  /** 操作唯一标识。 */
  key: string
  /** 操作文案。 */
  label: string | ((row: Row) => string)
  /** 操作按钮类型。 */
  type?: AxisTableActionType | ((row: Row) => AxisTableActionType)
  /** 命中任一角色时显示该操作；未配置时不限制角色。 */
  roles?: string[]
  /** 命中任一权限码时显示该操作；未配置时不限制权限码。 */
  permissions?: string[]
  /** 业务侧动态控制操作是否显示。 */
  ifShow?: boolean | ((row: Row) => boolean)
  /** 业务侧动态控制操作是否禁用。 */
  disabled?: boolean | ((row: Row) => boolean)
  /** Iconify 图标名；配置后操作按钮仅显示图标，鼠标悬浮显示 label 文字。 */
  icon?: string
  /** 图标自定义颜色；未配置时根据 type 自动映射。 */
  color?: string
  /** 是否放入更多下拉中。 */
  more?: boolean
  /** 二次确认文案；配置后点击操作会先弹出确认。 */
  confirm?: string | ((row: Row) => string)
  /** 点击操作时触发。 */
  onClick?: (row: Row, rowIndex: number) => void | Promise<void>
}

/** 表格操作列配置。 */
export interface AxisTableActionColumn<Row extends AxisTableRow = AxisTableRow> {
  /** 是否启用操作列。 */
  enabled?: boolean
  /** 操作列标题。 */
  title?: string
  /** 操作列 key。 */
  key?: string
  /** 操作列宽度。 */
  width?: number
  /** 操作列固定位置。 */
  fixed?: 'left' | 'right'
  /** 行内展示的操作数量；默认 1，其余操作自动进入更多下拉。 */
  maxInline?: number
  /** 行操作列表。 */
  actions: AxisTableRowAction<Row>[]
}

/** 表格动作对象，供父组件通过 ref 主动调用。 */
export interface AxisTableAction<Row extends AxisTableRow = AxisTableRow> {
  /** 重新请求数据；传入参数时会合并到当前查询条件。 */
  reload: (params?: AxisTableQuery) => Promise<void>
  /** 重置到第一页后重新请求数据。 */
  search: (params?: AxisTableQuery) => Promise<void>
  /** 设置表格 loading 状态。 */
  setLoading: (loading: boolean) => void
  /** 设置表格数据，适合纯前端临时更新行数据。 */
  setData: (rows: Row[]) => void
  /** 获取当前表格数据。 */
  getData: () => Row[]
  /** 导出当前表格数据为 CSV 文件。 */
  exportCsv: (filename?: string) => void
  /** 获取当前选中行。 */
  getSelectedRows: () => Row[]
  /** 获取当前选中行 key。 */
  getSelectedRowKeys: () => DataTableRowKey[]
  /** 设置当前选中行 key。 */
  setSelectedRowKeys: (keys: DataTableRowKey[]) => void
  /** 清空当前选中行。 */
  clearSelectedRows: () => void
  /** 设置当前可见列 key 列表。 */
  setColumns: (keys: DataTableRowKey[]) => void
  /** 重置列显隐状态。 */
  resetColumns: () => void
  /** 获取当前搜索表单参数。 */
  getSearchParams: () => AxisTableQuery
  /** 设置搜索表单参数。 */
  setSearchParams: (params: AxisTableQuery) => void
  /** 重置搜索表单并触发查询。 */
  resetSearch: () => Promise<void>
  /** 获取当前分页状态。 */
  getPaginationState: () => AxisTablePaginationState
  /** 设置当前分页状态。 */
  setPaginationState: (state: Partial<AxisTablePaginationState>) => void
  /** 获取 Naive UI DataTable 实例。 */
  getTableInstance: () => DataTableInst | null
}

/** 表格分页状态。 */
export interface AxisTablePaginationState {
  /** 当前页码。 */
  page: number
  /** 每页数量。 */
  pageSize: number
}

/** 表格缓存状态。 */
export interface AxisTableCacheState {
  /** 搜索参数。 */
  searchParams?: AxisTableQuery
  /** 表格密度。 */
  size?: AxisTableSize
  /** 分页状态。 */
  pagination?: AxisTablePaginationState
}

/** AxisTable 组件的对外属性。 */
export interface AxisTableProps<Row extends AxisTableRow = AxisTableRow> {
  /** 表格标题。 */
  title?: string
  /** 标题提示文案。 */
  titleTooltip?: string
  /** 表格列配置。 */
  columns: AxisTableColumn<Row>[]
  /** 静态表格数据；存在 request 时优先使用远程请求结果。 */
  data?: Row[]
  /** 远程请求函数。 */
  request?: AxisTableRequest
  /** 请求前参数处理。 */
  beforeRequest?: (params: AxisTableQuery) => AxisTableQuery | Promise<AxisTableQuery>
  /** 请求后列表处理。 */
  afterRequest?: (items: Row[]) => Row[] | Promise<Row[]>
  /** 后端响应适配器。 */
  requestAdapter?: AxisTableRequestAdapter<Row>
  /** 静态数据模式下的本地过滤函数；搜索和分页都会复用过滤后的结果。 */
  localFilter?: AxisTableLocalFilter<Row>
  /** 额外查询参数。 */
  queryParams?: AxisTableQuery
  /** 搜索表单配置；传入后会自动渲染内置搜索区。 */
  searchSchemas?: AxisTableSearchSchema[]
  /** 是否显示搜索区。 */
  showSearch?: boolean
  /** 搜索区表单配置。 */
  searchFormProps?: Partial<FormProps>
  /** 搜索区域的容器自适应网格配置。 */
  searchConfig?: AxisTableSearchConfig
  /** 搜索项较多时是否将高级条件和溢出条件放入“高级筛选”浮层。 */
  searchCollapsible?: boolean
  /** 是否默认使用精简搜索区；设为 false 时全部条件直接展示。 */
  defaultSearchCollapsed?: boolean
  /** 搜索区收起时最多展示的行数；建议改用 searchConfig.collapsedRows。 */
  searchCollapsedRows?: number
  /** @deprecated 建议改用 searchConfig.maxColumns。 */
  searchCols?: number
  /** 是否挂载后立即请求。 */
  immediate?: boolean
  /** 页码字段名。 */
  pageField?: string
  /** 每页数量字段名。 */
  pageSizeField?: string
  /** 行 key。 */
  rowKey?: AxisTableRowKeyResolver<Row>
  /** 行选择模式；false 表示不显示选择列。 */
  selection?: AxisTableSelectionMode
  /** 是否跨页保留已选行。 */
  reserveSelection?: boolean
  /** 默认选中的行 key。 */
  defaultSelectedRowKeys?: DataTableRowKey[]
  /** 选中行变化时触发。 */
  onSelectionChange?: (keys: DataTableRowKey[], rows: Row[]) => void
  /** 操作列配置。 */
  actionColumn?: AxisTableActionColumn<Row>
  /** 分页配置；传 false 时关闭分页。 */
  pagination?: PaginationProps | false
  /** 表格密度。 */
  size?: AxisTableSize
  /** 是否显示斑马纹。 */
  striped?: boolean
  /** 是否显示工具栏。 */
  showToolbar?: boolean
  /** 是否显示刷新按钮。 */
  showRefresh?: boolean
  /** 是否显示密度切换。 */
  showDensity?: boolean
  /** 是否显示列设置。 */
  showColumnSetting?: boolean
  /** 首次加载且暂无数据时是否显示表格骨架屏。 */
  showSkeleton?: boolean
  /** 表格骨架屏行数。 */
  skeletonRows?: number
  /** 空状态文案。 */
  emptyDescription?: string
  /** 请求错误状态文案。 */
  errorDescription?: string
  /** 请求失败后是否保留旧数据。 */
  keepDataOnError?: boolean
  /** 请求失败后是否显示错误状态。 */
  showErrorState?: boolean
  /** 是否按剩余主屏高度给表体封顶；默认开启。抽屉 flexHeight 场景请关掉。 */
  autoHeight?: boolean
  /** 兼容旧参数，自动高度改为按视口剩余空间计算后不再使用。 */
  heightOffset?: number
  /** 列设置持久化 key；传入后会把列显隐保存到 localStorage。 */
  columnSettingKey?: string
  /** 表格状态缓存 key；传入后会缓存搜索条件、分页和密度。 */
  stateStorageKey?: string
  /** 是否开启远程模式。 */
  remote?: boolean
  /** 表格最大高度。 */
  maxHeight?: DataTableProps['maxHeight']
  /** 透传给 Naive UI DataTable 的其他属性。 */
  tableProps?: Partial<DataTableProps>
}

/** 组件内部使用的列配置列表。 */
export type AxisTableColumns<Row extends AxisTableRow = AxisTableRow> = DataTableColumns<Row>

/** 行 key 配置，兼容 Naive UI 的函数写法和常见的字段名写法。 */
export type AxisTableRowKeyResolver<Row extends AxisTableRow = AxisTableRow> =
  | NonNullable<DataTableProps['rowKey']>
  | Extract<keyof Row, string | number>
