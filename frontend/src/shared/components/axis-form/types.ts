import type { FormInst, FormItemProps, FormItemRule, FormProps, FormRules } from 'naive-ui'

/** AxisForm 表单数据。 */
export type AxisFormModel = Record<string, unknown>

/** AxisForm 内置字段类型。 */
export type AxisFormComponent =
  | 'input'
  | 'textarea'
  | 'password'
  | 'inputNumber'
  | 'select'
  | 'treeSelect'
  | 'date'
  | 'dateRange'
  | 'radio'
  | 'checkbox'
  | 'switch'
  | 'custom'

/** 兼容选择器、树选择器和单复选项的通用选项。 */
export interface AxisFormOption {
  label?: string
  value?: string | number | boolean
  key?: string | number
  disabled?: boolean
  children?: AxisFormOption[]
  [key: string]: unknown
}

/** 字段插槽与动态配置函数接收的上下文。 */
export interface AxisFormFieldContext {
  model: AxisFormModel
  schema: AxisFormSchema
  value: unknown
  disabled: boolean
  setValue: (value: unknown) => void
}

/** 支持静态值或根据当前表单状态动态计算的值。 */
export type AxisFormResolvable<T> = T | ((context: AxisFormFieldContext) => T)

/** AxisForm 字段配置。 */
export interface AxisFormSchema {
  /** 字段路径，支持 `profile.name` 形式的嵌套路径。 */
  field: string
  /** 表单项标签。 */
  label: string
  /** 内置控件类型，默认 input。 */
  component?: AxisFormComponent
  /** 控件选项，也可以根据当前表单动态生成。 */
  options?: AxisFormResolvable<AxisFormOption[]>
  /** 透传给内置控件的属性。 */
  componentProps?: AxisFormResolvable<Record<string, unknown>>
  /** 透传给 NFormItem 的属性。 */
  formItemProps?: AxisFormResolvable<Partial<FormItemProps>>
  /** 字段校验规则；未配置时 required 会自动生成基础规则。 */
  rules?: FormItemRule | FormItemRule[]
  /** 是否必填。 */
  required?: AxisFormResolvable<boolean>
  /** required 自动校验时使用的提示文案。 */
  requiredMessage?: string
  /** 是否显示字段。 */
  ifShow?: AxisFormResolvable<boolean>
  /** 是否禁用字段。 */
  disabled?: AxisFormResolvable<boolean>
  /** 字段说明；显示在标签旁问号图标的悬停提示中。 */
  help?: string
  /** 栅格占位。 */
  span?: string | number
  /** 是否独占整行，会自动适配响应式列数。 */
  fullWidth?: boolean
  /** 透传给 NGridItem 的属性（如 style / class，可用于跨行）。 */
  gridItemProps?: Record<string, unknown>
}

/** AxisForm 表单分组。 */
export interface AxisFormSection {
  /** 分组唯一标识，用于渲染和 section-extra 插槽。 */
  key: string
  /** 分组标题；不配置时仅渲染字段。 */
  title?: string
  /** 分组标题前的 Iconify 线性图标。 */
  icon?: string
  /** 分组辅助说明。 */
  description?: string
  /** 分组字段。 */
  schemas: AxisFormSchema[]
  /** 覆盖表单默认列数。 */
  columns?: string | number
  /** 是否显示分组。 */
  ifShow?: boolean | ((model: AxisFormModel) => boolean)
}

/** AxisForm 属性。 */
export interface AxisFormProps {
  model: AxisFormModel
  schemas?: AxisFormSchema[]
  sections?: AxisFormSection[]
  rules?: FormRules
  columns?: string | number
  responsive?: 'self' | 'screen'
  xGap?: number
  yGap?: number
  labelPlacement?: FormProps['labelPlacement']
  labelWidth?: FormProps['labelWidth']
  size?: FormProps['size']
  disabled?: boolean
  loading?: boolean
  showFeedback?: boolean
  showRequireMark?: boolean
  requireMarkPlacement?: FormProps['requireMarkPlacement']
  formProps?: Partial<FormProps>
  /** `#before` 插槽位置；`aside` 时在分区左侧，适合预览面板。 */
  beforePlacement?: 'block' | 'aside'
}

/** 字段值变化事件。 */
export interface AxisFormChangeEvent {
  field: string
  value: unknown
  model: AxisFormModel
  schema: AxisFormSchema
}

/** AxisForm 暴露的方法。 */
export interface AxisFormExposed {
  validate: FormInst['validate']
  restoreValidation: FormInst['restoreValidation']
  getFormInstance: () => FormInst | null
  getValue: (field: string) => unknown
  setValue: (field: string, value: unknown) => void
}
