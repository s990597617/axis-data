<script setup lang="ts">
  import { computed, ref } from 'vue'
  import {
    NAvatar,
    NButton,
    NSpin,
    NUpload,
    type UploadCustomRequestOptions,
    type UploadFileInfo
  } from 'naive-ui'

  import { uploadFile } from '@/modules/system/api/file'
  import { Icon } from '@/shared/icons'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'

  defineOptions({ name: 'UserAvatarField' })

  const props = defineProps<{
    value?: string | null
    disabled?: boolean
    /** 无头像时展示的首字（如姓名/账号） */
    fallbackText?: string
  }>()

  const emit = defineEmits<{
    'update:value': [value: string]
  }>()

  const uploading = ref(false)
  const uploadRootRef = ref<HTMLElement | null>(null)
  const isDragOver = ref(false)

  const previewUrl = computed(() => props.value || '')

  const initials = computed(() => {
    const text = (props.fallbackText || '').trim()
    if (!text) return ''
    return text.slice(0, 1).toUpperCase()
  })

  async function handleUpload({ file, onFinish, onError }: UploadCustomRequestOptions) {
    const raw = file.file
    if (!raw) {
      onError()
      return
    }

    if (!raw.type.startsWith('image/')) {
      message.warning('请上传图片文件')
      onError()
      return
    }

    uploading.value = true
    const [result, error] = await tryRequest(uploadFile(raw, 'avatar'))
    uploading.value = false

    if (error || !result?.url) {
      onError()
      return
    }

    emit('update:value', result.url)
    onFinish()
  }

  function handleRemove() {
    if (props.disabled || uploading.value) return
    emit('update:value', '')
  }

  function beforeUpload(options: { file: UploadFileInfo }) {
    const raw = options.file.file
    if (!raw) return false
    if (!raw.type.startsWith('image/')) {
      message.warning('请上传图片文件')
      return false
    }
    if (raw.size > 5 * 1024 * 1024) {
      message.warning('头像不能超过 5MB')
      return false
    }
    return true
  }

  function openPicker() {
    if (props.disabled || uploading.value) return
    uploadRootRef.value?.querySelector<HTMLInputElement>('input[type="file"]')?.click()
  }
</script>

