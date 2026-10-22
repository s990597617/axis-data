<script setup lang="ts">
  import { useResizeObserver } from '@vueuse/core'
  import { computed, onBeforeUnmount, ref, useSlots, watch } from 'vue'
  import type { FormProps } from 'naive-ui/es/form'
  import type { SelectOption } from 'naive-ui/es/select'

  import { Icon } from '@/shared/icons'

  import AxisTableSearchField from './axis-table-search-field.vue'
  import type { AxisTableQuery, AxisTableSearchConfig, AxisTableSearchSchema } from '../types'

  defineOptions({ name: 'AxisTableSearchPanel' })

  const props = defineProps<{
    schemas: AxisTableSearchSchema[]
    model: AxisTableQuery
    config: Required<AxisTableSearchConfig>
    formProps?: Partial<FormProps>
    collapsible: boolean
    defaultCollapsed: boolean
    dictLoading: Record<string, boolean>
    readSchemaOptions: (schema: AxisTableSearchSchema) => SelectOption[]
    getValue: (field: string) => unknown
    setValue: (field: string, value: unknown) => void
  }>()

  const emit = defineEmits<{
    search: []
    reset: []
  }>()

  const slots = useSlots()
  const searchMainRef = ref<HTMLElement | null>(null)
  const advancedTriggerRef = ref<HTMLElement | null>(null)
  const searchMainWidth = ref(0)
  const columnCount = ref(Math.max(1, Math.floor(props.config.maxColumns)))
  const advancedVisible = ref(false)
  let resizeFrame = 0

  // 日期/下拉等浮层会 teleport 到 body，点击时不能当成“点了高级筛选外面”
  const OVERLAY_CLICK_IGNORE_SELECTORS = [
    '.n-date-panel',
    '.n-time-picker-panel',
    '.n-base-select-menu',
    '.n-cascader-menu',
    '.n-tree-select-menu',
    '.n-color-picker-panel',
    '.n-mention-menu',
    '.v-binder-follower-content'
  ].join(',')

  const minItemWidth = computed(() => Math.max(160, props.config.minItemWidth))
  const maxColumns = computed(() => Math.max(1, Math.floor(props.config.maxColumns)))
  const collapsedRows = computed(() => Math.max(1, Math.floor(props.config.collapsedRows)))

  function updateColumnCount() {
    const width = searchMainWidth.value
    if (!width) return

    const columnGap = 12
    const nextCount = Math.floor((width + columnGap) / (minItemWidth.value + columnGap))
    columnCount.value = Math.min(maxColumns.value, Math.max(1, nextCount))
  }

  useResizeObserver(searchMainRef, entries => {
    const width = entries[0]?.contentRect.width ?? 0
    window.cancelAnimationFrame(resizeFrame)
    resizeFrame = window.requestAnimationFrame(() => {
      searchMainWidth.value = width
      updateColumnCount()
    })
  })

  onBeforeUnmount(() => window.cancelAnimationFrame(resizeFrame))
  watch([minItemWidth, maxColumns], updateColumnCount)

  const inlineSchemas = computed(() => {
    if (!props.collapsible || !props.defaultCollapsed) return props.schemas

    const capacity = columnCount.value * collapsedRows.value
    const primarySchemas = props.schemas.filter(schema => schema.primary)
    const regularSchemas = props.schemas.filter(schema => !schema.primary && !schema.advanced)
    const selectedFields = new Set(primarySchemas.map(schema => schema.field))

    for (const schema of regularSchemas) {
      if (selectedFields.size >= Math.max(capacity, primarySchemas.length)) break
      selectedFields.add(schema.field)
    }

    return props.schemas.filter(schema => selectedFields.has(schema.field))
  })

  const advancedSchemas = computed(() => {
    const inlineFields = new Set(inlineSchemas.value.map(schema => schema.field))
    return props.schemas.filter(schema => !inlineFields.has(schema.field))
  })

  const activeAdvancedCount = computed(
    () =>
      advancedSchemas.value.filter(schema => hasSearchValue(props.getValue(schema.field))).length
  )

  const gridStyle = computed(() => ({
    gridTemplateColumns: `repeat(${columnCount.value}, minmax(0, 1fr))`
  }))

  function getInlineItemStyle(schema: AxisTableSearchSchema) {
    if (schema.fullWidth) return { gridColumn: '1 / -1' }
    const span = Math.min(columnCount.value, Math.max(1, Math.floor(schema.span ?? 1)))
    return span > 1 ? { gridColumn: `span ${span}` } : undefined
  }

  function getAdvancedItemStyle(schema: AxisTableSearchSchema) {
    if (schema.fullWidth || (schema.span ?? 1) > 1) return { gridColumn: '1 / -1' }
    return undefined
  }

  function hasCustomSlot(schema: AxisTableSearchSchema) {
    return Boolean(slots['search-item'] || slots[`search-${schema.field}`])
  }

  function hasSearchValue(value: unknown) {
    if (value === null || value === undefined || value === '') return false
    return !Array.isArray(value) || value.length > 0
  }

  function cloneSearchValue(value: unknown) {
    if (Array.isArray(value)) return [...value]
    if (value && typeof value === 'object') return { ...(value as Record<string, unknown>) }
    return value
  }

  function resetAdvancedFilters() {
    advancedSchemas.value.forEach(schema => {
      props.setValue(schema.field, cloneSearchValue(schema.defaultValue ?? null))
    })
  }

  function applyAdvancedFilters() {
    advancedVisible.value = false
    emit('search')
  }

  function runSearch() {
    emit('search')
  }

  function resetSearch() {
    advancedVisible.value = false
    emit('reset')
  }

  function toggleAdvancedPanel() {
    advancedVisible.value = !advancedVisible.value
  }

  function handleAdvancedClickOutside(event: MouseEvent) {
    const target = event.target
    if (!(target instanceof Element)) {
      advancedVisible.value = false
      return
    }

    // 再次点击触发按钮由 toggle 处理，避免这里先关再被点开
    if (advancedTriggerRef.value?.contains(target)) return

    // 忽略挂到 body 的浮层交互（日期范围选择最常见）
    if (target.closest(OVERLAY_CLICK_IGNORE_SELECTORS)) return

    advancedVisible.value = false
  }
