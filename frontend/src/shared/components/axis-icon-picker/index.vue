<script setup lang="ts">
  import { computed } from 'vue'

  import { Icon } from '@/shared/icons'
  import { useIconPickerState } from './use-icon-picker.ts'

  interface Props {
    value?: string | null
    placeholder?: string
    disabled?: boolean
    clearable?: boolean
    width?: number | string
    popoverWidth?: number
    pageSize?: number
  }

  interface Emits {
    (event: 'update:value', value: string | null): void
    (event: 'change', value: string | null): void
  }

  const props = withDefaults(defineProps<Props>(), {
    value: '',
    placeholder: '点击选择图标',
    disabled: false,
    clearable: true,
    width: '100%',
    popoverWidth: 420,
    pageSize: 96
  })

  const emit = defineEmits<Emits>()

  const triggerStyle = computed(() => ({
    width: typeof props.width === 'number' ? `${props.width}px` : props.width
  }))

  const {
    clear,
    filteredIcons,
    hoverName,
    nextPage,
    page,
    pagedIcons,
    prefix,
    prefixTabs,
    prevPage,
    search,
    select,
    toggleOpen,
    totalPages,
    visible
  } = useIconPickerState(props, emit)
</script>

<template>
  <n-popover
    v-model:show="visible"
    trigger="manual"
    placement="bottom-start"
    :raw="true"
    :disabled="disabled"
    :show-arrow="false"
    :to="true"
    @clickoutside="visible = false"
  >
    <template #trigger>
      <div
        class="icon-picker__trigger"
        :class="{
          'is-active': visible,
          'is-disabled': disabled,
          'is-empty': !value
        }"
        :style="triggerStyle"
        @click="toggleOpen"
      >
        <div class="icon-picker__preview">
          <Icon v-if="value" :icon="value" />
          <Icon v-else icon="ant-design:appstore-outlined" class="icon-picker__placeholder-icon" />
        </div>
        <span class="icon-picker__label">{{ value || placeholder }}</span>
        <span
          v-if="clearable && value && !disabled"
          class="icon-picker__clear"
          title="清除"
          @click="clear"
        >
          <Icon icon="ant-design:close-circle-filled" />
        </span>
        <span class="icon-picker__chevron">
          <Icon icon="ant-design:down-outlined" />
        </span>
      </div>
    </template>

    <div class="icon-picker__panel" :style="{ width: `${popoverWidth}px` }">
      <div class="icon-picker__panel-header">
        <n-input v-model:value="search" placeholder="搜索图标名..." clearable size="small">
          <template #prefix>
            <Icon icon="ant-design:search-outlined" />
          </template>
        </n-input>
      </div>

      <div class="icon-picker__tabs">
        <button
          v-for="tab in prefixTabs"
          :key="tab.value"
          type="button"
          class="icon-picker__tab"
          :class="{ 'is-active': prefix === tab.value }"
          @click="prefix = tab.value"
        >
          {{ tab.label }}
          <span class="icon-picker__tab-count">{{ tab.count }}</span>
        </button>
      </div>

      <div class="icon-picker__grid-wrap">
        <div v-if="pagedIcons.length" class="icon-picker__grid">
          <button
            v-for="name in pagedIcons"
            :key="name"
            type="button"
            class="icon-picker__cell"
            :class="{ 'is-selected': name === value }"
            :title="name"
            @click="select(name)"
            @mouseenter="hoverName = name"
            @mouseleave="hoverName = value || null"
          >
            <Icon :icon="name" />
          </button>
        </div>

        <div v-else class="icon-picker__empty">
          <n-empty size="small" description="没有匹配的图标" />
        </div>
      </div>

      <div class="icon-picker__footer">
        <span class="icon-picker__hover-name">
          {{ hoverName || `共 ${filteredIcons.length} 个图标` }}
        </span>

        <div class="icon-picker__pager">
          <button
            type="button"
            class="icon-picker__pager-btn"
            :disabled="page <= 1"
            @click="prevPage"
          >
            <Icon icon="ant-design:left-outlined" />
          </button>
          <span class="icon-picker__pager-info">{{ page }} / {{ totalPages }}</span>
          <button
            type="button"
            class="icon-picker__pager-btn"
            :disabled="page >= totalPages"
            @click="nextPage"
          >
            <Icon icon="ant-design:right-outlined" />
          </button>
        </div>
      </div>
    </div>
  </n-popover>
</template>

