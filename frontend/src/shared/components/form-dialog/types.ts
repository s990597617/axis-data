import type { Component, CSSProperties } from 'vue'

/** FormDialog 状态主题。 */
export type FormDialogTheme = 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info'

/**
 * 布局语义：
 * - default：普通表单（较大内边距）
 * - sectioned：分区卡片（紧凑 12px 间距 + 浅色底，配合 AxisForm sections 使用）
 */
export type FormDialogLayout = 'default' | 'sectioned'

/** 呈现方式：居中弹窗或侧滑抽屉。 */
export type FormDialogPresentation = 'modal' | 'drawer'

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
  /** 布局：普通或分区；分区建议配 sectioned。 */
  layout?: FormDialogLayout
  /** 呈现：modal（默认）或 drawer。 */
  presentation?: FormDialogPresentation
  /** drawer 时的方位；默认 right。 */
  drawerPlacement?: 'right' | 'left'
  /** 主体区域额外样式，会与 maxBodyHeight / layout 合并。 */
  bodyStyle?: CSSProperties
  loading?: boolean
  /** 仅 modal 生效。 */
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