<script setup lang="ts">
  import DictDataPanel from './components/DictDataPanel.vue'
  import DictTypeFormDialog from './components/DictTypeFormDialog.vue'
  import DictTypePanel from './components/DictTypePanel.vue'
  import { useDictPage } from './composables/useDictPage'

  defineOptions({ name: 'SysDict' })

  /* ============ 状态 & 引用 ============ */

  const {
    typeLoading,
    keyword,
    page,
    pageSize,
    total,
    types,
    selectedId,
    selectedType,
    typeFormRef,
    dataPanelRef,
    selectType,
    handleKeywordUpdate,
    handlePageChange,
    handlePageSizeChange,
    openCreateType,
    openEditType,
    confirmDeleteType,
    handleTypeSaved
  } = useDictPage()
</script>

<template>
  <div class="dict-page-root">
    <div class="dict-page">
      <DictTypePanel
        :keyword="keyword"
        :loading="typeLoading"
        :types="types"
        :selected-id="selectedId"
        :page="page"
        :page-size="pageSize"
        :total="total"
        @update:keyword="handleKeywordUpdate"
        @update:page="handlePageChange"
        @update:page-size="handlePageSizeChange"
        @select="selectType"
        @create="openCreateType"
        @edit="openEditType"
        @delete="confirmDeleteType"
      />

      <DictDataPanel
        ref="dataPanelRef"
        :selected-type="selectedType"
        @edit-type="openEditType()"
      />
    </div>

    <DictTypeFormDialog ref="typeFormRef" @saved="handleTypeSaved" />
  </div>
</template>

<style lang="scss">
  @use './styles/index.scss';
</style>
