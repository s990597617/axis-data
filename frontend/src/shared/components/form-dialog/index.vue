<script setup lang="ts">
  import { computed, provide } from 'vue'
  import type { CSSProperties } from 'vue'
  import { NButton, NDrawer, NDrawerContent, NModal, NSpin, NTag } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import { formDialogLayoutKey } from './context'
  import type { FormDialogExposed, FormDialogProps } from './types'

  defineOptions({ name: 'FormDialog' })

  const props = withDefaults(defineProps<FormDialogProps>(), {
    modelValue: false,
    title: '',
    iconTheme: 'primary',
    statusType: 'default',
    width: 700,
    layout: 'default',
    presentation: 'modal',
    drawerPlacement: 'right',
    loading: false,
    draggable: true,
    maskClosable: false,
    closeOnEsc: true,
    destroyOnClose: false,
    showFooter: true,
    showCancel: true,
    showConfirm: true,
    cancelText: '取消',
    confirmText: '确定',
    confirmLoading: false,
    confirmDisabled: false,
    extraClass: ''
  })

  const emit = defineEmits<{
    'update:modelValue': [value: boolean]
    close: []
    cancel: []
    confirm: []
    afterLeave: []
  }>()

  const slots = defineSlots<{
    default?: () => unknown
    'header-extra'?: () => unknown
    footer?: (props: { close: () => void; cancel: () => void; confirm: () => void }) => unknown
  }>()

  const layout = computed(() => props.layout)
  provide(formDialogLayoutKey, layout)

  const isDrawer = computed(() => props.presentation === 'drawer')

  const dialogStyle = computed<CSSProperties>(() => ({
    width: toCssSize(props.width),
    maxWidth: 'calc(100vw - 32px)'
  }))

  const drawerWidth = computed(() =>
    typeof props.width === 'number' ? `min(${props.width}px, 100vw)` : props.width
  )

  const resolvedMaxBodyHeight = computed(() => {
    if (props.maxBodyHeight !== undefined && props.maxBodyHeight !== null && props.maxBodyHeight !== '') {
      return toCssSize(props.maxBodyHeight)
    }
    return isDrawer.value ? 'none' : 'min(68vh, 720px)'
  })

  const bodyStyle = computed(() => ({
    maxHeight: resolvedMaxBodyHeight.value,
    ...(isDrawer.value ? { flex: '1 1 auto', height: '100%' } : null),
    ...props.bodyStyle
  }))

  const rootClass = computed(() => [
    'form-dialog',
    `form-dialog--${layout.value}`,
    isDrawer.value ? 'form-dialog--drawer' : 'form-dialog--modal',
    props.extraClass
  ])

  function toCssSize(value: string | number) {
    return typeof value === 'number' ? `${value}px` : value
  }

  function updateVisible(value: boolean) {
    emit('update:modelValue', value)
    if (!value) emit('close')
  }

  function open() {
    emit('update:modelValue', true)
  }

  function close() {
    emit('update:modelValue', false)
    emit('close')
  }

  function cancel() {
    emit('cancel')
    close()
  }

  function confirm() {
    emit('confirm')
  }

  defineExpose<FormDialogExposed>({ open, close })
</script>

<template>
  <n-modal
    v-if="!isDrawer"
    :show="modelValue"
    preset="card"
    :style="dialogStyle"
    :class="rootClass"
    :draggable="draggable"
    :mask-closable="maskClosable"
    :close-on-esc="closeOnEsc"
    :display-directive="destroyOnClose ? 'if' : 'show'"
    :segmented="{ content: true, footer: 'soft' }"
    :content-style="{ padding: 0 }"
    :header-style="{ padding: '16px 20px' }"
    :footer-style="{ padding: '12px 20px' }"
    @update:show="updateVisible"
    @after-leave="emit('afterLeave')"
  >
    <template #header>
      <div class="form-dialog__header">
        <div v-if="icon" class="form-dialog__header-icon" :class="`is-${iconTheme}`">
          <Icon v-if="typeof icon === 'string'" :icon="icon" />
          <component :is="icon" v-else />
        </div>
        <div class="form-dialog__titles">
          <div class="form-dialog__title-row">
            <span class="form-dialog__title">{{ title }}</span>
            <n-tag v-if="statusText" size="small" round :type="statusType">
              {{ statusText }}
            </n-tag>
          </div>
          <span v-if="subtitle" class="form-dialog__subtitle">{{ subtitle }}</span>
        </div>
        <slot name="header-extra" />
      </div>
    </template>

    <div class="form-dialog__body" :style="bodyStyle">
      <n-spin :show="loading" class="form-dialog__spin">
        <slot />
      </n-spin>
    </div>

    <template v-if="showFooter" #footer>
      <div class="form-dialog__footer">
        <slot name="footer" :close="close" :cancel="cancel" :confirm="confirm">
          <n-button v-if="showCancel" @click="cancel">{{ cancelText }}</n-button>
          <n-button
            v-if="showConfirm"
            type="primary"
            :loading="confirmLoading"
            :disabled="confirmDisabled"
            @click="confirm"
          >
            {{ confirmText }}
          </n-button>
        </slot>
      </div>
    </template>
  </n-modal>

  <n-drawer
    v-else
    :show="modelValue"
    :width="drawerWidth"
    :placement="drawerPlacement"
    :mask-closable="maskClosable"
    :close-on-esc="closeOnEsc"
    :display-directive="destroyOnClose ? 'if' : 'show'"
    @update:show="updateVisible"
    @after-leave="emit('afterLeave')"
  >
    <n-drawer-content
      closable
      :native-scrollbar="false"
      :class="[...rootClass, 'form-dialog__drawer-content']"
      body-content-class="form-dialog__drawer-body"
      :body-content-style="{ padding: 0 }"
    >
      <template #header>
        <div class="form-dialog__header">
          <div v-if="icon" class="form-dialog__header-icon" :class="`is-${iconTheme}`">
            <Icon v-if="typeof icon === 'string'" :icon="icon" />
            <component :is="icon" v-else />
          </div>
          <div class="form-dialog__titles">
            <div class="form-dialog__title-row">
              <span class="form-dialog__title">{{ title }}</span>
              <n-tag v-if="statusText" size="small" round :type="statusType">
                {{ statusText }}
              </n-tag>
            </div>
            <span v-if="subtitle" class="form-dialog__subtitle">{{ subtitle }}</span>
          </div>
          <slot name="header-extra" />
        </div>
      </template>

      <div class="form-dialog__body" :style="bodyStyle">
        <n-spin :show="loading" class="form-dialog__spin form-dialog__spin--fill">
          <slot />
        </n-spin>
      </div>

      <template v-if="showFooter || slots.footer" #footer>
        <div class="form-dialog__footer">
          <slot name="footer" :close="close" :cancel="cancel" :confirm="confirm">
            <n-button v-if="showCancel" @click="cancel">{{ cancelText }}</n-button>
            <n-button
              v-if="showConfirm"
              type="primary"
              :loading="confirmLoading"
              :disabled="confirmDisabled"
              @click="confirm"
            >
              {{ confirmText }}
            </n-button>
          </slot>
        </div>
      </template>
    </n-drawer-content>
  </n-drawer>
