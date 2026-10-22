<script setup lang="ts">
  import { computed } from 'vue'
  import { NImage, NImageGroup, NTag, NTooltip } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import type { DetailField } from './types'

  defineOptions({ name: 'DetailFieldGrid' })

  const props = withDefaults(
    defineProps<{
      fields: DetailField[]
      columns?: number
      labelWidth?: string | number
    }>(),
    {
      columns: 2,
      labelWidth: 96
    }
  )

  defineSlots<{
    field?: (props: { field: DetailField; displayValue: string }) => unknown
  }>()

  const columnCount = computed(() => Math.max(1, Math.floor(props.columns)))
  const gridStyle = computed(() => ({
    gridTemplateColumns: `repeat(${columnCount.value}, minmax(0, 1fr))`,
    '--detail-label-width': toCssSize(props.labelWidth)
  }))

  function toCssSize(value: string | number) {
    return typeof value === 'number' ? `${value}px` : value
  }

  function getFieldStyle(field: DetailField) {
    const span = field.fullRow
      ? columnCount.value
      : Math.min(columnCount.value, Math.max(1, Math.floor(field.span ?? 1)))
    return span > 1 ? { gridColumn: `span ${span}` } : undefined
  }

  function readDisplayValue(field: DetailField) {
    const formatted = field.formatter?.(field.value, field)
    const value = formatted ?? field.value

    if (value === null || value === undefined || value === '') return field.emptyText ?? '-'
    if (Array.isArray(value)) return value.length ? value.join('、') : (field.emptyText ?? '-')
    if (typeof value === 'boolean') return value ? '是' : '否'
    return String(value)
  }

  function readImages(field: DetailField) {
    if (Array.isArray(field.value)) return field.value.filter(Boolean)
    return typeof field.value === 'string' && field.value ? [field.value] : []
  }
</script>

<template>
  <div class="detail-field-grid" :style="gridStyle">
    <div
      v-for="(field, index) in fields"
      :key="field.key ?? `${field.label}-${index}`"
      class="detail-field-grid__item"
      :class="{ 'is-full': field.fullRow }"
      :style="getFieldStyle(field)"
    >
      <div class="detail-field-grid__label">
        <Icon v-if="field.icon" :icon="field.icon" class="detail-field-grid__icon" />
        <span>{{ field.label }}</span>
      </div>

      <div class="detail-field-grid__value">
        <slot name="field" :field="field" :display-value="readDisplayValue(field)">
          <n-tooltip
            v-if="field.ellipsis && field.type !== 'tag' && field.type !== 'image' && field.type !== 'pre'"
            placement="top"
            :style="{ maxWidth: 'min(80vw, 720px)' }"
          >
            <template #trigger>
              <code v-if="field.type === 'code'" class="detail-field-grid__code is-ellipsis">
                {{ readDisplayValue(field) }}
              </code>
              <span v-else class="detail-field-grid__text is-ellipsis">
                {{ readDisplayValue(field) }}
              </span>
            </template>
            <div class="detail-field-grid__tooltip">
              {{ readDisplayValue(field) }}
            </div>
          </n-tooltip>

          <pre v-else-if="field.type === 'pre'" class="detail-field-grid__pre">{{
            readDisplayValue(field)
          }}</pre>

          <code v-else-if="field.type === 'code'" class="detail-field-grid__code">
            {{ readDisplayValue(field) }}
          </code>

          <n-tag
            v-else-if="field.type === 'tag'"
            size="small"
            round
            :bordered="false"
            :type="field.tagType ?? 'default'"
          >
            {{ readDisplayValue(field) }}
          </n-tag>

          <n-image-group v-else-if="field.type === 'image' && readImages(field).length">
            <div class="detail-field-grid__images">
              <n-image
                v-for="image in readImages(field)"
                :key="image"
                :src="image"
                width="56"
                height="56"
                object-fit="cover"
                class="detail-field-grid__image"
              />
            </div>
          </n-image-group>

          <span v-else class="detail-field-grid__text">{{ readDisplayValue(field) }}</span>
        </slot>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
  .detail-field-grid {
    display: grid;
    gap: 0 28px;
    min-width: 0;

    &__item {
      display: grid;
      min-width: 0;
      grid-template-columns: var(--detail-label-width) minmax(0, 1fr);
      align-items: start;
      gap: 10px;
      padding: 10px 0;
      border-bottom: 1px dashed color-mix(in srgb, var(--axis-border-color) 78%, transparent);

      &.is-full {
        grid-template-columns: var(--detail-label-width) minmax(0, 1fr);
      }

      &:last-child {
        border-bottom: 0;
        padding-bottom: 2px;
      }
    }

    &__label {
      display: flex;
      min-width: 0;
      align-items: center;
      gap: 6px;
      color: var(--axis-text-color-3);
      font-size: 13px;
      font-weight: 500;
      line-height: 22px;
    }

    &__icon {
      flex: 0 0 auto;
      font-size: 14px;
      color: color-mix(in srgb, var(--axis-primary-color) 65%, var(--axis-text-color-3));
    }

    &__value {
      min-width: 0;
      color: var(--axis-text-color-1);
      font-size: 13px;
      font-weight: 500;
      line-height: 22px;
      overflow-wrap: anywhere;
    }

    &__text {
      white-space: pre-wrap;

      &.is-ellipsis {
        display: block;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }

    &__code {
      display: inline-block;
      max-width: 100%;
      padding: 1px 8px;
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-primary-color) 10%, var(--axis-card-color));
      color: var(--axis-primary-color);
      font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
      font-size: 12px;
      font-weight: 600;
      line-height: 1.5;
      overflow-wrap: anywhere;

      &.is-ellipsis {
        display: block;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }

    &__pre {
      margin: 0;
      max-width: 100%;
      max-height: min(48vh, 420px);
      overflow: auto;
      padding: 12px 14px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 72%, transparent);
      border-radius: var(--axis-border-radius, 6px);
      background: color-mix(in srgb, var(--axis-bg-color) 55%, var(--axis-card-color));
      color: var(--axis-text-color-1);
      font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
      font-size: 12px;
      font-weight: 500;
      line-height: 1.6;
      white-space: pre-wrap;
      word-break: break-word;
      overflow-wrap: anywhere;
    }

    &__images {
      display: flex;
      flex-wrap: wrap;
      gap: 6px;
    }

    &__image {
      overflow: hidden;
      border-radius: var(--axis-border-radius, 6px);
    }
  }

  @media (max-width: 720px) {
    .detail-field-grid {
      grid-template-columns: minmax(0, 1fr) !important;
      gap: 0;

      &__item {
        grid-column: span 1 !important;
        grid-template-columns: minmax(72px, 30%) minmax(0, 1fr);
      }
    }
  }
</style>

<!-- tooltip 传送到 body，需非 scoped 才能保证换行样式生效 -->
<style lang="scss">
  .detail-field-grid__tooltip {
    max-width: min(80vw, 720px);
    white-space: pre-wrap;
    word-break: break-all;
    overflow-wrap: anywhere;
    line-height: 1.55;
  }
</style>
