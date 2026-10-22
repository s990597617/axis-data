<script setup lang="ts">
  import MenuDetailPanel from './components/MenuDetailPanel.vue'
  import MenuFormDialog from './components/MenuFormDialog.vue'
  import MenuTreePanel from './components/MenuTreePanel.vue'
  import { useMenuPage } from './composables/useMenuPage'

  defineOptions({ name: 'SysMenu' })

  /* ============ 状态 & 引用 ============ */

  const {
    formDialogRef,
    loading,
    keyword,
    rows,
    selectedId,
    expandedKeys,
    canCreate,
    canDelete,
    navCount,
    menuTree,
    filteredMenuTree,
    selectedMenu,
    selectedButtons,
    openCreateRoot,
    openCreateChild,
    openCreateButton,
    openEdit,
    confirmDelete,
    handleDelete,
    handleSaved
  } = useMenuPage()
</script>

<template>
  <div class="menu-page-root">
    <div class="menu-page">
      <MenuTreePanel
        v-model:keyword="keyword"
        v-model:selected-id="selectedId"
        v-model:expanded-keys="expandedKeys"
        :loading="loading"
        :nav-count="navCount"
        :tree-data="filteredMenuTree"
        :can-create="canCreate"
        :can-delete="canDelete"
        @create-root="openCreateRoot"
        @create-child="openCreateChild"
        @delete="confirmDelete"
      />

      <MenuDetailPanel
        :loading="loading"
        :selected-menu="selectedMenu"
        :selected-buttons="selectedButtons"
        @edit="openEdit"
        @create-button="openCreateButton"
        @delete="handleDelete"
      />
    </div>

    <MenuFormDialog ref="formDialogRef" :rows="rows" :menu-tree="menuTree" @saved="handleSaved" />
  </div>
</template>

<style lang="scss">
  @use './styles/index.scss';
</style>
