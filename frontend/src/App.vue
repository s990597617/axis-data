<script setup lang="ts">
  import { dateZhCN, zhCN } from 'naive-ui'
  import { useTheme } from '@/layouts/composables/use-theme.ts'

  /**
   * App
   *
   * @description
   * 应用根组件：
   *   - 主题（暗/亮 / 主色 / 圆角）来自 `useTheme`
   */
  const { theme, themeOverrides } = useTheme()
</script>

<template>
  <n-config-provider
    :theme="theme"
    :theme-overrides="themeOverrides"
    :locale="zhCN"
    :date-locale="dateZhCN"
  >
    <router-view />
  </n-config-provider>
</template>
