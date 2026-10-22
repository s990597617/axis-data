<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { NButton, NEmpty } from 'naive-ui'

  import { getCacheEntry, type CacheEntryDto } from '@/modules/system/api/cache'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'
  import { tryRequest } from '@/shared/http'
  import { Icon } from '@/shared/icons'
  import { message } from '@/shared/utils/message'

  import { formatTtl, formatValuePreview } from '../utils/display'

  defineOptions({ name: 'CacheDetailDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const loading = ref(false)
  const detail = ref<CacheEntryDto | null>(null)

  /* ============ 视图模型 ============ */

  const previewText = computed(() => formatValuePreview(detail.value?.valuePreview))

  const statusText = computed(() => {
    if (!detail.value) return undefined
    return detail.value.exists ? '存在' : '不存在'
  })

  const statusType = computed(() => {
    if (!detail.value) return undefined
    return detail.value.exists ? ('success' as const) : ('warning' as const)
  })

  const detailFields = computed<DetailField[]>(() => {
    const row = detail.value
    if (!row) return []
    return [
      {
        key: 'valueType',
        label: '值类型',
        value: row.valueType || '-',
        icon: 'ant-design:tag-outlined',
        ellipsis: true
      },
      {
        key: 'ttl',
        label: '剩余 TTL',
        value: formatTtl(row.expirationSeconds),
        icon: 'ant-design:clock-circle-outlined'
      },
      {
        key: 'exists',
        label: '键状态',
        value: row.exists ? '存在' : '不存在',
        type: 'tag',
        tagType: row.exists ? 'success' : 'warning',
        icon: 'ant-design:check-circle-outlined'
      },
      {
        key: 'truncated',
        label: '预览',
        value: row.previewTruncated ? '已截断' : '完整',
        type: 'tag',
        tagType: row.previewTruncated ? 'warning' : 'default',
        icon: 'ant-design:file-text-outlined'
      }
    ]
  })

  /* ============ 交互操作 ============ */

  async function open(key: string) {
    visible.value = true
    loading.value = true
    detail.value = null

    const [data, error] = await tryRequest(getCacheEntry({ key }))
    loading.value = false
    if (error) {
      visible.value = false
      return
    }
    detail.value = data ?? null
  }

  async function copyPreview() {
    if (!previewText.value) {
      message.warning('无可复制内容')
      return
    }
    try {
      await navigator.clipboard.writeText(previewText.value)
      message.success('已复制预览内容')
    } catch {
      message.error('复制失败')
    }
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="缓存详情"
    :subtitle="detail?.key"
    icon="ant-design:database-outlined"
    :status-text="statusText"
    :status-type="statusType"
    :loading="loading"
    :width="760"
    :fields="detailFields"
    section-title="基本信息"
    fill-content
  >
    <section v-if="detail" class="cache-detail">
      <div class="cache-detail__preview">
        <div class="cache-detail__preview-header">
          <span class="cache-detail__preview-title">缓存值预览</span>
          <div class="cache-detail__preview-actions">
            <span v-if="detail.previewTruncated" class="cache-detail__preview-hint">
              内容已截断
            </span>
            <NButton
              size="tiny"
              secondary
              :disabled="!previewText"
              @click="copyPreview"
            >
              <template #icon>
                <Icon icon="ant-design:copy-outlined" />
              </template>
              复制
            </NButton>
          </div>
        </div>

        <pre v-if="previewText" class="cache-detail__code">{{ previewText }}</pre>
        <NEmpty
          v-else
          class="cache-detail__empty"
          description="无缓存值或无法序列化"
        />
      </div>
    </section>
  </DetailDrawer>
</template>

<style scoped lang="scss">
  @use '../styles/detail-drawer.scss';
</style>
