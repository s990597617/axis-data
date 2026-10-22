<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import RoutingCodeField from './components/RoutingCodeField.vue'
  import RoutingStepsField from './components/RoutingStepsField.vue'
  import { useRoutingPage } from './composables/useRoutingPage'

  defineOptions({ name: 'MdmRouting' })

  /* ============ 状态 & 引用 ============ */

  const { resource, previewNextCode, markCodeManual, processOptions } = useRoutingPage()
</script>

<template>
  <div class="routing-page">
    <AxisCrudPage :resource="resource">
      <template #field-code="{ value, setValue, disabled, editing }">
        <RoutingCodeField
          :value="value"
          :disabled="disabled"
          :editing="editing"
          :set-value="setValue"
          :preview="previewNextCode"
          :on-manual-edit="markCodeManual"
        />
      </template>
      <template #field-steps="{ value, setValue, disabled }">
        <RoutingStepsField
          :value="value"
          :disabled="disabled"
          :set-value="setValue"
          :process-options="processOptions"
        />
      </template>
    </AxisCrudPage>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
