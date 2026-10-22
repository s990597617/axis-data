<script setup lang="ts">
  import { ref } from 'vue'
  import { NButton, NInput, NInputGroup } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'MaterialCodeField' })

  const props = defineProps<{
    value: unknown
    disabled?: boolean
    editing?: boolean
    setValue: (value: unknown) => void
    preview: () => Promise<string | undefined>
    onManualEdit: () => void
  }>()

  /* ============ 状态 & 引用 ============ */

  const loading = ref(false)

  /* ============ 交互操作 ============ */

  function onInput(next: string) {
    props.onManualEdit()
    props.setValue(next)
  }

  async function refresh() {
    loading.value = true
    const code = await props.preview()
    loading.value = false
    if (code) props.setValue(code)
  }
</script>

<template>
  <div class="material-code-field">
    <NInputGroup>
      <NInput
        :value="typeof value === 'string' ? value : ''"
        :disabled="disabled"
        :loading="loading"
        maxlength="64"
        clearable
        placeholder="留空则按规则 MAT 自动编号"
        @update:value="onInput"
      />
      <NButton
        v-if="!editing"
        secondary
        :disabled="disabled"
        :loading="loading"
        :focusable="false"
        @click="refresh"
      >
        <template #icon>
          <Icon icon="ant-design:reload-outlined" />
        </template>
      </NButton>
    </NInputGroup>
  </div>
</template>

<style scoped lang="scss">
  @use '../styles/code-field.scss';
</style>
