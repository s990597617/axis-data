<script setup lang="ts">
  import { computed, inject, ref, useSlots } from 'vue'
  import {
    NForm,
    NFormItem,
    NGrid,
    NGridItem,
    NSpin,
    NTooltip,
    type FormInst,
    type FormItemRule
  } from 'naive-ui'

  import { formDialogLayoutKey } from '@/shared/components/form-dialog/context'
  import { Icon } from '@/shared/icons'

  import AxisFormField from './components/axis-form-field.vue'
  import type {
    AxisFormChangeEvent,
    AxisFormExposed,
    AxisFormProps,
    AxisFormSchema,
    AxisFormSection
  } from './types'
  import { createFieldContext, getModelValue, resolveFormValue, setModelValue } from './utils'

  defineOptions({ name: 'AxisForm' })

  const props = withDefaults(defineProps<AxisFormProps>(), {
    schemas: () => [],
    sections: () => [],
    columns: '1 640:2',
    responsive: 'self',
    xGap: 12,
    yGap: 0,
    labelPlacement: 'top',
    size: 'medium',
    disabled: false,
    loading: false,
    showFeedback: true,
    showRequireMark: true,
    formProps: () => ({}),
    beforePlacement: 'block'
  })

  const emit = defineEmits<{
    change: [event: AxisFormChangeEvent]
    submit: [event: Event]
  }>()

  const slots = useSlots()
  const formRef = ref<FormInst | null>(null)
  const dialogLayout = inject(
    formDialogLayoutKey,
    computed(() => 'default' as const)
  )

  const normalizedSections = computed<AxisFormSection[]>(() => {
    const result: AxisFormSection[] = []
    if (props.schemas.length) {
      result.push({ key: '__default', schemas: props.schemas })
    }
    result.push(...props.sections)
    return result.filter(section =>
      typeof section.ifShow === 'function' ? section.ifShow(props.model) : section.ifShow !== false
    )
  })

  const hasSectionCards = computed(() =>
    normalizedSections.value.some(section => Boolean(section.title || section.description))
  )

  // sectioned 布局默认左侧必填标记，业务仍可显式覆盖
  const requireMarkPlacement = computed(
    () =>
      props.requireMarkPlacement ??
      (dialogLayout.value === 'sectioned' ? 'left' : 'right-hanging')
  )

  function getContext(schema: AxisFormSchema) {
    return createFieldContext(props.model, schema, resolveDisabled(schema), value =>
      setValue(schema.field, value)
    )
  }

  function resolveDisabled(schema: AxisFormSchema) {
    const baseContext = createFieldContext(props.model, schema, props.disabled, value =>
      setValue(schema.field, value)
    )
    return props.disabled || resolveFormValue(schema.disabled, baseContext, false)
  }

  function isFieldVisible(schema: AxisFormSchema) {
    return resolveFormValue(schema.ifShow, getContext(schema), true)
  }

  function isFieldRequired(schema: AxisFormSchema) {
    return resolveFormValue(schema.required, getContext(schema), false)
  }

  function getFormItemRule(schema: AxisFormSchema): FormItemRule | FormItemRule[] | undefined {
    if (schema.rules) return schema.rules
    if (!isFieldRequired(schema) || hasGlobalRule(schema.field)) return undefined

    const component = schema.component ?? 'input'
    const isInput = ['input', 'textarea', 'password', 'inputNumber'].includes(component)
    const message = schema.requiredMessage ?? `${isInput ? '请输入' : '请选择'}${schema.label}`

    // 不用 rule.required：async-validator 会把数字 0 / false 当成空（如目录=0、禁用=0）
    return {
      validator: (_rule, value) => {
        if (value === null || value === undefined || value === '') {
          return new Error(message)
        }
        return true
      },
      trigger: isInput ? ['input', 'blur'] : ['change', 'blur']
    }
  }

  function hasGlobalRule(field: string) {
    if (!props.rules) return false
    if (Object.hasOwn(props.rules, field)) return true

    let current: unknown = props.rules
    for (const segment of field.split('.')) {
      if (!current || typeof current !== 'object' || !Object.hasOwn(current, segment)) return false
      current = (current as Record<string, unknown>)[segment]
    }
    return current !== undefined
  }

  function getFormItemProps(schema: AxisFormSchema) {
    return resolveFormValue(schema.formItemProps, getContext(schema), {})
  }

  function getItemSpan(section: AxisFormSection, schema: AxisFormSchema) {
    if (schema.fullWidth) return section.columns ?? props.columns
    return schema.span ?? 1
  }

  function hasSlot(name: string) {
    return Boolean(slots[name])
  }

  function handleFieldChange(field: string, value: unknown, schema: AxisFormSchema) {
    emit('change', { field, value, model: props.model, schema })
  }

  function getValue(field: string) {
    return getModelValue(props.model, field)
  }

  function setValue(field: string, value: unknown) {
    setModelValue(props.model, field, value)
  }

  function validate(...args: Parameters<FormInst['validate']>) {
    if (!formRef.value) return Promise.resolve({ warnings: undefined })
    return formRef.value.validate(...args)
  }

  function restoreValidation() {
    formRef.value?.restoreValidation()
  }

  function getFormInstance() {
    return formRef.value
  }

  function handleSubmit(event: Event) {
    event.preventDefault()
    emit('submit', event)
  }

  defineExpose<AxisFormExposed>({
    validate,
    restoreValidation,
    getFormInstance,
    getValue,
    setValue
  })
