<script setup lang="ts">
  import { computed } from 'vue'
  import type { SelectOption } from 'naive-ui/es/select'

  import type { AxisTableSearchSchema } from '../types'

  defineOptions({ name: 'AxisTableSearchField' })

  const props = defineProps<{
    schema: AxisTableSearchSchema
    value: unknown
    options: SelectOption[]
    loading?: boolean
    labelWidth: number | string
  }>()

  const emit = defineEmits<{
    'update:value': [value: unknown]
    submit: []
  }>()

  const labelStyle = computed(() => {
    if (props.labelWidth === 'auto') return undefined
    const width = toCssSize(props.labelWidth)
    return { width, flex: `0 0 ${width}` }
  })

  const controlStyle = computed(() =>
    props.schema.width ? { width: toCssSize(props.schema.width) } : undefined
  )

  const stringValue = computed(() => (typeof props.value === 'string' ? props.value : null))
  const selectValue = computed(() =>
    typeof props.value === 'string' || typeof props.value === 'number' ? props.value : null
  )
  const numberValue = computed(() => (typeof props.value === 'number' ? props.value : null))
  const numberRangeValue = computed(() =>
    Array.isArray(props.value) && props.value.length === 2
      ? (props.value as [number, number])
      : null
  )

  function toCssSize(value: number | string) {
    return typeof value === 'number' ? `${value}px` : value
  }
</script>

<template>
  <div class="axis-table-search-field">
    <div class="axis-table-search-field__label" :style="labelStyle">
      {{ schema.label }}
    </div>
    <div class="axis-table-search-field__control" :style="controlStyle">
      <slot name="custom">
        <n-input
          v-if="!schema.component || schema.component === 'input'"
          :value="stringValue"
          :placeholder="schema.placeholder ?? `请输入${schema.label}`"
          :clearable="schema.clearable ?? true"
          style="width: 100%"
          v-bind="schema.componentProps"
          @update:value="value => emit('update:value', value)"
          @keyup.enter="emit('submit')"
        />
        <n-select
          v-else-if="schema.component === 'select'"
          :value="selectValue"
          :options="options"
          :loading="loading"
          :placeholder="schema.placeholder ?? `请选择${schema.label}`"
          :clearable="schema.clearable ?? true"
          :to="true"
          style="width: 100%"
          v-bind="schema.componentProps"
          @update:value="value => emit('update:value', value)"
        />
        <n-date-picker
          v-else-if="schema.component === 'date'"
          :value="numberValue"
          type="date"
          :placeholder="schema.placeholder ?? `请选择${schema.label}`"
          :clearable="schema.clearable ?? true"
          :to="true"
          style="width: 100%"
          v-bind="schema.componentProps"
          @update:value="value => emit('update:value', value)"
        />
        <n-date-picker
          v-else-if="schema.component === 'dateRange'"
          :value="numberRangeValue"
          type="daterange"
          :clearable="schema.clearable ?? true"
          :to="true"
          style="width: 100%"
          v-bind="schema.componentProps"
          @update:value="value => emit('update:value', value)"
        />
      </slot>
    </div>
  </div>
</template>

<style scoped lang="scss">
  .axis-table-search-field {
    display: flex;
    min-width: 0;
    min-height: 30px;
    align-items: center;
    gap: 8px;

    &__label {
      display: inline-flex;
      height: 30px;
      align-items: center;
      color: var(--axis-text-color-2);
      font-size: 13px;
      white-space: nowrap;
    }

    &__control {
      display: inline-flex;
      max-width: 100%;
      min-width: 0;
      min-height: 30px;
      flex: 1 1 auto;
      align-items: center;
    }
  }
</style>
