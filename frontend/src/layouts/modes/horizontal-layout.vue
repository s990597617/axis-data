<script setup lang="ts">
import LayoutContent from '@/layouts/components/layout-content/index.vue'
import LayoutFooter from '@/layouts/components/layout-footer/index.vue'
import LayoutHeader from '@/layouts/components/layout-header/index.vue'
import LayoutLogo from '@/layouts/components/layout-logo/index.vue'
import LayoutSetting from '@/layouts/components/layout-setting/index.vue'
import LayoutTabs from '@/layouts/components/layout-tabs/index.vue'
import { useLayoutMenu } from '@/layouts/composables/use-layout-menu'
import { useAppStore } from '@/store/modules/app.store'

/**
 * HorizontalLayout
 *
 * @description
 * 水平布局：顶部承载完整菜单（二级及以下使用下拉形式），主内容区占据整屏。
 * 适合一级栏目较少、需要最大化展示内容的平台型系统。
 */
const appStore = useAppStore()
const { menuOptions, currentMenu, updateValue } = useLayoutMenu()
</script>

<template>
  <div class="horizontal-layout">
    <header class="horizontal-layout__top">
      <div
        v-if="appStore.layout.showLogo"
        class="horizontal-layout__brand"
        :style="{ width: `${appStore.layout.siderWidth}px` }"
      >
        <LayoutLogo />
      </div>
      <div class="horizontal-layout__menu">
        <n-menu
          mode="horizontal"
          :options="menuOptions"
          :value="currentMenu"
          :on-update:value="updateValue"
          responsive
        />
      </div>
      <div class="horizontal-layout__tools">
        <LayoutHeader :show-collapse="false" :show-breadcrumb="false" />
      </div>
    </header>

    <div class="horizontal-layout__main">
      <LayoutTabs v-if="appStore.layout.showTabs" />
      <LayoutContent />
      <LayoutFooter v-if="appStore.layout.showFooter" />
    </div>

    <LayoutSetting />
  </div>
</template>

<style scoped lang="scss">
.horizontal-layout {
  height: 100vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;

  &__top {
    display: flex;
    align-items: center;
    height: 56px;
    background: var(--axis-card-color);
    border-bottom: 1px solid var(--axis-border-color);
    flex-shrink: 0;
  }

  &__brand {
    flex-shrink: 0;
    height: 100%;
    display: flex;
    align-items: stretch;
    border-right: 1px solid var(--axis-border-color);
    transition: width 0.2s var(--axis-bezier, ease);

    :deep(.layout-logo) {
      flex: 1;
      height: 100%;
      padding: 0 12px;
      border-bottom: none;
      border-right: none;
    }
  }

  &__menu {
    flex: 1;
    min-width: 0;
    overflow: hidden;
    align-self: stretch;
    display: flex;
    align-items: center;

    :deep(.n-menu.n-menu--horizontal) {
      display: flex;
      align-items: center;
      height: 100%;
      width: 100%;
    }
  }

  &__tools {
    flex-shrink: 0;
    align-self: stretch;
    display: flex;
    align-items: center;

    :deep(.layout-header) {
      border-bottom: none;
      background: transparent;
    }
  }

  &__main {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }
}
</style>
