<script setup lang="ts">
  import FileGridPanel from './components/FileGridPanel.vue'
  import FileModulePanel from './components/FileModulePanel.vue'
  import FilePreviewDialog from './components/FilePreviewDialog.vue'
  import { useFilePage } from './composables/useFilePage'

  defineOptions({ name: 'SysFile' })

  /* ============ 状态 & 引用 ============ */

  const {
    permissions,
    modulesLoading,
    listLoading,
    uploading,
    keyword,
    activeModuleKey,
    page,
    pageSize,
    total,
    files,
    moduleOptions,
    currentTitle,
    previewVisible,
    previewFile,
    selectModule,
    handleKeywordUpdate,
    handlePageChange,
    handlePageSizeChange,
    openPreview,
    handleDownload,
    handleDelete,
    customUpload,
    beforeUpload,
    fileKindOf,
    fileUrlOf,
    formatFileSize
  } = useFilePage()
</script>

<template>
  <div class="file-page-root">
    <div class="file-page">
      <FileModulePanel
        :loading="modulesLoading"
        :options="moduleOptions"
        :active-key="activeModuleKey"
        @select="selectModule"
      />

      <FileGridPanel
        :title="currentTitle"
        :loading="listLoading"
        :uploading="uploading"
        :keyword="keyword"
        :files="files"
        :page="page"
        :page-size="pageSize"
        :total="total"
        :can-upload="permissions.upload"
        :can-download="permissions.download"
        :can-delete="permissions.delete"
        :file-kind-of="fileKindOf"
        :file-url-of="fileUrlOf"
        :format-size="formatFileSize"
        :before-upload="beforeUpload"
        :custom-upload="customUpload"
        @update:keyword="handleKeywordUpdate"
        @update:page="handlePageChange"
        @update:page-size="handlePageSizeChange"
        @preview="openPreview"
        @download="handleDownload"
        @delete="handleDelete"
      />
    </div>

    <FilePreviewDialog
      v-model="previewVisible"
      :file="previewFile"
      :can-download="permissions.download"
      @download="handleDownload"
    />
  </div>
</template>

<style lang="scss">
  @use './styles/index.scss';
</style>
