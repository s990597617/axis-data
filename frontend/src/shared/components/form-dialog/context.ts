import type { ComputedRef, InjectionKey } from 'vue'

import type { FormDialogLayout } from './types'

export const formDialogLayoutKey: InjectionKey<ComputedRef<FormDialogLayout>> =
  Symbol('axis-form-dialog-layout')
