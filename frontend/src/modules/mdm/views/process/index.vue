<script setup lang="ts">
  import { AxisCrudPage } from '@/shared/components/axis-crud'

  import ProcessCodeField from './components/ProcessCodeField.vue'
  import { useProcessPage } from './composables/useProcessPage'

  defineOptions({ name: 'MdmProcess' })

  /* ============ 状态 & 引用 ============ */

  const { resource, previewNextCode, markCodeManual } = useProcessPage()
</script>

<template>
  <div class="process-page">
    <AxisCrudPage :resource="resource">
      <template #field-code="{ value, setValue, disabled, editing }">
        <ProcessCodeField
          :value="value"
          :disabled="disabled"
          :editing="editing"
          :set-value="setValue"
          :preview="previewNextCode"
          :on-manual-edit="markCodeManual"
        />
      </template>
    </AxisCrudPage>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
