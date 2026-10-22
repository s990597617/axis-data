<script setup lang="ts">
  import { computed } from 'vue'
  import { NButton, NDrawer, NDrawerContent, NSpin, NTag } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import DetailFieldGrid from './detail-field-grid.vue'
  import type { DetailDrawerExposed, DetailDrawerProps, DetailField } from './types'

  defineOptions({ name: 'DetailDrawer' })

  const props = withDefaults(defineProps<DetailDrawerProps>(), {
    show: false,
    title: '详情',
    icon: 'ant-design:file-text-outlined',
    iconTheme: 'primary',
    statusType: 'default',
    width: 760,
    loading: false,
    fields: () => [],
    blocks: () => [],
    sectionTitle: '基本信息',
    columns: 2,
    labelWidth: 96,
    maskClosable: true,
    closeOnEsc: true,
    destroyOnClose: true,
    showFooter: false,
    closeText: '关闭',
    fillContent: false
  })

  const emit = defineEmits<{
    'update:show': [value: boolean]
    close: []
    afterLeave: []
  }>()

  const slots = defineSlots<{
    default?: () => unknown
    'header-extra'?: () => unknown
    field?: (props: { field: DetailField; displayValue: string }) => unknown
    footer?: (props: { close: () => void }) => unknown
  }>()
  const drawerWidth = computed(() =>
    typeof props.width === 'number' ? `min(${props.width}px, 100vw)` : props.width
  )

  function updateShow(value: boolean) {
    emit('update:show', value)
    if (!value) emit('close')
  }

  function open() {
    emit('update:show', true)
  }

  function close() {
    emit('update:show', false)
    emit('close')
  }

  defineExpose<DetailDrawerExposed>({ open, close })
</script>

<template>
  <n-drawer
    :show="show"
    :width="drawerWidth"
    placement="right"
    :mask-closable="maskClosable"
    :close-on-esc="closeOnEsc"
    :display-directive="destroyOnClose ? 'if' : 'show'"
    @update:show="updateShow"
    @after-leave="emit('afterLeave')"
  >
    <n-drawer-content
      closable
      :native-scrollbar="fillContent"
      body-content-class="detail-drawer__body-content"
      :body-content-style="{ padding: '8px' }"
      :class="{ 'detail-drawer--fill': fillContent }"
    >
      <template #header>
        <div class="detail-drawer__header">
          <div v-if="icon" class="detail-drawer__header-icon" :class="`is-${iconTheme}`">
            <Icon :icon="icon" />
          </div>
          <div class="detail-drawer__titles">
            <div class="detail-drawer__title-row">
              <span class="detail-drawer__title">{{ title }}</span>
              <n-tag v-if="statusText" size="small" round :type="statusType">
                {{ statusText }}
              </n-tag>
            </div>
            <div v-if="subtitle" class="detail-drawer__subtitle">{{ subtitle }}</div>
          </div>
          <slot name="header-extra" />
        </div>
      </template>

      <n-spin :show="loading" class="detail-drawer__spin" :class="{ 'is-fill': fillContent }">
        <div class="detail-drawer__content" :class="{ 'is-fill': fillContent }">
          <section v-if="fields.length" class="detail-drawer__section">
            <div v-if="sectionTitle" class="detail-drawer__section-header">
              <span class="detail-drawer__section-title">{{ sectionTitle }}</span>
            </div>
            <DetailFieldGrid :fields="fields" :columns="columns" :label-width="labelWidth">
              <template v-if="slots.field" #field="slotProps">
                <slot name="field" v-bind="slotProps" />
              </template>
            </DetailFieldGrid>
          </section>

          <section
            v-for="(block, index) in blocks"
            :key="`${block.title}-${index}`"
            class="detail-drawer__section"
          >
            <div class="detail-drawer__section-header">
              <span class="detail-drawer__section-title">{{ block.title }}</span>
              <span v-if="block.description" class="detail-drawer__section-description">
                {{ block.description }}
              </span>
            </div>
            <DetailFieldGrid :fields="block.fields" :columns="columns" :label-width="labelWidth">
              <template v-if="slots.field" #field="slotProps">
                <slot name="field" v-bind="slotProps" />
              </template>
            </DetailFieldGrid>
          </section>

          <div v-if="slots.default" class="detail-drawer__extra" :class="{ 'is-fill': fillContent }">
            <slot />
          </div>
        </div>
      </n-spin>

      <template v-if="showFooter || slots.footer" #footer>
        <div class="detail-drawer__footer">
          <slot name="footer" :close="close">
            <n-button @click="close">{{ closeText }}</n-button>
          </slot>
        </div>
      </template>
    </n-drawer-content>
  </n-drawer>