</script>

<template>
  <div class="axis-table-search">
    <slot
      name="search"
      :model="model"
      :schemas="schemas"
      :search="runSearch"
      :reset="resetSearch"
      :set-value="setValue"
      :get-value="getValue"
    >
      <n-form
        :model="model"
        label-placement="left"
        label-width="auto"
        :show-feedback="false"
        v-bind="formProps"
      >
        <div class="axis-table-search__layout">
          <div ref="searchMainRef" class="axis-table-search__main" :style="gridStyle">
            <AxisTableSearchField
              v-for="schema in inlineSchemas"
              :key="schema.field"
              :schema="schema"
              :value="getValue(schema.field)"
              :options="readSchemaOptions(schema)"
              :loading="dictLoading[schema.field]"
              :label-width="config.labelWidth"
              :style="getInlineItemStyle(schema)"
              @update:value="value => setValue(schema.field, value)"
              @submit="runSearch"
            >
              <template v-if="hasCustomSlot(schema)" #custom>
                <slot
                  name="search-item"
                  :schema="schema"
                  :model="model"
                  :value="getValue(schema.field)"
                  :set-value="setValue"
                  :search="runSearch"
                  :reset="resetSearch"
                >
                  <slot
                    :name="`search-${schema.field}`"
                    :schema="schema"
                    :model="model"
                    :value="getValue(schema.field)"
                    :set-value="setValue"
                    :search="runSearch"
                    :reset="resetSearch"
                  />
                </slot>
              </template>
            </AxisTableSearchField>
          </div>

          <div class="axis-table-search__actions">
            <n-button type="primary" size="small" @click="runSearch">查询</n-button>
            <n-button secondary size="small" @click="resetSearch">重置</n-button>

            <n-popover
              v-if="advancedSchemas.length"
              v-model:show="advancedVisible"
              trigger="manual"
              placement="bottom-end"
              :show-arrow="false"
              :style="{
                width: 'min(680px, calc(100vw - 32px))',
                padding: 0,
                borderRadius: '10px',
                overflow: 'visible'
              }"
              @clickoutside="handleAdvancedClickOutside"
            >
              <template #trigger>
                <span ref="advancedTriggerRef" class="axis-table-search__advanced-trigger">
                  <n-button secondary type="primary" size="small" @click="toggleAdvancedPanel">
                    <template #icon>
                      <Icon icon="ant-design:filter-outlined" />
                    </template>
                    高级筛选
                    <span v-if="activeAdvancedCount" class="axis-table-search__active-count">
                      {{ activeAdvancedCount }}
                    </span>
                  </n-button>
                </span>
              </template>

              <div class="axis-table-search__advanced">
                <div class="axis-table-search__advanced-header">
                  <div>
                    <div class="axis-table-search__advanced-title">高级筛选</div>
                    <div class="axis-table-search__advanced-desc">设置更多条件以缩小查询范围</div>
                  </div>
                  <div
                    v-if="activeAdvancedCount"
                    class="axis-table-search__advanced-summary is-active"
                  >
                    已设置 {{ activeAdvancedCount }} 项
                  </div>
                  <div v-else class="axis-table-search__advanced-summary">
                    共 {{ advancedSchemas.length }} 项
                  </div>
                </div>

                <div class="axis-table-search__advanced-body">
                  <div class="axis-table-search__advanced-grid">
                    <div
                      v-for="schema in advancedSchemas"
                      :key="schema.field"
                      class="axis-table-search__advanced-item"
                      :style="getAdvancedItemStyle(schema)"
                    >
                      <AxisTableSearchField
                        :schema="schema"
                        :value="getValue(schema.field)"
                        :options="readSchemaOptions(schema)"
                        :loading="dictLoading[schema.field]"
                        :label-width="config.labelWidth"
                        @update:value="value => setValue(schema.field, value)"
                        @submit="applyAdvancedFilters"
                      >
                        <template v-if="hasCustomSlot(schema)" #custom>
                          <slot
                            name="search-item"
                            :schema="schema"
                            :model="model"
                            :value="getValue(schema.field)"
                            :set-value="setValue"
                            :search="applyAdvancedFilters"
                            :reset="resetSearch"
                          >
                            <slot
                              :name="`search-${schema.field}`"
                              :schema="schema"
                              :model="model"
                              :value="getValue(schema.field)"
                              :set-value="setValue"
                              :search="applyAdvancedFilters"
                              :reset="resetSearch"
                            />
                          </slot>
                        </template>
                      </AxisTableSearchField>
                    </div>
                  </div>
                </div>

                <div class="axis-table-search__advanced-footer">
                  <n-button quaternary size="small" @click="resetAdvancedFilters">
                    恢复默认
                  </n-button>
                  <n-button type="primary" size="small" @click="applyAdvancedFilters">
                    查询
                  </n-button>
                </div>
              </div>
            </n-popover>
          </div>
        </div>
      </n-form>
    </slot>
  </div>