</script>

<template>
  <n-spin :show="loading" class="axis-form__spin">
    <n-form
      v-bind="formProps"
      ref="formRef"
      class="axis-form"
      :class="{
        'is-sectioned': hasSectionCards,
        'is-before-aside': props.beforePlacement === 'aside' && hasSlot('before')
      }"
      :model="model"
      :rules="rules"
      :disabled="disabled"
      :size="size"
      :label-placement="labelPlacement"
      :label-width="labelWidth"
      :show-feedback="showFeedback"
      :show-require-mark="showRequireMark"
      :require-mark-placement="requireMarkPlacement"
      @submit="handleSubmit"
    >
      <div v-if="hasSlot('before')" class="axis-form__before">
        <slot name="before" :model="model" />
      </div>

      <div class="axis-form__main">
        <section
          v-for="section in normalizedSections"
          :key="section.key"
          class="axis-form__section"
          :class="{ 'has-header': section.title || section.description }"
        >
        <header v-if="section.title || section.description" class="axis-form__section-header">
          <div class="axis-form__section-heading">
            <span v-if="section.title" class="axis-form__section-bar" />
            <Icon v-if="section.icon" :icon="section.icon" class="axis-form__section-icon" />
            <div class="axis-form__section-copy">
              <h3 v-if="section.title" class="axis-form__section-title">{{ section.title }}</h3>
              <p v-if="section.description" class="axis-form__section-description">
                {{ section.description }}
              </p>
            </div>
          </div>
          <slot
            v-if="hasSlot(`section-${section.key}-extra`)"
            :name="`section-${section.key}-extra`"
            :section="section"
            :model="model"
          />
          <slot v-else name="section-extra" :section="section" :model="model" />
        </header>

        <n-grid
          :cols="section.columns ?? columns"
          :responsive="responsive"
          item-responsive
          :x-gap="xGap"
          :y-gap="yGap"
        >
          <n-grid-item
            v-for="schema in section.schemas.filter(isFieldVisible)"
            :key="schema.field"
            v-bind="schema.gridItemProps"
            :span="getItemSpan(section, schema)"
          >
            <n-form-item
              v-bind="getFormItemProps(schema)"
              :label="hasSlot(`label-${schema.field}`) || hasSlot('label') || schema.help ? undefined : schema.label"
              :path="schema.field"
              :required="isFieldRequired(schema)"
              :show-require-mark="isFieldRequired(schema)"
              :rule="getFormItemRule(schema)"
            >
              <template
                v-if="hasSlot(`label-${schema.field}`) || hasSlot('label') || schema.help"
                #label
              >
                <slot
                  v-if="hasSlot(`label-${schema.field}`)"
                  :name="`label-${schema.field}`"
                  v-bind="getContext(schema)"
                />
                <slot v-else-if="hasSlot('label')" name="label" v-bind="getContext(schema)" />
                <span v-else class="axis-form__label">
                  <span>{{ schema.label }}</span>
                  <NTooltip v-if="schema.help" trigger="hover" placement="top">
                    <template #trigger>
                      <button
                        type="button"
                        class="axis-form__help-trigger"
                        tabindex="-1"
                        aria-label="字段说明"
                        @click.stop.prevent
                      >
                        <Icon icon="ant-design:question-circle-outlined" />
                      </button>
                    </template>
                    {{ schema.help }}
                  </NTooltip>
                </span>
              </template>

              <AxisFormField
                :model="model"
                :schema="schema"
                :form-disabled="disabled"
                @change="handleFieldChange"
              >
                <template
                  v-if="hasSlot(`field-${schema.field}`) || hasSlot('field')"
                  #default="fieldContext"
                >
                  <slot
                    v-if="hasSlot(`field-${schema.field}`)"
                    :name="`field-${schema.field}`"
                    v-bind="fieldContext"
                  />
                  <slot v-else name="field" v-bind="fieldContext" />
                </template>
              </AxisFormField>

              <template v-if="hasSlot(`feedback-${schema.field}`) || hasSlot('feedback')" #feedback>
                <slot
                  v-if="hasSlot(`feedback-${schema.field}`)"
                  :name="`feedback-${schema.field}`"
                  v-bind="getContext(schema)"
                />
                <slot v-else name="feedback" v-bind="getContext(schema)" />
              </template>
            </n-form-item>
          </n-grid-item>
        </n-grid>
      </section>

      <slot name="after" :model="model" />
      </div>
    </n-form>
  </n-spin>
