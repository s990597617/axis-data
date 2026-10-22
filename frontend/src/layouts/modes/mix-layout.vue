<script setup lang="ts">
import LayoutContent from '@/layouts/components/layout-content/index.vue'
import LayoutFooter from '@/layouts/components/layout-footer/index.vue'
import LayoutHeader from '@/layouts/components/layout-header/index.vue'
import LayoutLogo from '@/layouts/components/layout-logo/index.vue'
import LayoutSetting from '@/layouts/components/layout-setting/index.vue'
import LayoutTabs from '@/layouts/components/layout-tabs/index.vue'
import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'
import { layoutMenuNodeProps, useLayoutMenu } from '@/layouts/composables/use-layout-menu'
import { useAppStore } from '@/store/modules/app.store'

/**
 * MixLayout
 *
 * @description
 * 混合布局：顶部展示一级菜单，左侧展示当前一级菜单下的二级菜单树。
 * 适合一级域明确、二级菜单较丰富的复杂中后台。
 */
const appStore = useAppStore()
const { collapsed } = useLayoutCollapse()
const {
  rootMenus,
  activeRootKey,
  subMenus,
  expandKeys,
  updateExpandKeys,
  currentMenu,
  updateValue,
  setActiveRoot,
} = useLayoutMenu()
</script>

<template>
  <div class="mix-layout">
    <!-- 顶部：Logo + 一级菜单 + 工具区 -->
    <header class="mix-layout__top">
      <div
        v-if="appStore.layout.showLogo"
        class="mix-layout__brand"
        :style="{
          width: `${collapsed ? appStore.layout.siderCollapsedWidth : appStore.layout.siderWidth}px`
        }"
      >
        <LayoutLogo />
      </div>
      <div class="mix-layout__top-menu">
        <n-menu
          mode="horizontal"
          :options="rootMenus"
          :value="activeRootKey"
          :on-update:value="setActiveRoot"
          responsive
        />
      </div>
      <div class="mix-layout__top-tools">
        <LayoutHeader :show-collapse="subMenus.length > 0" :show-breadcrumb="false" />
      </div>
    </header>

    <!-- 主体：侧边二级菜单 + 内容区 -->
    <n-layout has-sider class="mix-layout__body">
      <n-layout-sider
        v-if="subMenus.length"
        bordered
        collapse-mode="width"
        :collapsed="collapsed"
        :width="appStore.layout.siderWidth"
        :collapsed-width="appStore.layout.siderCollapsedWidth"
        :native-scrollbar="false"
        class="mix-layout__sider"
      >
        <n-scrollbar>
          <n-menu
            class="mix-layout__menu"
            :options="subMenus"
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
        </n-scrollbar>
      </n-layout-sider>

      <div class="mix-layout__main">
        <LayoutTabs v-if="appStore.layout.showTabs" />
        <LayoutContent />
        <LayoutFooter v-if="appStore.layout.showFooter" />
      </div>
    </n-layout>

    <LayoutSetting />
  </div>
</template>

<style scoped lang="scss">
.mix-layout {
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
    transition: width 0.2s var(--axis-bezier, ease);

    :deep(.layout-logo) {
      flex: 1;
      padding: 0 12px;
      border-bottom: none;
      border-right: 1px solid var(--axis-border-color);
    }
  }

  &__top-menu {
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

  &__top-tools {
    flex-shrink: 0;
    align-self: stretch;
    display: flex;
    align-items: center;

    :deep(.layout-header) {
      border-bottom: none;
      background: transparent;
    }
  }

  &__body {
    flex: 1;
    min-height: 0;
    overflow: hidden;
  }

  &__main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  &__menu {
    :deep(.n-menu-item.axis-menu-level-3 > .n-menu-item-content) {
      padding-left: 45px !important;
    }
  }
}
</style>
