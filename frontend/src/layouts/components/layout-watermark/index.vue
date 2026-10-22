<script setup lang="ts">
import { computed } from 'vue'
import { NWatermark } from 'naive-ui'

import { useAppStore } from '@/store/modules/app.store'
import { useUserStore } from '@/store/modules/user.store'

/**
 * 全局水印
 *
 * @description
 * 由布局偏好 `showWatermark` 控制；文案优先取自定义内容，否则回退为「姓名 · 机构」。
 */
const appStore = useAppStore()
const userStore = useUserStore()

const enabled = computed(() => appStore.layout.showWatermark)

const content = computed(() => {
  const custom = appStore.layout.watermarkContent?.trim()
  if (custom) return custom

  const info = userStore.userInfo
  if (!info) return 'Axis'

  const name = info.nickname || info.username || ''
  const org = info.orgName || ''
  return [name, org].filter(Boolean).join(' · ') || 'Axis'
})
</script>

<template>
  <NWatermark
    v-if="enabled"
    :content="content"
    cross
    fullscreen
    :font-size="14"
    :line-height="16"
    :width="320"
    :height="240"
    :x-offset="12"
    :y-offset="60"
    :rotate="-20"
    font-color="rgba(128, 128, 128, 0.16)"
  />
</template>
