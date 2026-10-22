<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { watchDebounced } from '@vueuse/core'
  import { NButton, NTag } from 'naive-ui'

  import { previewCodeRule, type CodePreviewDto } from '@/modules/system/api/code-rule'
  import { useDict } from '@/shared/composables/use-dict'
  import { tryRequest } from '@/shared/http'
  import { Icon } from '@/shared/icons'
  import { message } from '@/shared/utils/message'

  import { codeRuleDictCodes, toPreviewInput } from '../composables/useCodeRulePage'

  defineOptions({ name: 'CodeRulePreview' })

  const props = withDefaults(
    defineProps<{
      model?: Record<string, unknown>
    }>(),
    { model: () => ({}) }
  )

  /* ============ 状态 & 引用 ============ */

  const { getLabel: resetCycleLabel } = useDict(codeRuleDictCodes.resetCycle)
  const { getLabel: checkAlgoLabel } = useDict(codeRuleDictCodes.checkAlgo)
  const { getLabel: sourceTypeLabel } = useDict(codeRuleDictCodes.sourceType)

  const preview = ref<CodePreviewDto | null>(null)
  const errorText = ref('')
  const loading = ref(false)

  /* ============ 视图模型 ============ */

  const signature = computed(() =>
    JSON.stringify({
      prefix: props.model.prefix,
      dateFormat: props.model.dateFormat,
      serialLength: props.model.serialLength,
      resetCycle: props.model.resetCycle,
      template: props.model.template,
      checkAlgo: props.model.checkAlgo,
      sourceType: props.model.sourceType,
      reverseRegex: props.model.reverseRegex
    })
  )

  const sample = computed(() => preview.value?.sample ?? '')

  const parseItems = computed(() =>
    (preview.value?.segments ?? []).map(segment => ({
      kind: segment.name === 'Literal' ? 'sep' : 'seg',
      name: segment.name,
      value: segment.value
    }))
  )

  const summaries = computed(() => {
    const data = preview.value
    if (!data) return []
    return [
      { icon: 'ant-design:column-width-outlined', label: '总长度', value: String(data.length) },
      { icon: 'ant-design:reload-outlined', label: '重置周期', value: resetCycleLabel(data.resetCycle) },
      { icon: 'ant-design:safety-certificate-outlined', label: '校验位', value: checkAlgoLabel(data.checkAlgo) },
      { icon: 'ant-design:apartment-outlined', label: '来源', value: sourceTypeLabel(data.sourceType) }
    ]
  })

  const regexStatus = computed(() => {
    if (errorText.value) return { ok: false, text: errorText.value }
    if (!preview.value) return null
    const pattern = String(props.model.reverseRegex || preview.value.generatedRegex || '').trim()
    if (!pattern) return { ok: false, text: '尚未生成反向正则' }
    try {
      const passed = new RegExp(pattern).test(preview.value.sample)
      return passed
        ? { ok: true, text: '正则校验通过' }
        : { ok: false, text: '正则校验未通过' }
    } catch {
      return { ok: false, text: '反向正则无效' }
    }
  })

  /* ============ 交互操作 ============ */

  watchDebounced(signature, () => void refreshPreview(), { debounce: 280, immediate: true })

  async function refreshPreview() {
    const template = String(props.model.template ?? '').trim()
    if (!template) {
      preview.value = null
      errorText.value = ''
      return
    }

    loading.value = true
    const [data, err] = await tryRequest(previewCodeRule(toPreviewInput(props.model)), {
      silent: true
    })
    loading.value = false

    if (err) {
      preview.value = null
      errorText.value = err.message || '预览失败'
      return
    }

    preview.value = data ?? null
    errorText.value = ''
  }

  async function copySample() {
    if (!sample.value) {
      message.warning('暂无预览编码')
      return
    }
    try {
      await navigator.clipboard.writeText(sample.value)
      message.success('已复制预览编码')
    } catch {
      message.error('复制失败')
    }
  }
</script>

<template>
  <aside class="code-rule-preview" :class="{ 'is-loading': loading }">
    <header class="code-rule-preview__header">
      <span class="code-rule-preview__bar" />
      <Icon icon="ant-design:eye-outlined" class="code-rule-preview__icon" />
      <div class="code-rule-preview__heading">
        <h3 class="code-rule-preview__title">实时预览</h3>
        <p class="code-rule-preview__hint">按当前表单拼示例，不占用流水</p>
      </div>
    </header>

    <section class="code-rule-preview__result" :class="{ 'is-empty': !preview }">
      <div class="code-rule-preview__result-head">
        <span>生成结果</span>
        <NButton
          text
          type="primary"
          size="tiny"
          :disabled="!sample"
          aria-label="复制预览编码"
          @click="copySample"
        >
          <template #icon>
            <Icon icon="ant-design:copy-outlined" />
          </template>
        </NButton>
      </div>
      <p class="code-rule-preview__sample">
        {{ errorText || sample || '填写模板后显示示例' }}
      </p>
    </section>

    <NTag
      v-if="regexStatus"
      size="small"
      round
      :bordered="false"
      :type="regexStatus.ok ? 'success' : 'error'"
    >
      {{ regexStatus.text }}
    </NTag>

    <section v-if="parseItems.length" class="code-rule-preview__block">
      <h4 class="code-rule-preview__label">模板解析</h4>
      <div class="code-rule-preview__parse">
        <template v-for="(item, index) in parseItems" :key="`${item.name}-${index}`">
          <span v-if="item.kind === 'sep'" class="code-rule-preview__sep">
            {{ item.value }}
          </span>
          <div v-else class="code-rule-preview__seg">
            <span>{{ item.name }}</span>
            <strong>{{ item.value || '空' }}</strong>
          </div>
        </template>
      </div>
    </section>

    <section v-if="summaries.length" class="code-rule-preview__block">
      <dl class="code-rule-preview__facts">
        <div v-for="item in summaries" :key="item.label" class="code-rule-preview__fact">
          <dt>
            <Icon :icon="item.icon" class="code-rule-preview__fact-icon" />
            {{ item.label }}
          </dt>
          <dd>{{ item.value }}</dd>
        </div>
      </dl>
    </section>
  </aside>
</template>

<style scoped lang="scss">
  @use '../styles/preview.scss';
</style>
