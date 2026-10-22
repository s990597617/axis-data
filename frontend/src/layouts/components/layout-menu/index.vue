<script setup lang="ts">
  import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'
  import {
    layoutMenuNodeProps,
    useLayoutMenu
  } from '@/layouts/composables/use-layout-menu'
  import { useAppStore } from '@/store/modules/app.store'

  // 侧边菜单渲染：数据来自 use-layout-menu，折叠态来自 use-layout-collapse
  const { menuOptions, expandKeys, updateExpandKeys, currentMenu, updateValue } = useLayoutMenu()
  const { collapsed } = useLayoutCollapse()
  const appStore = useAppStore()
</script>

<template>
  <n-menu
    class="layout-menu"
    :options="menuOptions"
    :collapsed="collapsed"
    :collapsed-width="appStore.layout.siderCollapsedWidth"
    :collapsed-icon-size="20"
    :indent="18"
    :root-indent="18"
    :accordion="appStore.layout.accordionMenu"
    :expanded-keys="expandKeys"
    :value="currentMenu"
    :node-props="layoutMenuNodeProps"
    :on-update:expanded-keys="updateExpandKeys"
    :on-update:value="updateValue"
  />
</template>

<style scoped lang="scss">
  .layout-menu {
    // 三级无图标：左内边距对齐二级标题文字（图标宽 + 间距约 26px，叠在二级 padding 上）
    :deep(.n-menu-item.axis-menu-level-3 > .n-menu-item-content) {
      padding-left: 45px !important;
    }
  }
</style>
