import type { Component, CSSProperties } from 'vue'

/** FormDialog 状态主题。 */
export type FormDialogTheme = 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'

/**
 * 内容区布局：
 * - default：普通表单（较大内边距）
 * - sectioned：分区卡片表单（12px 间距 + 浅色底），配合 AxisForm sections 使用
 */
export type FormDialogLayout = 'default' | 'sectioned'

/** FormDialog 属性。 */
export interface FormDialogProps {
  modelValue?: boolean
  title?: string
  subtitle?: string
  icon?: Component | string
  iconTheme?: Exclude<FormDialogTheme, 'default'>
  statusText?: string
  statusType?: FormDialogTheme
  width?: string | number
  maxBodyHeight?: string | number
  /** 内容区布局模板；分区表单请用 sectioned。 */
  layout?: FormDialogLayout
  /** 内容区附加样式，会与 maxBodyHeight / layout 合并。 */
  bodyStyle?: CSSProperties
  loading?: boolean
  draggable?: boolean
  maskClosable?: boolean
  closeOnEsc?: boolean
  destroyOnClose?: boolean
  showFooter?: boolean
  showCancel?: boolean
  showConfirm?: boolean
  cancelText?: string
  confirmText?: string
  confirmLoading?: boolean
  confirmDisabled?: boolean
  extraClass?: string
}

/** FormDialog 暴露的方法。 */
export interface FormDialogExposed {
  open: () => void
  close: () => void
}