</template>

<style scoped lang="scss">
  .axis-table-search {
    margin-bottom: 8px;
    padding: 10px 12px;
    border: 1px solid var(--axis-border-color);
    border-radius: 8px;
    background: var(--axis-card-color);

    &__layout {
      display: flex;
      align-items: flex-start;
      gap: 12px;
    }

    &__main {
      display: grid;
      min-width: 0;
      flex: 1 1 auto;
      gap: 8px 12px;
    }

    &__actions {
      display: inline-flex;
      height: 30px;
      flex: 0 0 auto;
      align-items: center;
      gap: 8px;
      white-space: nowrap;
    }

    &__active-count {
      display: inline-flex;
      min-width: 18px;
      height: 18px;
      align-items: center;
      justify-content: center;
      margin: -1px -3px -1px 2px;
      padding: 0 4px;
      border-radius: 10px;
      background: var(--axis-primary-color);
      color: #fff;
      font-size: 11px;
      font-weight: 600;
      line-height: 18px;
    }

    &__advanced-trigger {
      display: inline-flex;
    }

    &__advanced {
      background: var(--axis-card-color);
      color: var(--axis-text-color-1);
    }

    &__advanced-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 16px;
      padding: 18px 20px 15px;
      border-bottom: 1px solid var(--axis-border-color);
      border-radius: 10px 10px 0 0;
    }

    &__advanced-title {
      color: var(--axis-text-color-1);
      font-size: 15px;
      font-weight: 600;
      line-height: 22px;
    }

    &__advanced-desc {
      margin-top: 1px;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 18px;
    }

    &__advanced-summary {
      flex: 0 0 auto;
      padding-top: 2px;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 20px;

      &.is-active {
        color: var(--axis-primary-color);
      }
    }

    &__advanced-body {
      max-height: min(440px, 58vh);
      padding: 20px;
      overflow: auto;
    }

    &__advanced-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 20px 24px;
    }

    &__advanced-item {
      min-width: 0;

      :deep(.axis-table-search-field) {
        min-height: 0;
        flex-direction: column;
        align-items: stretch;
        gap: 8px;
      }

      :deep(.axis-table-search-field__label) {
        width: auto !important;
        height: 20px;
        flex: none !important;
        color: var(--axis-text-color-2);
        font-size: 13px;
        font-weight: 500;
        line-height: 20px;
      }

      :deep(.axis-table-search-field__control) {
        width: 100%;
      }
    }

    &__advanced-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      padding: 11px 20px;
      border-top: 1px solid var(--axis-border-color);
      border-radius: 0 0 10px 10px;
      background: color-mix(in srgb, var(--axis-card-color) 97%, var(--axis-bg-color));
    }
  }

  @media (max-width: 720px) {
    .axis-table-search {
      &__layout {
        flex-wrap: wrap;
      }

      &__main,
      &__actions {
        width: 100%;
      }

      &__actions {
        justify-content: flex-end;
      }

      &__advanced-grid {
        grid-template-columns: minmax(0, 1fr);
      }

      &__advanced-header,
      &__advanced-body,
      &__advanced-footer {
        padding-right: 14px;
        padding-left: 14px;
      }

      &__advanced-desc {
        display: none;
      }
    }
  }
</style>
