<script setup lang="ts">
  import { computed, ref, watch } from 'vue'
  import {
    NProgress,
    NScrollbar,
    NUpload,
    NUploadDragger,
    type UploadFileInfo
  } from 'naive-ui'

  import { FormDialog } from '@/shared/components/form-dialog'
  import { Icon } from '@/shared/icons'
  import { message } from '@/shared/utils/message'

  import type { ExcelImportDialogProps, ExcelImportResult } from './types'

  defineOptions({ name: 'ExcelImportDialog' })

  const props = withDefaults(
    defineProps<
      ExcelImportDialogProps & {
        downloadTemplate: () => Promise<void>
        importFile: (file: File) => Promise<ExcelImportResult>
      }
    >(),
    {
      modelValue: false,
      title: '导入数据',
      tip: '请先下载 Excel 模板，按列填写后再上传。仅支持 .xlsx 文件。',
      accept: '.xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      templateButtonText: '下载模板',
      confirmText: '开始导入',
      maxSizeMb: 10
    }
  )

  const emit = defineEmits<{
    'update:modelValue': [value: boolean]
    success: [result: ExcelImportResult]
  }>()

  /* ============ 状态 & 引用 ============ */

  const selectedFile = ref<File | null>(null)
  const templateLoading = ref(false)
  const importing = ref(false)
  const progress = ref(0)
  const progressLabel = ref('')
  const result = ref<ExcelImportResult | null>(null)
  let progressTimer: ReturnType<typeof setInterval> | null = null

  const visible = computed({
    get: () => props.modelValue,
    set: value => emit('update:modelValue', value)
  })

  const hasResult = computed(() => !!result.value)
  const showUploader = computed(() => !selectedFile.value && !hasResult.value)
  const confirmText = computed(() => {
    if (importing.value) return '导入中…'
    if (hasResult.value) return '重新选择'
    return props.confirmText
  })

  const confirmDisabled = computed(() => {
    if (importing.value) return true
    if (hasResult.value) return false
    return !selectedFile.value
  })

  /* ============ 交互操作 ============ */

  watch(
    () => props.modelValue,
    value => {
      if (!value) resetState()
    }
  )

  function resetState() {
    stopProgress()
    selectedFile.value = null
    templateLoading.value = false
    importing.value = false
    progress.value = 0
    progressLabel.value = ''
    result.value = null
  }

  function formatSize(size: number) {
    if (size < 1024) return `${size} B`
    if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
    return `${(size / 1024 / 1024).toFixed(2)} MB`
  }

  function beforeUpload(options: { file: UploadFileInfo }) {
    const raw = options.file.file
    if (!raw) {
      message.warning('请选择文件')
      return false
    }

    const maxBytes = props.maxSizeMb * 1024 * 1024
    if (raw.size > maxBytes) {
      message.warning(`文件不能超过 ${props.maxSizeMb}MB`)
      return false
    }

    if (!raw.name.toLowerCase().endsWith('.xlsx')) {
      message.warning('仅支持 .xlsx 格式')
      return false
    }

    selectedFile.value = raw
    result.value = null
    progress.value = 0
    progressLabel.value = ''
    return false
  }

  function clearFile() {
    if (importing.value) return
    selectedFile.value = null
    result.value = null
    progress.value = 0
    progressLabel.value = ''
  }

  async function handleDownloadTemplate() {
    templateLoading.value = true
    try {
      await props.downloadTemplate()
      message.success('模板已开始下载')
    } finally {
      templateLoading.value = false
    }
  }

  function startProgress() {
    stopProgress()
    progress.value = 8
    progressLabel.value = '正在上传并校验…'
    progressTimer = setInterval(() => {
      if (progress.value >= 90) return
      progress.value += Math.max(1, Math.round((90 - progress.value) * 0.08))
      if (progress.value > 55) progressLabel.value = '正在写入数据…'
    }, 180)
  }

  function stopProgress() {
    if (!progressTimer) return
    clearInterval(progressTimer)
    progressTimer = null
  }

  async function handleConfirm() {
    if (hasResult.value && !importing.value) {
      clearFile()
      return
    }

    if (!selectedFile.value) return

    importing.value = true
    startProgress()
    try {
      const data = await props.importFile(selectedFile.value)
      stopProgress()
      progress.value = 100
      progressLabel.value =
        data.failureCount > 0 ? '导入完成，部分行失败' : '导入完成'
      result.value = data
      if (data.failureCount === 0) {
        message.success(`成功导入 ${data.successCount} 行`)
      }
      emit('success', data)
    } catch {
      stopProgress()
      progress.value = 0
      progressLabel.value = ''
    } finally {
      importing.value = false
    }
  }
</script>

