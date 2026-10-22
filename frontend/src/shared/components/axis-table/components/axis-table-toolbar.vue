<script setup lang="ts">
  import type { DataTableRowKey } from 'naive-ui/es/data-table'

  import { Icon } from '@/shared/icons'

  import type { AxisTableColumnSettingOption } from '../hooks/use-axis-table-columns'
  import type { AxisTableSize } from '../types'

  defineOptions({ name: 'AxisTableToolbar' })

  defineProps<{
    title?: string
    titleTooltip?: string
    showRefresh: boolean
    showDensity: boolean
    showColumnSetting: boolean
    size: AxisTableSize
    columnOptions: AxisTableColumnSettingOption[]
    checkedColumnKeys: DataTableRowKey[]
  }>()

  const emit = defineEmits<{
    refresh: []
    'update:size': [size: AxisTableSize]
    'update:checkedColumnKeys': [keys: DataTableRowKey[]]
    resetColumns: []
  }>()

  const densityOptions = [
    { label: '紧凑', key: 'small' },
    { label: '默认', key: 'medium' },
    { label: '宽松', key: 'large' }
  ]

  function handleDensitySelect(key: string | number) {
    if (key === 'small' || key === 'medium' || key === 'large') {
      emit('update:size', key)
    }
  }

  function handleColumnChange(keys: DataTableRowKey[]) {
    emit('update:checkedColumnKeys', keys)
  }
</script>

<template>
  <div class="axis-table-toolbar">
    <div class="axis-table-toolbar__left">
      <slot name="title">
        <div v-if="title" class="axis-table-toolbar__title">
          <span>{{ title }}</span>
          <n-tooltip v-if="titleTooltip" trigger="hover">
            <template #trigger>
              <Icon
                icon="ant-design:question-circle-outlined"
                class="axis-table-toolbar__title-help"
              />
            </template>
            {{ titleTooltip }}
          </n-tooltip>
        </div>
      </slot>

      <div v-if="$slots.toolbar" class="axis-table-toolbar__actions">
        <slot name="toolbar" />
      </div>
    </div>

    <div class="axis-table-toolbar__right">
      <slot name="extra" />

      <n-tooltip v-if="showRefresh" trigger="hover">
        <template #trigger>
          <button type="button" class="axis-table-toolbar__icon-btn" @click="emit('refresh')">
            <Icon icon="ant-design:reload-outlined" />
          </button>
        </template>
        刷新
      </n-tooltip>

      <n-dropdown
        v-if="showDensity"
        trigger="click"
        :options="densityOptions"
        :value="size"
        @select="handleDensitySelect"
      >
        <button type="button" class="axis-table-toolbar__icon-btn">
          <Icon icon="ant-design:column-height-outlined" />
        </button>
      </n-dropdown>

      <n-popover v-if="showColumnSetting" trigger="click" placement="bottom-end">
        <template #trigger>
          <button type="button" class="axis-table-toolbar__icon-btn">
            <Icon icon="ant-design:setting-outlined" />
          </button>
        </template>

        <div class="axis-table-toolbar__column-setting">
          <div class="axis-table-toolbar__column-setting-title">列设置</div>
          <n-checkbox-group :value="checkedColumnKeys" @update:value="handleColumnChange">
            <div class="axis-table-toolbar__column-list">
              <n-checkbox
                v-for="column in columnOptions"
                :key="String(column.key)"
                :value="column.key"
              >
                {{ column.title }}
              </n-checkbox>
            </div>
          </n-checkbox-group>
          <button
            type="button"
            class="axis-table-toolbar__column-reset"
            @click="emit('resetColumns')"
          >
            恢复默认
          </button>
        </div>
      </n-popover>
    </div>
  </div>
</template>

<style scoped lang="scss">
  .axis-table-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    padding: 0 0 10px;

    &__left,
    &__right {
      display: flex;
      align-items: center;
      gap: 12px;
      min-width: 0;
    }

    &__actions {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;

      /* 工具栏内上传控件默认占满整行，会导致按钮竖排 */
      :deep(.n-upload) {
        width: auto;
      }

      :deep(.n-upload-trigger) {
        display: inline-flex;
      }
    }

    &__right {
      gap: 6px;
      justify-content: flex-end;
    }

    &__title {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      min-width: 0;
      color: var(--axis-text-color-1);
      font-size: 14px;
      font-weight: 600;
    }

    &__title-help {
      color: var(--axis-text-color-3);
      font-size: 15px;
    }

    &__icon-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 30px;
      height: 30px;
      border: none;
      border-radius: 6px;
      background: transparent;
      color: var(--axis-text-color-2);
      font-size: 16px;
      cursor: pointer;
      transition:
        background-color 0.15s ease,
        color 0.15s ease;

      &:hover {
        background: var(--axis-action-color);
        color: var(--axis-primary-color);
      }
    }

    &__column-setting {
      width: 180px;
    }

    &__column-setting-title {
      margin-bottom: 8px;
      color: var(--axis-text-color-1);
      font-size: 13px;
      font-weight: 600;
    }

    &__column-list {
      display: flex;
      max-height: 260px;
      flex-direction: column;
      gap: 8px;
      overflow: auto;
    }

    &__column-reset {
      width: 100%;
      height: 30px;
      margin-top: 10px;
      border: 1px solid var(--axis-border-color);
      border-radius: 6px;
      background: var(--axis-card-color);
      color: var(--axis-text-color-2);
      font-size: 13px;
      cursor: pointer;
      transition:
        border-color 0.15s ease,
        color 0.15s ease;

      &:hover {
        border-color: var(--axis-primary-color);
        color: var(--axis-primary-color);
      }
    }
  }
</style>
