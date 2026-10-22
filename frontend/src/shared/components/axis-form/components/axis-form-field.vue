<script setup lang="ts">
  import { computed } from 'vue'
  import {
    NCheckbox,
    NCheckboxGroup,
    NDatePicker,
    NInput,
    NInputNumber,
    NRadio,
    NRadioGroup,
    NSelect,
    NSpace,
    NSwitch,
    NTreeSelect,
    type SelectOption,
    type TreeSelectOption
  } from 'naive-ui'

  import type {
    AxisFormFieldContext,
    AxisFormModel,
    AxisFormOption,
    AxisFormSchema
  } from '../types'
  import { createFieldContext, getModelValue, resolveFormValue, setModelValue } from '../utils'

  defineOptions({ name: 'AxisFormField' })

  const props = defineProps<{
    model: AxisFormModel
    schema: AxisFormSchema
    formDisabled?: boolean
  }>()

  const emit = defineEmits<{
    change: [field: string, value: unknown, schema: AxisFormSchema]
  }>()

  defineSlots<{
    default?: (props: AxisFormFieldContext) => unknown
  }>()

  const componentType = computed(() => props.schema.component ?? 'input')
  const value = computed(() => getModelValue(props.model, props.schema.field))

  const baseContext = computed(() =>
    createFieldContext(props.model, props.schema, Boolean(props.formDisabled), updateValue)
  )

  const disabled = computed(
    () =>
      Boolean(props.formDisabled) ||
      resolveFormValue(props.schema.disabled, baseContext.value, false)
  )

  const context = computed(() =>
    createFieldContext(props.model, props.schema, disabled.value, updateValue)
  )

  const options = computed(() =>
    resolveFormValue(props.schema.options, context.value, [] as AxisFormOption[])
  )

  const selectOptions = computed(() => options.value as SelectOption[])
  const treeOptions = computed(() => options.value as TreeSelectOption[])
  const componentProps = computed(() => ({
    ...resolveFormValue(props.schema.componentProps, context.value, {}),
    disabled: disabled.value
  }))

  const placeholder = computed(() => {
    const action = ['input', 'textarea', 'password', 'inputNumber'].includes(componentType.value)
      ? '请输入'
      : '请选择'
    return `${action}${props.schema.label}`
  })

  const inputValue = computed(() => (typeof value.value === 'string' ? value.value : null))
  const numberValue = computed(() => (typeof value.value === 'number' ? value.value : null))
  const scalarValue = computed(() => {
    const current = value.value
    return typeof current === 'string' ||
      typeof current === 'number' ||
      typeof current === 'boolean'
      ? current
      : null
  })
  const switchValue = computed(() => scalarValue.value ?? undefined)
  const selectValue = computed(() => {
    const current = value.value
    if (typeof current === 'string' || typeof current === 'number' || Array.isArray(current)) {
      return current as string | number | Array<string | number>
    }
    return null
  })
  const treeValue = computed(() => {
    const current = value.value
    if (typeof current === 'string' || typeof current === 'number' || Array.isArray(current)) {
      return current as string | number | string[] | number[]
    }
    return null
  })
  const dateRangeValue = computed(() => {
    const current = value.value
    return Array.isArray(current) && current.length === 2 ? (current as [number, number]) : null
  })
  const checkboxValue = computed(() =>
    Array.isArray(value.value) ? (value.value as Array<string | number>) : []
  )

  function updateValue(nextValue: unknown) {
    setModelValue(props.model, props.schema.field, nextValue)
    emit('change', props.schema.field, nextValue, props.schema)
  }

  function optionValue(option: AxisFormOption) {
    return option.value ?? option.key ?? ''
  }

  function optionLabel(option: AxisFormOption) {
    return option.label ?? String(optionValue(option))
  }
</script>

<template>
  <div class="axis-form-field">
    <slot v-if="$slots.default" v-bind="context" />

    <template v-else>
      <n-input
        v-if="componentType === 'input'"
        :value="inputValue"
        :placeholder="placeholder"
        clearable
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-input
        v-else-if="componentType === 'textarea'"
        :value="inputValue"
        type="textarea"
        :placeholder="placeholder"
        :rows="3"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-input
        v-else-if="componentType === 'password'"
        :value="inputValue"
        type="password"
        show-password-on="click"
        :placeholder="placeholder"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-input-number
        v-else-if="componentType === 'inputNumber'"
        :value="numberValue"
        :placeholder="placeholder"
        clearable
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-select
        v-else-if="componentType === 'select'"
        :value="selectValue"
        :options="selectOptions"
        :placeholder="placeholder"
        clearable
        :to="true"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-tree-select
        v-else-if="componentType === 'treeSelect'"
        :value="treeValue"
        :options="treeOptions"
        :placeholder="placeholder"
        clearable
        :to="true"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-date-picker
        v-else-if="componentType === 'date'"
        :value="numberValue"
        type="date"
        :placeholder="placeholder"
        clearable
        :to="true"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-date-picker
        v-else-if="componentType === 'dateRange'"
        :value="dateRangeValue"
        type="daterange"
        clearable
        :to="true"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <n-radio-group
        v-else-if="componentType === 'radio'"
        :value="scalarValue"
        v-bind="componentProps"
        @update:value="updateValue"
      >
        <n-space :size="16">
          <n-radio
            v-for="option in options"
            :key="String(optionValue(option))"
            :value="optionValue(option)"
            :disabled="disabled || option.disabled"
          >
            {{ optionLabel(option) }}
          </n-radio>
        </n-space>
      </n-radio-group>
      <n-checkbox-group
        v-else-if="componentType === 'checkbox'"
        :value="checkboxValue"
        v-bind="componentProps"
        @update:value="updateValue"
      >
        <n-space :size="16">
          <n-checkbox
            v-for="option in options"
            :key="String(optionValue(option))"
            :value="optionValue(option) as string | number"
            :disabled="disabled || option.disabled"
          >
            {{ optionLabel(option) }}
          </n-checkbox>
        </n-space>
      </n-checkbox-group>
      <n-switch
        v-else-if="componentType === 'switch'"
        :value="switchValue"
        v-bind="componentProps"
        @update:value="updateValue"
      />
      <div v-else class="axis-form-field__missing">请为“{{ schema.label }}”提供字段插槽</div>
    </template>
  </div>
</template>

<style scoped lang="scss">
  .axis-form-field {
    width: 100%;
    min-width: 0;

    :deep(.n-input-number),
    :deep(.n-date-picker),
    :deep(.n-select),
    :deep(.n-tree-select) {
      width: 100%;
    }

    &__missing {
      display: flex;
      min-height: 34px;
      align-items: center;
      color: var(--axis-text-color-3);
      font-size: 13px;
    }
  }
</style>