</template>

<style scoped lang="scss">
  .detail-drawer {
    &__header {
      display: flex;
      min-width: 0;
      align-items: center;
      gap: 12px;
      padding-right: 32px;
    }

    &__header-icon {
      display: inline-flex;
      width: 40px;
      height: 40px;
      flex: 0 0 40px;
      align-items: center;
      justify-content: center;
      border-radius: var(--axis-border-radius, 6px);
      background:
        linear-gradient(
          145deg,
          color-mix(in srgb, var(--axis-primary-color) 18%, var(--axis-card-color)),
          var(--axis-primary-color-soft)
        );
      color: var(--axis-primary-color);
      font-size: 20px;
      box-shadow: inset 0 0 0 1px color-mix(in srgb, var(--axis-primary-color) 12%, transparent);

      &.is-success {
        background: linear-gradient(145deg, rgba(24, 160, 88, 0.16), rgba(24, 160, 88, 0.08));
        color: #18a058;
        box-shadow: inset 0 0 0 1px rgba(24, 160, 88, 0.12);
      }

      &.is-warning {
        background: linear-gradient(145deg, rgba(240, 160, 32, 0.16), rgba(240, 160, 32, 0.08));
        color: #f0a020;
        box-shadow: inset 0 0 0 1px rgba(240, 160, 32, 0.12);
      }

      &.is-error {
        background: linear-gradient(145deg, rgba(208, 48, 80, 0.16), rgba(208, 48, 80, 0.08));
        color: #d03050;
        box-shadow: inset 0 0 0 1px rgba(208, 48, 80, 0.12);
      }

      &.is-info {
        background: linear-gradient(145deg, rgba(32, 128, 240, 0.16), rgba(32, 128, 240, 0.08));
        color: #2080f0;
        box-shadow: inset 0 0 0 1px rgba(32, 128, 240, 0.12);
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
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__subtitle {
      margin-top: 3px;
      overflow: hidden;
      color: var(--axis-text-color-3);
      font-size: 12px;
      font-weight: 400;
      line-height: 18px;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__spin {
      min-height: 120px;

      &.is-fill {
        flex: 1;
        min-height: 0;
        height: 100%;
        display: flex;
        flex-direction: column;

        :deep(.n-spin-container),
        :deep(.n-spin-content) {
          flex: 1;
          min-height: 0;
          height: 100%;
          display: flex;
          flex-direction: column;
        }
      }
    }

    &__content {
      display: flex;
      flex-direction: column;
      gap: 10px;

      &.is-fill {
        flex: 1;
        min-height: 0;
        height: 100%;
      }
    }

    &__extra {
      min-width: 0;

      &.is-fill {
        flex: 1;
        min-height: 0;
        display: flex;
        flex-direction: column;
      }
    }

    &__section {
      flex-shrink: 0;
      padding: 14px 16px 12px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 80%, transparent);
      border-radius: var(--axis-border-radius, 6px);
      background: var(--axis-card-color);
    }

    &__section-header {
      display: flex;
      align-items: baseline;
      gap: 10px;
      margin-bottom: 4px;
      padding-bottom: 10px;
      border-bottom: 1px dashed color-mix(in srgb, var(--axis-border-color) 85%, transparent);
    }

    &__section-title {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      color: var(--axis-text-color-1);
      font-size: 14px;
      font-weight: 600;

      &::before {
        width: 3px;
        height: 14px;
        border-radius: 999px;
        background: linear-gradient(
          180deg,
          var(--axis-primary-color),
          color-mix(in srgb, var(--axis-primary-color) 45%, transparent)
        );
        content: '';
      }
    }

    &__section-description {
      color: var(--axis-text-color-3);
      font-size: 12px;
    }

    &__footer {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: 10px;
    }
  }

  :deep(.detail-drawer__body-content) {
    min-height: 100%;
    /* 覆盖 Naive 默认 --n-body-padding，仅作用于详情抽屉 */
    --n-body-padding: 8px;
    background: color-mix(in srgb, var(--axis-card-color) 36%, var(--axis-bg-color));
  }

  :deep(.detail-drawer--fill) {
    height: 100%;

    .n-drawer-body-content-wrapper,
    .detail-drawer__body-content {
      height: 100%;
      box-sizing: border-box;
      overflow: hidden;
      display: flex;
      flex-direction: column;
    }
  }

  @media (max-width: 720px) {
    .detail-drawer {
      &__section {
        padding: 14px;
      }

      &__header-icon {
        display: none;
      }
    }
  }
</style>
