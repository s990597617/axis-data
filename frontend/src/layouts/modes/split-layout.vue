<script setup lang="ts">
import { computed } from 'vue'

import LayoutContent from '@/layouts/components/layout-content/index.vue'
import LayoutFooter from '@/layouts/components/layout-footer/index.vue'
import LayoutHeader from '@/layouts/components/layout-header/index.vue'
import LayoutSetting from '@/layouts/components/layout-setting/index.vue'
import LayoutTabs from '@/layouts/components/layout-tabs/index.vue'
import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'
import { layoutMenuNodeProps, useLayoutMenu } from '@/layouts/composables/use-layout-menu'
import { Icon } from '@/shared/icons'
import logoUrl from '@/assets/images/logo.png'
import { useAppStore } from '@/store/modules/app.store'

/**
 * SplitLayout
 *
 * @description
 * 分栏布局：最左侧为一级菜单图标栏，中间为当前一级菜单下的二级菜单树，右侧为主内容区。
 * 适合导航切换频繁、一二级菜单分工明显的中大型平台。
 */
const appStore = useAppStore()
const { collapsed, toggle: toggleCollapse } = useLayoutCollapse()
const {
  rootMenuItems,
  activeRootKey,
  subMenus,
  expandKeys,
  updateExpandKeys,
  currentMenu,
  updateValue,
  setActiveRoot,
} = useLayoutMenu()

const activeRoot = computed(() => rootMenuItems.value.find((m) => m.key === activeRootKey.value))
</script>

<template>
  <n-layout has-sider class="split-layout">
    <!-- 一级图标栏 -->
    <aside class="split-layout__rail">
      <div class="split-layout__rail-logo">
        <img :src="logoUrl" alt="logo" />
      </div>
      <nav class="split-layout__rail-list">
        <button
          v-for="item in rootMenuItems"
          :key="item.key"
          type="button"
          class="split-layout__rail-item"
          :class="{ 'is-active': activeRootKey === item.key }"
          :title="item.label"
          @click="setActiveRoot(item.key)"
        >
          <Icon :icon="item.icon ?? 'ant-design:folder-outlined'" />
          <span class="split-layout__rail-label">{{ item.label }}</span>
        </button>
      </nav>
    </aside>

    <!-- 二级菜单栏：不用 n-layout-sider，避免其滚动容器导致标题视觉偏右 -->
    <aside
      v-if="subMenus.length"
      class="split-layout__sub"
      :class="{ 'is-collapsed': collapsed }"
      :style="{
        width: `${collapsed ? appStore.layout.siderCollapsedWidth : appStore.layout.siderWidth}px`
      }"
    >
      <div class="split-layout__sub-title">
        <span class="split-layout__sub-title-inner">
          <Icon
            v-if="activeRoot?.icon"
            :icon="activeRoot.icon"
            class="split-layout__sub-icon"
          />
          <span v-if="!collapsed" class="split-layout__sub-text">{{ activeRoot?.label }}</span>
        </span>
      </div>
      <n-scrollbar class="split-layout__sub-scroll">
        <n-menu
          class="split-layout__menu"
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
    </aside>

    <!-- 主内容 -->
    <div class="split-layout__main">
      <LayoutHeader />
      <LayoutTabs v-if="appStore.layout.showTabs" />
      <LayoutContent />
      <LayoutFooter v-if="appStore.layout.showFooter" />
    </div>

    <LayoutSetting />
  </n-layout>
</template>

<style scoped lang="scss">
.split-layout {
  height: 100vh;
  overflow: hidden;

  :deep(.n-layout-scroll-container) {
    overflow: hidden !important;
  }

  &__rail {
    width: 72px;
    flex-shrink: 0;
    background: var(--axis-rail-bg);
    color: #fff;
    display: flex;
    flex-direction: column;
    align-items: stretch;
  }

  &__rail-logo {
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-bottom: 1px solid rgba(255, 255, 255, 0.06);

    img {
      width: 28px;
      height: 28px;
      object-fit: contain;
    }
  }

  &__rail-list {
    display: flex;
    flex-direction: column;
    padding: 8px 8px;
    gap: 6px;
  }

  &__rail-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 10px 4px;
    border: none;
    border-radius: var(--axis-border-radius, 6px);
    background: transparent;
    color: rgba(255, 255, 255, 0.7);
    font-size: 11px;
    cursor: pointer;
    transition: all 0.2s ease;

    svg,
    :deep(svg) {
      font-size: 20px;
    }

    &:hover {
      color: #fff;
      background: rgba(255, 255, 255, 0.1);
    }

    &.is-active {
      color: #fff;
      background: var(--axis-primary-color);
    }
  }

  &__rail-label {
    max-width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    font-size: 11px;
    line-height: 1.2;
  }

  &__sub {
    display: flex;
    flex-direction: column;
    flex-shrink: 0;
    height: 100%;
    min-height: 0;
    background: var(--axis-sider-bg, var(--axis-card-color));
    border-right: 1px solid var(--axis-border-color);
    box-sizing: border-box;
    transition: width 0.2s var(--axis-bezier, ease);
    overflow: hidden;
  }

  &__sub-scroll {
    flex: 1;
    min-height: 0;
  }

  &__sub-title {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 56px;
    padding: 0;
    border-bottom: 1px solid var(--axis-border-color);
    color: var(--axis-text-color-1);
    font-size: 15px;
    font-weight: 600;
    flex-shrink: 0;
    box-sizing: border-box;
  }

  &__sub-title-inner {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    max-width: calc(100% - 24px);
    overflow: hidden;
  }

  &__sub-icon {
    font-size: 18px;
    flex-shrink: 0;
    color: var(--axis-primary-color);
  }

  &__sub-text {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  &__menu {
    :deep(.n-menu-item.axis-menu-level-3 > .n-menu-item-content) {
      padding-left: 45px !important;
    }
  }

  &__main {
    flex: 1;
    min-width: 0;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }
}
</style>
