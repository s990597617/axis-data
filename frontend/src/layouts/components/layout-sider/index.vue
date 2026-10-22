<script setup lang="ts">
import LayoutLogo from '@/layouts/components/layout-logo/index.vue'
import LayoutMenu from '@/layouts/components/layout-menu/index.vue'
import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'
import { useAppStore } from '@/store/modules/app.store'

/**
 * LayoutSider
 *
 * @description
 * 侧边栏容器，负责组装 Logo + 菜单 + 滚动区，处理折叠宽度。
 * 不关心菜单数据与业务权限。
 */
const { collapsed } = useLayoutCollapse()
const appStore = useAppStore()
</script>

<template>
  <n-layout-sider
    v-if="appStore.layout.showSidebar"
    bordered
    collapse-mode="width"
    :collapsed="collapsed"
    :width="appStore.layout.siderWidth"
    :collapsed-width="appStore.layout.siderCollapsedWidth"
    :native-scrollbar="false"
    class="layout-sider"
  >
    <LayoutLogo v-if="appStore.layout.showLogo" />
    <n-scrollbar class="layout-sider__menu">
      <LayoutMenu />
    </n-scrollbar>
  </n-layout-sider>
</template>

<style scoped lang="scss">
.layout-sider {
  background: var(--axis-sider-bg) !important;

  :deep(.n-menu),
  :deep(.n-scrollbar) {
    background: var(--axis-sider-bg);
  }

  &__menu {
    height: calc(100vh - 56px);
  }
}
</style>
