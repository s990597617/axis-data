<script setup lang="ts">
  import { computed } from 'vue'
  import { NAlert, NButton, NInput, NInputNumber, NSelect, NSwitch } from 'naive-ui'

  import type { ProcessOptionDto } from '@/modules/mdm/api/process'
  import type { SaveRoutingStepInput } from '@/modules/mdm/api/routing'
  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'RoutingStepsField' })

  const props = defineProps<{
    value: unknown
    disabled?: boolean
    setValue: (value: unknown) => void
    processOptions: ProcessOptionDto[]
  }>()

  /* ============ 派生数据 ============ */

  const steps = computed(() => (Array.isArray(props.value) ? (props.value as SaveRoutingStepInput[]) : []))

  const selectOptions = computed(() =>
    props.processOptions.map(item => ({
      label: `${item.code} ${item.name}`,
      value: String(item.id)
    }))
  )

  /* ============ 交互操作 ============ */

  function emit(next: SaveRoutingStepInput[]) {
    props.setValue(next)
  }

  function asProcessId(value: unknown) {
    if (value == null || value === '' || value === 0 || value === '0') return ''
    return String(value)
  }

  function patch(index: number, patchValue: Partial<SaveRoutingStepInput>) {
    emit(steps.value.map((item, i) => (i === index ? { ...item, ...patchValue } : item)))
  }

  function addRow() {
    const maxSeq = steps.value.reduce((max, item) => Math.max(max, Number(item.seq) || 0), 0)
    emit([
      ...steps.value,
      {
        seq: maxSeq + 10,
        processId: '',
        standardMinutes: 0,
        isReportPoint: true,
        remark: ''
      }
    ])
  }

  function removeRow(index: number) {
    emit(steps.value.filter((_, i) => i !== index))
  }
</script>

<template>
  <div class="routing-steps-field">
    <NAlert class="routing-steps-field__hint" type="info" :bordered="false">
      至少一道工序，且至少一个报工点；序号建议 10 / 20 / 30
    </NAlert>
    <div v-if="steps.length" class="routing-steps-field__table">
      <div class="routing-steps-field__head">
        <span>序号</span>
        <span>工序</span>
        <span>工时(分)</span>
        <span>报工</span>
        <span>备注</span>
        <span />
      </div>
      <div v-for="(row, index) in steps" :key="index" class="routing-steps-field__row">
        <NInputNumber
          :value="row.seq"
          :disabled="disabled"
          :min="1"
          :max="9999"
          :show-button="false"
          placeholder="序号"
          @update:value="value => patch(index, { seq: Number(value) || 1 })"
        />
        <NSelect
          :value="asProcessId(row.processId) || null"
          :options="selectOptions"
          :disabled="disabled"
          filterable
          clearable
          placeholder="选择工序"
          @update:value="value => patch(index, { processId: asProcessId(value) })"
        />
        <NInputNumber
          :value="row.standardMinutes"
          :disabled="disabled"
          :min="0"
          :max="99999"
          :show-button="false"
          placeholder="分钟"
          @update:value="value => patch(index, { standardMinutes: Number(value) || 0 })"
        />
        <NSwitch
          :value="row.isReportPoint !== false"
          :disabled="disabled"
          @update:value="value => patch(index, { isReportPoint: value })"
        />
        <NInput
          :value="row.remark ?? ''"
          :disabled="disabled"
          maxlength="256"
          placeholder="可选"
          @update:value="value => patch(index, { remark: value })"
        />
        <NButton
          quaternary
          type="error"
          :disabled="disabled"
          :focusable="false"
          @click="removeRow(index)"
        >
          <template #icon>
            <Icon icon="ant-design:delete-outlined" />
          </template>
        </NButton>
      </div>
    </div>
    <div v-else class="routing-steps-field__empty">尚未添加工序</div>
    <NButton
      class="routing-steps-field__add"
      dashed
      :disabled="disabled"
      :focusable="false"
      @click="addRow"
    >
      <template #icon>
        <Icon icon="ant-design:plus-outlined" />
      </template>
      添加工序
    </NButton>
  </div>
</template>

<style scoped lang="scss">
  @use '../styles/steps-field.scss';
</style>
