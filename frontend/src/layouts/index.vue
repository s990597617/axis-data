<script setup lang="ts">
import { useLayoutMode } from '@/layouts/composables/use-layout-mode'
import LayoutWatermark from '@/layouts/components/layout-watermark/index.vue'

/**
 * AppLayout
 *
 * @description
 * 应用布局统一入口，根据 `app.store.layoutMode` 动态渲染对应布局模式组件。
 * 本文件不得承载任何业务逻辑与具体布局结构。
 */
const { layoutComponent } = useLayoutMode()
</script>

<template>
  <component :is="layoutComponent" />
  <LayoutWatermark />
</template>
