import type {
  AxisFormFieldContext,
  AxisFormModel,
  AxisFormResolvable,
  AxisFormSchema
} from './types'

const unsafePathSegments = new Set(['__proto__', 'prototype', 'constructor'])

export function getModelValue(model: AxisFormModel, field: string): unknown {
  return toPath(field).reduce<unknown>((value, segment) => {
    if (!isRecord(value)) return undefined
    return value[segment]
  }, model)
}

export function setModelValue(model: AxisFormModel, field: string, value: unknown) {
  const segments = toPath(field)
  const lastSegment = segments.pop()
  if (!lastSegment) return

  let target = model
  segments.forEach(segment => {
    const current = target[segment]
    if (!isRecord(current)) target[segment] = {}
    target = target[segment] as AxisFormModel
  })
  target[lastSegment] = value
}

export function createFieldContext(
  model: AxisFormModel,
  schema: AxisFormSchema,
  disabled: boolean,
  setValue: (value: unknown) => void
): AxisFormFieldContext {
  return {
    model,
    schema,
    value: getModelValue(model, schema.field),
    disabled,
    setValue
  }
}

export function resolveFormValue<T>(
  value: AxisFormResolvable<T> | undefined,
  context: AxisFormFieldContext,
  fallback: T
): T {
  if (typeof value === 'function') {
    return (value as (context: AxisFormFieldContext) => T)(context)
  }
  return value ?? fallback
}

function toPath(field: string) {
  return field
    .split('.')
    .map(segment => segment.trim())
    .filter(segment => segment && !unsafePathSegments.has(segment))
}

function isRecord(value: unknown): value is AxisFormModel {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}
