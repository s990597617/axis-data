<script setup lang="ts">
  import { computed, ref } from 'vue'

  import type { LoginLogDto } from '@/modules/system/api/log'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'
  import { formatLocalDateTime } from '@/shared/utils/date'

  defineOptions({ name: 'LoginLogDetailDrawer' })

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const detail = ref<LoginLogDto | null>(null)

  /* ============ 视图模型 ============ */

  const detailFields = computed<DetailField[]>(() => {
    const row = detail.value
    if (!row) return []
    return [
      {
        key: 'account',
        label: '账号',
        value: row.account,
        icon: 'ant-design:user-outlined'
      },
      {
        key: 'success',
        label: '登录结果',
        value: row.success ? '成功' : '失败',
        type: 'tag',
        tagType: row.success ? 'success' : 'error',
        icon: 'ant-design:check-circle-outlined'
      },
      {
        key: 'remoteIp',
        label: 'IP 地址',
        value: row.remoteIp || '-',
        icon: 'ant-design:global-outlined'
      },
      {
        key: 'createdTime',
        label: '登录时间',
        value: row.createdTime ? formatLocalDateTime(row.createdTime) : '-',
        icon: 'ant-design:clock-circle-outlined'
      },
      {
        key: 'failReason',
        label: '失败原因',
        value: row.failReason || '-',
        fullRow: true,
        icon: 'ant-design:warning-outlined'
      },
      {
        key: 'userAgent',
        label: 'User-Agent',
        value: row.userAgent || '-',
        fullRow: true,
        icon: 'ant-design:chrome-outlined'
      }
    ]
  })

  const statusText = computed(() =>
    detail.value ? (detail.value.success ? '成功' : '失败') : undefined
  )
  const statusType = computed(() =>
    detail.value?.success ? 'success' : 'error'
  )

  /* ============ 交互操作 ============ */

  function open(row: LoginLogDto) {
    detail.value = row
    visible.value = true
  }

  defineExpose({ open })
</script>

<template>
  <DetailDrawer
    v-model:show="visible"
    title="登录日志详情"
    :subtitle="detail?.account"
    icon="ant-design:login-outlined"
    :status-text="statusText"
    :status-type="statusType"
    :width="640"
    :fields="detailFields"
    section-title="基本信息"
  />
</template>