<template>
  <FormDialog
    v-model="visible"
    :title="title"
    icon="ant-design:file-excel-outlined"
    icon-theme="primary"
    :width="520"
    max-body-height="min(62vh, 560px)"
    :mask-closable="!importing"
    :close-on-esc="!importing"
    :confirm-loading="importing"
    :confirm-disabled="confirmDisabled"
    :confirm-text="confirmText"
    cancel-text="关闭"
    @confirm="handleConfirm"
  >
    <div class="excel-import-dialog">
      <div class="excel-import-dialog__toolbar">
        <div class="excel-import-dialog__toolbar-text">
          <span class="excel-import-dialog__toolbar-title">准备导入</span>
          <span class="excel-import-dialog__toolbar-tip">{{ tip }}</span>
        </div>
        <button
          type="button"
          class="excel-import-dialog__template-btn"
          :disabled="importing || templateLoading"
          @click="handleDownloadTemplate"
        >
          <Icon
            :icon="
              templateLoading
                ? 'ant-design:loading-outlined'
                : 'ant-design:download-outlined'
            "
            :class="{ 'is-spin': templateLoading }"
          />
          <span>{{ templateButtonText }}</span>
        </button>
      </div>

      <NUpload
        v-if="showUploader"
        class="excel-import-dialog__upload"
        :accept="accept"
        :show-file-list="false"
        :disabled="importing"
        :on-before-upload="beforeUpload"
      >
        <NUploadDragger>
          <div class="excel-import-dialog__drop">
            <div class="excel-import-dialog__drop-badge">
              <Icon icon="ant-design:cloud-upload-outlined" />
            </div>
            <p class="excel-import-dialog__drop-title">将 Excel 拖到此处，或点击上传</p>
            <p class="excel-import-dialog__drop-desc">
              支持 .xlsx · 不超过 {{ maxSizeMb }}MB
            </p>
          </div>
        </NUploadDragger>
      </NUpload>

      <div v-if="selectedFile && !hasResult" class="excel-import-dialog__file">
        <div class="excel-import-dialog__file-badge">XLS</div>
        <div class="excel-import-dialog__file-meta">
          <div class="excel-import-dialog__file-name" :title="selectedFile.name">
            {{ selectedFile.name }}
          </div>
          <div class="excel-import-dialog__file-size">{{ formatSize(selectedFile.size) }}</div>
        </div>
        <button
          type="button"
          class="excel-import-dialog__file-remove"
          :disabled="importing"
          @click="clearFile"
        >
          移除
        </button>
      </div>

      <div v-if="importing" class="excel-import-dialog__progress">
        <div class="excel-import-dialog__progress-label">
          <span>{{ progressLabel || '处理中' }}</span>
          <span>{{ progress }}%</span>
        </div>
        <NProgress
          type="line"
          :percentage="progress"
          :height="8"
          :border-radius="999"
          :fill-border-radius="999"
          :show-indicator="false"
          :processing="true"
          color="var(--axis-primary-color)"
          rail-color="var(--axis-primary-color-soft)"
        />
      </div>

      <div v-if="result" class="excel-import-dialog__result">
        <div
          class="excel-import-dialog__result-banner"
          :class="result.failureCount > 0 ? 'is-partial' : 'is-ok'"
        >
          <Icon
            :icon="
              result.failureCount > 0
                ? 'ant-design:exclamation-circle-filled'
                : 'ant-design:check-circle-filled'
            "
            class="excel-import-dialog__result-icon"
          />
          <div class="excel-import-dialog__result-copy">
            <p class="excel-import-dialog__result-title">
              {{ result.failureCount > 0 ? '导入完成，部分失败' : '全部导入成功' }}
            </p>
            <p class="excel-import-dialog__result-stats">
              成功
              <em>{{ result.successCount }}</em>
              行 · 失败
              <em>{{ result.failureCount }}</em>
              行
            </p>
          </div>
        </div>

        <div v-if="result.failures.length" class="excel-import-dialog__failures">
          <div class="excel-import-dialog__failures-head">
            <span>失败明细</span>
            <span class="excel-import-dialog__failures-count">
              {{ result.failures.length }} 条
            </span>
          </div>
          <NScrollbar class="excel-import-dialog__failures-scroll" style="max-height: 168px">
            <ul class="excel-import-dialog__failure-list">
              <li
                v-for="item in result.failures"
                :key="`${item.row}-${item.message}`"
                class="excel-import-dialog__failure-item"
              >
                <span class="excel-import-dialog__failure-row">{{ item.row }}</span>
                <span class="excel-import-dialog__failure-msg">{{ item.message }}</span>
              </li>
            </ul>
          </NScrollbar>
        </div>
      </div>
    </div>
  </FormDialog>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