</template>

<style scoped lang="scss">
  .axis-form {
    min-width: 0;

    &.is-sectioned {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    &.is-before-aside {
      display: grid;
      grid-template-columns: minmax(360px, 420px) minmax(0, 1fr);
      align-items: stretch;
      gap: 12px;
      container-type: inline-size;
    }

    @container (max-width: 720px) {
      &.is-before-aside {
        grid-template-columns: minmax(0, 1fr);
      }

      &.is-before-aside &__before {
        height: auto;

        :deep(> *) {
          height: auto;
        }
      }
    }

    &__before,
    &__main {
      min-width: 0;
    }

    &__main {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    &.is-before-aside &__before {
      height: 100%;

      :deep(> *) {
        height: 100%;
      }
    }

    &__spin {
      display: block;

      :deep(.n-spin-content) {
        min-width: 0;
      }
    }

    &__section {
      min-width: 0;

      &.has-header {
        padding: 12px;
        border: 1px solid color-mix(in srgb, var(--axis-border-color) 85%, transparent);
        border-radius: var(--axis-border-radius, 6px);
        background: var(--axis-card-color);
      }

      &:not(.has-header) + &:not(.has-header) {
        margin-top: 12px;
      }
    }

    &__section-header {
      display: flex;
      min-width: 0;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 12px;
      padding-bottom: 12px;
      border-bottom: 1px dashed color-mix(in srgb, var(--axis-border-color) 80%, transparent);
    }

    &__section-heading {
      display: flex;
      min-width: 0;
      flex: 1 1 auto;
      align-items: center;
      gap: 12px;
    }

    &__section-bar {
      width: 3px;
      height: 14px;
      flex: 0 0 auto;
      border-radius: 999px;
      background: linear-gradient(
        180deg,
        var(--axis-primary-color),
        color-mix(in srgb, var(--axis-primary-color) 45%, transparent)
      );
    }

    &__section-icon {
      width: 15px;
      height: 15px;
      flex: 0 0 auto;
      color: color-mix(in srgb, var(--axis-primary-color) 80%, var(--axis-text-color-3));
    }

    &__section-copy {
      min-width: 0;
    }

    &__section-title {
      margin: 0;
      color: var(--axis-text-color-1);
      font-size: 14px;
      line-height: 22px;
    }

    &__section-description {
      margin: 2px 0 0;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 18px;
    }

    &__label {
      display: inline-flex;
      align-items: center;
      gap: 4px;
    }

    &__help-trigger {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 0;
      border: 0;
      background: transparent;
      color: var(--axis-text-color-3);
      font-size: 13px;
      line-height: 1;
      cursor: help;

      &:hover {
        color: var(--axis-primary-color);
      }
    }
  }
</style>
