<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import CodeRulePreview from './components/CodeRulePreview.vue'
  import CodeRuleRegexField from './components/CodeRuleRegexField.vue'
  import CodeRuleTemplateField from './components/CodeRuleTemplateField.vue'
  import { useCodeRulePage } from './composables/useCodeRulePage'

  defineOptions({ name: 'SysCodeRule' })

  /* ============ 状态 & 引用 ============ */

  const { resource } = useCodeRulePage()
</script>

<template>
  <div class="code-rule-page">
    <AxisCrudPage :resource="resource">
      <template #before="{ model }">
        <CodeRulePreview :model="model" />
      </template>
      <template #field-template="{ value, setValue, disabled }">
        <CodeRuleTemplateField :value="value" :disabled="disabled" :set-value="setValue" />
      </template>
      <template #field-reverseRegex="{ value, setValue, disabled, model }">
        <CodeRuleRegexField
          :value="value"
          :disabled="disabled"
          :model="model"
          :set-value="setValue"
        />
      </template>
    </AxisCrudPage>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