<template>
  <div
    ref="uploadRootRef"
    class="user-avatar-field"
    :class="{
      'is-disabled': disabled,
      'has-image': !!previewUrl,
      'is-drag-over': isDragOver
    }"
  >
    <div class="user-avatar-field__card">
      <NSpin :show="uploading">
        <NUpload
          :disabled="disabled || uploading"
          accept="image/jpeg,image/png,image/gif,image/webp"
          :show-file-list="false"
          :custom-request="handleUpload"
          :on-before-upload="beforeUpload"
        >
          <div
            class="user-avatar-field__ring"
            role="button"
            tabindex="0"
            @keydown.enter.prevent="openPicker"
            @dragenter.prevent="isDragOver = true"
            @dragover.prevent="isDragOver = true"
            @dragleave.prevent="isDragOver = false"
            @drop.prevent="isDragOver = false"
          >
            <div class="user-avatar-field__avatar">
              <NAvatar
                v-if="previewUrl"
                round
                :size="112"
                :src="previewUrl"
                object-fit="cover"
                class="user-avatar-field__preview"
              />
              <div v-else-if="initials" class="user-avatar-field__initials">
                {{ initials }}
              </div>
              <div v-else class="user-avatar-field__empty">
                <Icon icon="ant-design:cloud-upload-outlined" class="user-avatar-field__empty-icon" />
              </div>

              <div v-if="!disabled" class="user-avatar-field__overlay">
                <span class="user-avatar-field__overlay-icon">
                  <Icon icon="ant-design:camera-outlined" />
                </span>
                <span class="user-avatar-field__overlay-text">
                  {{ previewUrl ? '更换头像' : '上传头像' }}
                </span>
              </div>
            </div>
          </div>
        </NUpload>
      </NSpin>

      <div class="user-avatar-field__meta">
        <div class="user-avatar-field__title">头像</div>
        <p class="user-avatar-field__hint">支持 JPG / PNG / WebP，不超过 5MB</p>
      </div>

      <div v-if="!disabled" class="user-avatar-field__actions">
        <NButton size="tiny" secondary type="primary" :loading="uploading" @click="openPicker">
          <template #icon>
            <Icon icon="ant-design:upload-outlined" />
          </template>
          {{ previewUrl ? '更换' : '上传' }}
        </NButton>
        <NButton
          v-if="previewUrl"
          size="tiny"
          quaternary
          type="error"
          :disabled="uploading"
          @click="handleRemove"
        >
          移除
        </NButton>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
  .user-avatar-field {
    display: flex;
    height: 100%;
    min-height: 100%;
    align-items: stretch;
    justify-content: center;
    padding: 4px 2px 8px;

    &.is-disabled {
      opacity: 0.72;
      pointer-events: none;
    }

    &__card {
      display: flex;
      width: 100%;
      max-width: 176px;
      flex-direction: column;
      align-items: center;
      gap: 10px;
      padding: 14px 10px 12px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 85%, transparent);
      border-radius: var(--axis-border-radius, 6px);
      background: var(--axis-card-color);
    }

    &__ring {
      position: relative;
      border-radius: 50%;
      cursor: pointer;
      outline: none;

      &:hover,
      &:focus-visible {
        .user-avatar-field__avatar {
          border-color: var(--axis-primary-color);
        }

        .user-avatar-field__overlay {
          opacity: 1;
        }
      }
    }

    &.is-drag-over &__avatar {
      border-color: var(--axis-primary-color);
      border-style: solid;
    }

    &__avatar {
      position: relative;
      display: flex;
      width: 112px;
      height: 112px;
      align-items: center;
      justify-content: center;
      overflow: hidden;
      border: 1px dashed color-mix(in srgb, var(--axis-border-color) 90%, var(--axis-text-color-3));
      border-radius: 50%;
      background: color-mix(in srgb, var(--axis-bg-color) 65%, var(--axis-card-color));
      transition: border-color 0.15s ease;
    }

    &.has-image &__avatar {
      border-style: solid;
      border-color: var(--axis-border-color);
      background: var(--axis-card-color);
    }

    &__preview {
      width: 112px !important;
      height: 112px !important;
    }

    &__initials {
      display: flex;
      width: 100%;
      height: 100%;
      align-items: center;
      justify-content: center;
      background: color-mix(in srgb, var(--axis-primary-color) 10%, var(--axis-card-color));
      color: var(--axis-primary-color);
      font-size: 34px;
      font-weight: 600;
      letter-spacing: 0.02em;
    }

    &__empty {
      display: flex;
      width: 100%;
      height: 100%;
      align-items: center;
      justify-content: center;
    }

    &__empty-icon {
      font-size: 30px;
      color: var(--axis-text-color-3);
    }

    &__overlay {
      position: absolute;
      inset: 0;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: 4px;
      background: rgb(0 0 0 / 45%);
      color: #fff;
      opacity: 0;
      transition: opacity 0.15s ease;
      pointer-events: none;
    }

    &__overlay-icon {
      display: inline-flex;
      font-size: 18px;
    }

    &__overlay-text {
      font-size: 12px;
      font-weight: 500;
      line-height: 16px;
    }

    &__meta {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 2px;
      text-align: center;
    }

    &__title {
      color: var(--axis-text-color-1);
      font-size: 13px;
      font-weight: 600;
      line-height: 20px;
    }

    &__hint {
      margin: 0;
      max-width: 132px;
      color: var(--axis-text-color-3);
      font-size: 11px;
      line-height: 16px;
    }

    &__actions {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      justify-content: center;
      gap: 6px;
    }
  }

  :deep(.n-upload) {
    width: auto;
  }

  :deep(.n-spin-container),
  :deep(.n-spin-content) {
    display: flex;
    justify-content: center;
  }
</style>