</template>

<style scoped lang="scss">
  .form-dialog {
    &__header {
      display: flex;
      min-width: 0;
      align-items: center;
      gap: 12px;
      padding-right: 28px;
    }

    &__header-icon {
      display: inline-flex;
      width: 40px;
      height: 40px;
      flex: 0 0 40px;
      align-items: center;
      justify-content: center;
      border-radius: var(--axis-border-radius, 8px);
      background: linear-gradient(
        145deg,
        color-mix(in srgb, var(--axis-primary-color) 18%, var(--axis-card-color)),
        var(--axis-primary-color-soft)
      );
      color: var(--axis-primary-color);
      font-size: 20px;
      box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--axis-primary-color) 12%, transparent);

      &.is-success {
        background: rgba(24, 160, 88, 0.1);
        color: #18a058;
      }

      &.is-warning {
        background: rgba(240, 160, 32, 0.1);
        color: #f0a020;
      }

      &.is-error {
        background: rgba(208, 48, 80, 0.1);
        color: #d03050;
      }

      &.is-info {
        background: rgba(32, 128, 240, 0.1);
        color: #2080f0;
      }
    }

    &__titles {
      min-width: 0;
    }

    &__title-row {
      display: flex;
      min-width: 0;
      align-items: center;
      gap: 8px;
    }

    &__title {
      overflow: hidden;
      color: var(--axis-text-color-1);
      font-size: 17px;
      font-weight: 600;
      letter-spacing: 0.01em;
      line-height: 24px;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__subtitle {
      display: block;
      margin-top: 2px;
      overflow: hidden;
      color: var(--axis-text-color-3);
      font-size: 12px;
      font-weight: 400;
      line-height: 18px;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__body {
      min-height: 100px;
      padding: 20px;
      overflow: auto;
    }

    &--sectioned &__body {
      padding: 12px;
      background: linear-gradient(
        180deg,
        color-mix(in srgb, var(--axis-primary-color) 5%, var(--axis-bg-color)) 0%,
        var(--axis-bg-color) 100%
      );
    }

    &__spin {
      min-height: 60px;

      &--fill {
        min-height: 0;
        height: 100%;

        :deep(.n-spin-container),
        :deep(.n-spin-content) {
          min-height: 0;
          height: 100%;
        }
      }
    }

    &__footer {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 10px;
    }
  }

  :deep(.form-dialog__drawer-body) {
    display: flex;
    flex: 1;
    flex-direction: column;
    min-height: 0;
    height: 100%;
    box-sizing: border-box;
    overflow: hidden;
  }

  :deep(.form-dialog__drawer-content) {
    height: 100%;

    .n-drawer-body-content-wrapper {
      display: flex;
      flex: 1;
      flex-direction: column;
      min-height: 0;
      height: 100%;
      overflow: hidden;
    }
  }


  .form-dialog--drawer {
    .form-dialog__body {
      display: flex;
      flex: 1;
      flex-direction: column;
      min-height: 0;
      padding: 16px 20px 20px;
    }

    .form-dialog__header {
      padding-right: 32px;
    }

    &.form-dialog--sectioned .form-dialog__body {
      padding: 16px 20px 24px;
      background: color-mix(in srgb, var(--axis-card-color) 36%, var(--axis-bg-color));
    }
  }

  :deep(.form-dialog__drawer-body) {
    min-height: 100%;
    --n-body-padding: 8px;
    background: color-mix(in srgb, var(--axis-card-color) 36%, var(--axis-bg-color));
  }

  :deep(.form-dialog__drawer-content) {
    height: 100%;

    .n-drawer-body-content-wrapper {
      height: 100%;
      box-sizing: border-box;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }
  }

  @media (max-width: 720px) {
    .form-dialog {
      &__header-icon {
        display: none;
      }

      &__body {
        padding: 16px;
      }

      &--sectioned &__body {
        padding: 12px;
      }
    }
  }
</style>