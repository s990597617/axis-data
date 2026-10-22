<script setup lang="ts">
  import { NSpin, NTree } from 'naive-ui'

  import { AxisCrudPage } from '@/shared/components/axis-crud'
  import { Icon } from '@/shared/icons'

  import MaterialCodeField from './components/MaterialCodeField.vue'
  import { useMaterialPage } from './composables/useMaterialPage'

  defineOptions({ name: 'MdmMaterial' })

  /* ============ 状态 & 引用 ============ */

  const {
    resource,
    previewNextCode,
    markCodeManual,
    categoryLoading,
    categoryTreeOptions,
    selectedCategoryId,
    handleCategorySelect,
    crudRef
  } = useMaterialPage()
</script>

<template>
  <div class="material-page">
    <div class="material-page__tree-panel">
      <div class="material-page__tree-header">
        <Icon icon="ant-design:apartment-outlined" class="material-page__tree-icon" />
        <span>物料分类</span>
      </div>
      <NSpin :show="categoryLoading" class="material-page__tree-spin">
        <n-scrollbar class="material-page__tree-scroll">
          <NTree
            :data="categoryTreeOptions"
            :selected-keys="selectedCategoryId ? [selectedCategoryId] : []"
            key-field="key"
            label-field="label"
            children-field="children"
            selectable
            :cancelable="false"
            expand-on-click
            default-expand-all
            block-line
            show-line
            @update:selected-keys="handleCategorySelect"
          />
        </n-scrollbar>
      </NSpin>
    </div>

    <div class="material-page__content">
      <AxisCrudPage ref="crudRef" :resource="resource">
        <template #field-code="{ value, setValue, disabled, editing }">
          <MaterialCodeField
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
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
