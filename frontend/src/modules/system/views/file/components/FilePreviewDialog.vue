<script setup lang="ts">
  import { computed, onBeforeUnmount, ref, watch } from 'vue'
  import { NButton, NSpin } from 'naive-ui'

  import { downloadFile, type FileDto } from '@/modules/system/api/file'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { Icon } from '@/shared/icons'
  import { tryRequest } from '@/shared/http'
  import { formatLocalDateTime } from '@/shared/utils/date'

  import {
    formatFileSize,
    getFileKind,
    kindIcon,
    kindTone,
    resolveFileUrl,
    type FileKind
  } from '../utils/file-browser'

  defineOptions({ name: 'FilePreviewDialog' })

  const props = defineProps<{
    modelValue: boolean
    file: FileDto | null
    canDownload: string
  }>()

  const emit = defineEmits<{
    'update:modelValue': [value: boolean]
    download: [file: FileDto]
  }>()

  /* ============ 状态 & 引用 ============ */

  const loading = ref(false)
  const blobUrl = ref('')
  const textPreview = ref('')
  const kind = computed<FileKind>(() => (props.file ? getFileKind(props.file) : 'other'))
  const staticUrl = computed(() => (props.file ? resolveFileUrl(props.file) : ''))
  const needsBlob = computed(() =>
    ['pdf', 'text', 'audio', 'video'].includes(kind.value) && !!props.file
  )

  const visible = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value)
  })

  /* ============ 预览加载 ============ */

  function revokeBlob() {
    if (!blobUrl.value) return
    URL.revokeObjectURL(blobUrl.value)
    blobUrl.value = ''
  }

  async function loadPreview() {
    revokeBlob()
    textPreview.value = ''
    if (!props.file || !needsBlob.value) return

    loading.value = true
    const [blob, error] = await tryRequest(downloadFile(props.file.fileId))
    loading.value = false
    if (error || !blob) return

    if (kind.value === 'text') {
      textPreview.value = await blob.text()
      return
    }

    blobUrl.value = URL.createObjectURL(blob)
  }

  watch(
    () => [props.modelValue, props.file?.fileId] as const,
    ([open]) => {
      if (open) loadPreview()
      else {
        revokeBlob()
        textPreview.value = ''
      }
    }
  )

  onBeforeUnmount(revokeBlob)

  function handleDownload() {
    if (!props.file) return
    emit('download', props.file)
  }
</script>

<template>
  <FormDialog
    v-model="visible"
    :title="file?.originalName || '文件预览'"
    icon="ant-design:eye-outlined"
    icon-theme="primary"
    :width="kind === 'image' || kind === 'pdf' ? 860 : 640"
    max-body-height="min(72vh, 760px)"
    :show-confirm="false"
    cancel-text="关闭"
  >
    <template #header-extra>
      <NButton
        v-if="file"
        v-permission="canDownload"
        size="tiny"
        type="primary"
        ghost
        @click="handleDownload"
      >
        <template #icon>
          <Icon icon="ant-design:download-outlined" />
        </template>
        下载
      </NButton>
    </template>

    <div v-if="file" class="file-preview">
      <div class="file-preview__meta">
        <span>{{ formatFileSize(file.size) }}</span>
        <span>{{ file.contentType || 'unknown' }}</span>
        <span>{{ formatLocalDateTime(file.createdTime) }}</span>
        <span v-if="file.module">{{ file.module }}</span>
      </div>

      <NSpin :show="loading" class="file-preview__body">
        <div v-if="kind === 'image'" class="file-preview__media">
          <img :src="staticUrl" :alt="file.originalName" />
        </div>

        <div v-else-if="kind === 'video'" class="file-preview__media">
          <video v-if="blobUrl" :src="blobUrl" controls playsinline />
        </div>

        <div v-else-if="kind === 'audio'" class="file-preview__audio">
          <audio v-if="blobUrl" :src="blobUrl" controls />
        </div>

        <div v-else-if="kind === 'pdf'" class="file-preview__pdf">
          <iframe v-if="blobUrl" :src="blobUrl" title="pdf-preview" />
        </div>

        <pre v-else-if="kind === 'text'" class="file-preview__text">{{ textPreview || '暂无内容' }}</pre>

        <div v-else class="file-preview__fallback" :class="`is-${kindTone(kind)}`">
          <Icon :icon="kindIcon(kind)" class="file-preview__fallback-icon" />
          <p class="file-preview__fallback-title">该类型暂不支持在线预览</p>
          <p class="file-preview__fallback-desc">可下载后使用本地应用打开</p>
          <NButton
            v-permission="canDownload"
            type="primary"
            size="small"
            @click="handleDownload"
          >
            下载文件
          </NButton>
        </div>
      </NSpin>
    </div>
  </FormDialog>
</template>
