<script setup lang="ts">
  import { NButton, NInput } from 'naive-ui'

  defineOptions({ name: 'CodeRuleTemplateField' })

  const props = defineProps<{
    value: unknown
    disabled?: boolean
    setValue: (value: unknown) => void
  }>()

  /* ============ 常量 ============ */

  const placeholderTokens = [
    { token: '{Prefix}', label: '前缀' },
    { token: '{Date}', label: '日期' },
    { token: '{Serial}', label: '流水号' },
    { token: '{Material}', label: '物料编码' },
    { token: '{Line}', label: '产线' },
    { token: '{Shift}', label: '班次' },
    { token: '{Check}', label: '校验位' }
  ]

  /* ============ 交互操作 ============ */

  function insertToken(token: string) {
    if (props.disabled) return
    const current = typeof props.value === 'string' ? props.value : ''
    props.setValue(current + token)
  }
</script>

<template>
  <div class="code-rule-template-field">
    <NInput
      :value="typeof value === 'string' ? value : ''"
      :disabled="disabled"
      maxlength="128"
      placeholder="{Prefix}-{Date}-{Serial}"
      @update:value="setValue"
    />
    <div class="code-rule-template-field__chips">
      <NButton
        v-for="item in placeholderTokens"
        :key="item.token"
        size="tiny"
        secondary
        :disabled="disabled"
        @click="insertToken(item.token)"
      >
        {{ item.token }} {{ item.label }}
      </NButton>
    </div>
  </div>
</template>

<style scoped lang="scss">
  @use '../styles/template-field.scss';
</style>
