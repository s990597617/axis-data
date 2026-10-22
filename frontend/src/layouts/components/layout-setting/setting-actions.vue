<script setup lang="ts">
import { Icon } from '@/shared/icons'
import { message } from '@/shared/utils/message'
import { useAppStore } from '@/store/modules/app.store'
import { useThemeStore } from '@/store/modules/theme.store'

const appStore = useAppStore()
const themeStore = useThemeStore()

async function handleCopy() {
  const payload = {
    layout: { ...appStore.layout },
    theme: { ...themeStore.config },
  }
  try {
    await navigator.clipboard.writeText(JSON.stringify(payload, null, 2))
    message.success('配置已复制到剪贴板')
  } catch {
    message.error('复制失败：剪贴板不可用')
  }
}

function handleReset() {
  appStore.resetLayout()
  themeStore.reset()
  message.success('已恢复默认配置')
}
</script>

<template>
  <div class="pref-actions">
    <n-button block type="primary" @click="handleCopy">
      <template #icon><Icon icon="ant-design:copy-outlined" /></template>
      复制配置
    </n-button>
    <n-button block @click="handleReset">
      <template #icon><Icon icon="ant-design:reload-outlined" /></template>
      重置配置
    </n-button>
  </div>
</template>
