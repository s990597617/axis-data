<script setup lang="ts">
  import { computed, ref } from 'vue'

  import type { VisitLogDto } from '@/modules/system/api/log'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'

  import { buildAuditDetailFields } from '../../shared/display'

  defineOptions({ name: 'VisitLogDetailDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const detail = ref<VisitLogDto | null>(null)

  /* ============ 视图模型 ============ */

  const detailFields = computed<DetailField[]>(() =>
    detail.value ? buildAuditDetailFields(detail.value) : []
  )

  /* ============ 交互操作 ============ */

  function open(row: VisitLogDto) {
    detail.value = row
    visible.value = true
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="访问日志详情"
    :subtitle="detail?.displayTitle || detail?.requestUrl"
    icon="ant-design:eye-outlined"
    :width="760"
    :fields="detailFields"
    section-title="请求信息"
  />
</template>
