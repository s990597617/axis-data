/** 通用状态主题。 */
export type DetailTheme = 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'

/** 详情字段允许直接展示的值。 */
export type DetailFieldValue = string | number | boolean | null | undefined | string[]

/** 详情字段。 */
export interface DetailField {
  /** 字段唯一标识；使用 field 插槽时可据此区分字段。 */
  key?: string
  /** 字段标签。 */
  label: string
  /** 字段值。 */
  value?: DetailFieldValue
  /** 标签前的 Iconify 图标名。 */
  icon?: string
  /** 展示类型。code 适合短编码；pre 适合堆栈 / JSON 等多行代码块。 */
  type?: 'text' | 'code' | 'pre' | 'tag' | 'image'
  /** type 为 tag 时的标签主题。 */
  tagType?: DetailTheme
  /** 占据的网格列数。 */
  span?: number
  /** 是否独占整行。 */
  fullRow?: boolean
  /** 空值文案，默认“-”。 */
  emptyText?: string
  /** 单行省略，悬停显示全文（适合长类型名、长编码等）。 */
  ellipsis?: boolean
  /** 字段格式化函数。 */
  formatter?: (value: DetailFieldValue, field: DetailField) => string | number | null | undefined
}

/** 详情信息分组。 */
export interface DetailBlock {
  /** 分组标题。 */
  title: string
  /** 分组标题后的辅助说明。 */
  description?: string
  /** 分组字段。 */
  fields: DetailField[]
}

/** DetailDrawer 属性。 */
export interface DetailDrawerProps {
  show?: boolean
  title?: string
  subtitle?: string
  icon?: string
  iconTheme?: Exclude<DetailTheme, 'default'>
  statusText?: string
  statusType?: DetailTheme
  width?: string | number
  loading?: boolean
  fields?: DetailField[]
  blocks?: DetailBlock[]
  sectionTitle?: string
  columns?: number
  labelWidth?: string | number
  maskClosable?: boolean
  closeOnEsc?: boolean
  destroyOnClose?: boolean
  showFooter?: boolean
  closeText?: string
  /** 内容区撑满抽屉高度（适合下方大预览/编辑区）。 */
  fillContent?: boolean
}

/** DetailDrawer 暴露的方法。 */
export interface DetailDrawerExposed {
  open: () => void
  close: () => void
}
