<script setup lang="ts">
  import { computed, ref } from 'vue'

  import type { OperateLogDto } from '@/modules/system/api/log'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'

  import { buildAuditDetailFields } from '../../shared/display'

  defineOptions({ name: 'OperateLogDetailDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const detail = ref<OperateLogDto | null>(null)

  /* ============ 视图模型 ============ */

  const detailFields = computed<DetailField[]>(() =>
    detail.value ? buildAuditDetailFields(detail.value) : []
  )

  /* ============ 交互操作 ============ */

  function open(row: OperateLogDto) {
    detail.value = row
    visible.value = true
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="操作日志详情"
    :subtitle="detail?.displayTitle || detail?.requestUrl"
    icon="ant-design:audit-outlined"
    :width="760"
    :fields="detailFields"
    section-title="请求信息"
  />
</template>
