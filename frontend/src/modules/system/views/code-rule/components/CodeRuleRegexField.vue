<script setup lang="ts">
  import { ref } from 'vue'
  import { NButton, NInput, NInputGroup } from 'naive-ui'

  import { generateCodeRuleRegex } from '@/modules/system/api/code-rule'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'

  import { toPreviewInput } from '../composables/useCodeRulePage'

  defineOptions({ name: 'CodeRuleRegexField' })

  const props = defineProps<{
    value: unknown
    disabled?: boolean
    model: Record<string, unknown>
    setValue: (value: unknown) => void
  }>()

  /* ============ 状态 & 引用 ============ */

  const generating = ref(false)

  /* ============ 交互操作 ============ */

  async function generateRegex() {
    generating.value = true
    const [data, err] = await tryRequest(generateCodeRuleRegex(toPreviewInput(props.model)), {
      fallbackMessage: '生成正则失败'
    })
    generating.value = false
    if (err || !data?.pattern) return
    props.setValue(data.pattern)
    message.success('已按模板生成反向正则')
  }
</script>

<template>
  <div class="code-rule-regex-field">
    <NInputGroup>
      <NInput
        :value="typeof value === 'string' ? value : ''"
        :disabled="disabled"
        maxlength="512"
        placeholder="用于扫码解析 / 外来码校验"
        @update:value="setValue"
      />
      <NButton type="primary" :disabled="disabled" :loading="generating" @click="generateRegex">
        按模板生成
      </NButton>
    </NInputGroup>
    <p class="code-rule-regex-field__hint">与模板联动，可留空保存时自动生成，也可手工覆盖</p>
  </div>
</template>

<style scoped lang="scss">
  @use '../styles/regex-field.scss';
</style>
