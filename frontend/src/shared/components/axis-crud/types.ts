import type { PaginationProps } from 'naive-ui'

import type {
  AxisTableColumn,
  AxisTableRequest,
  AxisTableRow,
  AxisTableRowAction
} from '@/shared/components/axis-table'
import type { AxisFormComponent, AxisFormOption, AxisFormResolvable } from '@/shared/components/axis-form'
import type { CrudPermissionSet } from '@/modules/system/permissions/codes'

// 通用 CRUD 表格行（支持树形 parentId/children）
export interface AxisCrudRow extends AxisTableRow {
  id: string
  parentId?: string
  children?: AxisCrudRow[]
  isBuiltin?: boolean
}

// 通用维护表单字段配置
export interface AxisCrudField {
  field: string
  label: string
  component?: AxisFormComponent
  required?: boolean
  defaultValue?: unknown
  options?: AxisFormResolvable<AxisFormOption[]> | 'tree'
  fullWidth?: boolean
  span?: string | number
  help?: string
  componentProps?: Record<string, unknown>
  ifShow?: (model: Record<string, unknown>) => boolean
  disabled?: (model: Record<string, unknown>, editing: boolean) => boolean
}

/** 新增/编辑弹窗分区（配合 FormDialog layout=sectioned）。 */
export interface AxisCrudFormSection {
  key: string
  title?: string
  icon?: string
  description?: string
  columns?: string | number
  fields: AxisCrudField[]
}

/** 分步向导步骤；key 须对应 formSections[].key。 */
export interface AxisCrudFormStep {
  key: string
  title: string
}

// 通用维护页资源描述：各业务模块只声明配置，不实现表格壳
export interface AxisCrudResource<Row extends AxisCrudRow = AxisCrudRow> {
  title: string
  entityName: string
  permissions: CrudPermissionSet
  columns: AxisTableColumn<Row>[]
  /** 扁平字段；与 formSections 二选一，优先 formSections。 */
  fields?: AxisCrudField[]
  /** 分区字段；有值时走卡片分区布局。 */
  formSections?: AxisCrudFormSection[]
  request: AxisTableRequest
  create: (data: Record<string, unknown>) => Promise<unknown>
  update: (id: string, data: Record<string, unknown>) => Promise<unknown>
  remove: (id: string) => Promise<unknown>
  pagination?: PaginationProps | false
  tree?: boolean
  /**
   * 新增/编辑弹窗副标题。
   * 可传固定文案，或按编辑态与当前表单生成。
   */
  formSubtitle?:
    | string
    | ((context: { editing: boolean; model: Record<string, unknown> }) => string | undefined)
  /** 新增/编辑弹窗宽度；默认 760。 */
  formWidth?: number | string
  /** 表单呈现：modal（默认）或 drawer。 */
  /** Passed to FormDialog extraClass for page-specific drawer polish. */
  formExtraClass?: string
  formPresentation?: 'modal' | 'drawer'
  /**
   * 分步向导：有值时同一时间只展示与当前步骤 key 匹配的分区。
   * 底部改为 取消 / 上一步 / 下一步 / 保存；下一步会尽量校验当前步字段。
   * 最终保存时再做整表校验（仅当前可见分区在 DOM 中；前序步已在「下一步」时校验过）。
   */
  formSteps?: AxisCrudFormStep[]
  /** `#before` 插槽位置；`aside` 时预览等面板在表单左侧。 */
  formBeforePlacement?: 'block' | 'aside'
  /** 操作列宽度、行内展示数等；默认 width 130、maxInline 1（多余进更多）。 */
  actionColumn?: {
    width?: number
    maxInline?: number
  }
  /** 插在「编辑」之前的行操作（如详情）。 */
  leadingActions?: AxisTableRowAction<Row>[]
  /** 插在「编辑」与「删除」之间的行操作（如授权）。 */
  extraActions?: AxisTableRowAction<Row>[]
  /** 是否允许新增；默认 true。 */
  canCreate?: boolean | (() => boolean)
  /** 是否允许编辑指定行；默认 true。 */
  canEdit?: (row: Row) => boolean
  /** 是否允许删除指定行；默认 true。 */
  canDelete?: (row: Row) => boolean
  beforeSubmit?: (data: Record<string, unknown>, editing: boolean) => Record<string, unknown> | Promise<Record<string, unknown>>
  /** 打开新增/编辑弹窗并填好默认值之后调用，可异步预加载编码等。 */
  onFormOpen?: (context: {
    editing: boolean
    model: Record<string, unknown>
    /** 编辑时的行主键；新增时为空。 */
    id?: string
  }) => void | Promise<void>
}