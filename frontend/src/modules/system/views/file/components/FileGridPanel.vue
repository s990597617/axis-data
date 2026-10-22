<script setup lang="ts">
  import { ref } from 'vue'
  import {
    NButton,
    NEmpty,
    NInput,
    NPagination,
    NScrollbar,
    NSpin,
    type UploadCustomRequestOptions,
    type UploadFileInfo
  } from 'naive-ui'

  import type { FileDto } from '@/modules/system/api/file'
  import { Icon } from '@/shared/icons'
  import { formatLocalDateTime } from '@/shared/utils/date'

  import type { FileKind } from '../utils/file-browser'
  import { kindIcon, kindTone } from '../utils/file-browser'

  defineOptions({ name: 'FileGridPanel' })

  const props = defineProps<{
    title: string
    loading: boolean
    uploading: boolean
    keyword: string
    files: FileDto[]
    page: number
    pageSize: number
    total: number
    canUpload: string
    canDownload: string
    canDelete: string
    fileKindOf: (file: FileDto) => FileKind
    fileUrlOf: (file: FileDto) => string
    formatSize: (size: number) => string
    beforeUpload: (options: { file: UploadFileInfo }) => boolean
    customUpload: (options: UploadCustomRequestOptions) => void
  }>()

  const emit = defineEmits<{
    'update:keyword': [value: string]
    'update:page': [value: number]
    'update:pageSize': [value: number]
    preview: [file: FileDto]
    download: [file: FileDto]
    delete: [file: FileDto]
  }>()

  /* ============ 状态 & 引用 ============ */

  const fileInputRef = ref<HTMLInputElement | null>(null)

  /* ============ 交互操作 ============ */

  function onThumbError(event: Event) {
    const el = event.target as HTMLImageElement
    el.style.display = 'none'
  }

  function openFilePicker() {
    fileInputRef.value?.click()
  }

  function handleFileChange(event: Event) {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0]
    input.value = ''
    if (!file) return

    const fileInfo = {
      id: `${Date.now()}`,
      name: file.name,
      status: 'pending',
      file
    } as UploadFileInfo

    if (props.beforeUpload({ file: fileInfo }) === false) return

    props.customUpload({
      file: fileInfo,
      onFinish: () => undefined,
      onError: () => undefined,
      onProgress: () => undefined
    } as UploadCustomRequestOptions)
  }
</script>

<template>
  <section class="file-grid-panel">
    <header class="file-grid-panel__header">
      <div class="file-grid-panel__heading">
        <h2 class="file-grid-panel__title">{{ title }}</h2>
        <span class="file-grid-panel__meta">共 {{ total }} 个文件</span>
      </div>

      <div class="file-grid-panel__actions">
        <NInput
          :value="keyword"
          clearable
          size="small"
          placeholder="搜索文件名"
          class="file-grid-panel__search"
          @update:value="value => emit('update:keyword', value)"
        >
          <template #prefix>
            <Icon icon="ant-design:search-outlined" />
          </template>
        </NInput>

        <input
          ref="fileInputRef"
          type="file"
          hidden
          @change="handleFileChange"
        />
        <NButton
          v-permission="canUpload"
          type="primary"
          size="small"
          :loading="uploading"
          @click="openFilePicker"
        >
          <template #icon>
            <Icon icon="ant-design:cloud-upload-outlined" />
          </template>
          上传文件
        </NButton>
      </div>
    </header>

    <div class="file-grid-panel__body">
      <NSpin :show="loading" class="file-grid-panel__spin">
        <NScrollbar v-if="files.length" class="file-grid-panel__scroll">
          <div class="file-grid">
            <article
              v-for="file in files"
              :key="file.fileId"
              class="file-card"
              @click="emit('preview', file)"
            >
              <div class="file-card__thumb" :class="`is-${kindTone(fileKindOf(file))}`">
                <img
                  v-if="fileKindOf(file) === 'image' && fileUrlOf(file)"
                  class="file-card__image"
                  :src="fileUrlOf(file)"
                  :alt="file.originalName"
                  loading="lazy"
                  @error="onThumbError"
                />
                <div v-else class="file-card__glyph">
                  <Icon :icon="kindIcon(fileKindOf(file))" />
                </div>

                <div class="file-card__actions" @click.stop>
                  <button
                    v-permission="canDownload"
                    type="button"
                    class="file-card__action"
                    title="下载"
                    @click="emit('download', file)"
                  >
                    <Icon icon="ant-design:download-outlined" />
                  </button>
                  <button
                    v-permission="canDelete"
                    type="button"
                    class="file-card__action is-danger"
                    title="删除"
                    @click="emit('delete', file)"
                  >
                    <Icon icon="ant-design:delete-outlined" />
                  </button>
                </div>
              </div>

              <div class="file-card__body">
                <button
                  type="button"
                  class="file-card__name"
                  :title="file.originalName"
                  @click.stop="emit('preview', file)"
                >
                  {{ file.originalName }}
                </button>
                <div class="file-card__footer">
                  <span>{{ formatSize(file.size) }}</span>
                  <span>{{ formatLocalDateTime(file.createdTime) }}</span>
                </div>
              </div>
            </article>
          </div>
        </NScrollbar>

        <div v-else class="file-grid-panel__empty">
          <NEmpty description="暂无文件，可上传或切换左侧分类" />
        </div>
      </NSpin>
    </div>

    <footer v-if="total > 0" class="file-grid-panel__pager">
      <NPagination
        :page="page"
        :page-size="pageSize"
        :item-count="total"
        :page-sizes="[24, 48, 72]"
        show-size-picker
        size="small"
        :prefix="info => `共 ${info.itemCount ?? 0} 条`"
        @update:page="value => emit('update:page', value)"
        @update:page-size="value => emit('update:pageSize', value)"
      />
    </footer>
  </section>
</template>