<style scoped>
  .icon-picker__trigger {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 0 8px 0 10px;
    height: 34px;
    border: 1px solid var(--n-border-color, #e0e0e6);
    border-radius: 6px;
    background: var(--n-card-color, #fff);
    cursor: pointer;
    transition:
      border-color 0.2s ease,
      box-shadow 0.2s ease;
    user-select: none;
    font-size: 14px;
    color: var(--n-text-color-1, rgba(0, 0, 0, 0.85));
    box-sizing: border-box;
  }

  .icon-picker__trigger:hover:not(.is-disabled),
  .icon-picker__trigger.is-active {
    border-color: var(--axis-primary-color);
  }

  .icon-picker__trigger.is-active {
    box-shadow: 0 0 0 2px color-mix(in srgb, var(--axis-primary-color) 20%, transparent);
  }

  .icon-picker__trigger.is-disabled {
    cursor: not-allowed;
    opacity: 0.6;
    background: var(--n-action-color, #f5f5f5);
  }

  .icon-picker__preview {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    width: 22px;
    height: 22px;
    font-size: 18px;
    color: inherit;
  }

  .icon-picker__placeholder-icon,
  .icon-picker__clear,
  .icon-picker__chevron {
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.4));
  }

  .icon-picker__label {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-family: ui-monospace, 'SF Mono', Menlo, Consolas, monospace;
    font-size: 13px;
  }

  .icon-picker__trigger.is-empty .icon-picker__label {
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.4));
    font-family: inherit;
  }

  .icon-picker__clear,
  .icon-picker__chevron {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
    font-size: 14px;
    transition: color 0.2s ease;
  }

  .icon-picker__clear {
    cursor: pointer;
  }

  .icon-picker__clear:hover {
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.6));
  }

  .icon-picker__chevron {
    font-size: 10px;
    transition: transform 0.2s ease;
  }

  .icon-picker__trigger.is-active .icon-picker__chevron {
    transform: rotate(180deg);
  }

  .icon-picker__panel {
    background: var(--n-card-color, #fff);
    border: 1px solid var(--n-border-color, #e0e0e6);
    border-radius: 8px;
    box-shadow: 0 6px 24px rgba(0, 0, 0, 0.08);
    overflow: hidden;
    display: flex;
    flex-direction: column;
    max-height: 420px;
  }

  .icon-picker__panel-header,
  .icon-picker__grid-wrap,
  .icon-picker__footer {
    padding: 10px 12px;
  }

  .icon-picker__panel-header,
  .icon-picker__tabs {
    border-bottom: 1px solid var(--n-border-color, #efeff5);
  }

  .icon-picker__tabs {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    padding: 10px 12px;
    background: var(--n-action-color, #fafafc);
  }

  .icon-picker__tab {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    padding: 3px 10px;
    border-radius: 4px;
    border: 1px solid transparent;
    background: var(--n-card-color, #fff);
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.65));
    font-size: 12px;
    cursor: pointer;
    transition: all 0.15s ease;
    font-family: inherit;
  }

  .icon-picker__tab:hover {
    color: var(--axis-primary-color);
  }

  .icon-picker__tab.is-active {
    background: var(--axis-primary-color);
    color: #fff;
  }

  .icon-picker__tab-count {
    font-size: 10px;
    opacity: 0.75;
  }

  .icon-picker__grid-wrap {
    flex: 1;
    overflow-y: auto;
  }

  .icon-picker__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(36px, 1fr));
    gap: 4px;
  }

  .icon-picker__cell {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    aspect-ratio: 1 / 1;
    border: 1px solid transparent;
    border-radius: 6px;
    background: transparent;
    color: var(--n-text-color-1, rgba(0, 0, 0, 0.85));
    font-size: 18px;
    cursor: pointer;
    transition: all 0.15s ease;
    padding: 0;
  }

  .icon-picker__cell:hover {
    background: color-mix(in srgb, var(--axis-primary-color) 8%, transparent);
    color: var(--axis-primary-color);
  }

  .icon-picker__cell.is-selected {
    background: var(--axis-primary-color);
    color: #fff;
    border-color: var(--axis-primary-color);
  }

  .icon-picker__empty {
    display: flex;
    align-items: center;
    justify-content: center;
    height: 180px;
  }

  .icon-picker__footer {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 10px;
    border-top: 1px solid var(--n-border-color, #efeff5);
    background: var(--n-action-color, #fafafc);
    font-size: 12px;
  }

  .icon-picker__hover-name {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.6));
    font-family: ui-monospace, 'SF Mono', Menlo, Consolas, monospace;
  }

  .icon-picker__pager {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    flex-shrink: 0;
  }

  .icon-picker__pager-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 22px;
    height: 22px;
    border: 1px solid var(--n-border-color, #e0e0e6);
    border-radius: 4px;
    background: var(--n-card-color, #fff);
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.65));
    font-size: 12px;
    cursor: pointer;
    transition: all 0.15s ease;
  }

  .icon-picker__pager-btn:hover:not(:disabled) {
    border-color: var(--axis-primary-color);
    color: var(--axis-primary-color);
  }

  .icon-picker__pager-btn:disabled {
    cursor: not-allowed;
    opacity: 0.4;
  }

  .icon-picker__pager-info {
    min-width: 48px;
    text-align: center;
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.6));
    font-variant-numeric: tabular-nums;
  }
</style>
