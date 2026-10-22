<script setup lang="ts">
import LayoutContent from '@/layouts/components/layout-content/index.vue'
import LayoutFooter from '@/layouts/components/layout-footer/index.vue'
import LayoutHeader from '@/layouts/components/layout-header/index.vue'
import LayoutSetting from '@/layouts/components/layout-setting/index.vue'
import LayoutSider from '@/layouts/components/layout-sider/index.vue'
import LayoutTabs from '@/layouts/components/layout-tabs/index.vue'
import { useAppStore } from '@/store/modules/app.store'

/**
 * VerticalLayout
 *
 * @description
 * 垂直布局模式：左侧主菜单 + 顶部工具栏 + 内容区（含标签页、页脚）。
 * - 面包屑已内嵌在 `LayoutHeader`，不再单独占一栏
 * - `fixedHeader = true` 时 header / tabs 贴顶不随内容滚动；为 false 时随内容滚动
 * - `showFooter = true` 时在内容底部渲染 `LayoutFooter`
 *
 * 本组件只负责"拼装"，所有子能力均来自 `layouts/components` 与 `layouts/composables`。
 */
const appStore = useAppStore()
</script>

<template>
  <n-layout has-sider class="vertical-layout">
    <LayoutSider />
    <div class="vertical-layout__main">
      <!-- 固定头部：header + tabs 贴顶，内容独立滚动 -->
      <template v-if="appStore.layout.fixedHeader">
        <LayoutHeader v-if="appStore.layout.showHeader" />
        <LayoutTabs v-if="appStore.layout.showTabs" />
        <LayoutContent />
      </template>

      <!-- 非固定头部：header + tabs 在滚动容器内，随内容滚走 -->
      <template v-else>
        <LayoutContent>
          <template #before>
            <LayoutHeader v-if="appStore.layout.showHeader" />
            <LayoutTabs v-if="appStore.layout.showTabs" />
          </template>
        </LayoutContent>
      </template>

      <!-- 页脚：始终固定在视口底部，不随内容滚动 -->
      <LayoutFooter v-if="appStore.layout.showFooter" />
    </div>
    <LayoutSetting />
  </n-layout>
</template>

<style scoped lang="scss">
.vertical-layout {
  height: 100vh;
  overflow: hidden;

  /* naive-ui 的 n-layout 默认会在根与滚动容器上各加 overflow:auto；此处强制收敛，只让 LayoutContent 滚 */
  :deep(.n-layout-scroll-container) {
    overflow: hidden !important;
  }

  &__main {
    display: flex;
    flex-direction: column;
    overflow: hidden;
    flex: 1;
    min-width: 0;
    height: 100%;
  }
}
</style>
