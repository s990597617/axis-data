export interface ExcelImportFailure {
  row: number
  message: string
}

export interface ExcelImportResult {
  successCount: number
  failureCount: number
  failures: ExcelImportFailure[]
}

export interface ExcelImportDialogProps {
  modelValue?: boolean
  title?: string
  tip?: string
  accept?: string
  templateButtonText?: string
  confirmText?: string
  maxSizeMb?: number
}
