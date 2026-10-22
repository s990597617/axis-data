<script setup lang="ts">
  import LayoutBreadcrumb from '@/layouts/components/layout-breadcrumb/index.vue'
  import LayoutSearch from '@/layouts/components/layout-search/index.vue'
  import LayoutHeaderNotify from '@/layouts/components/layout-header/layout-header-notify.vue'
  import LayoutHeaderUserPanel from '@/layouts/components/layout-header/layout-header-user-panel.vue'
  import { useLayoutHeaderActions } from '@/layouts/composables/use-layout-header-actions'
  import { Icon } from '@/shared/icons'

  interface Props {
    showCollapse?: boolean
    showBreadcrumb?: boolean
  }

  const props = withDefaults(defineProps<Props>(), {
    showCollapse: true,
    showBreadcrumb: true
  })

  const {
    appStore,
    avatarSrc,
    collapsed,
    handleLogout,
    isFullscreen,
    nickname,
    refresh,
    searchVisible,
    themeIcon,
    themeTitle,
    toggle,
    toggleFullscreen,
    toggleTheme,
    userEmail,
    userRole,
    userStore
  } = useLayoutHeaderActions()
</script>

<template>
  <header class="layout-header">
    <div class="layout-header__left">
      <button
        v-if="showCollapse"
        type="button"
        class="layout-header__icon-btn"
        :title="collapsed ? '展开菜单' : '收起菜单'"
        @click="toggle"
      >
        <Icon
          :icon="collapsed ? 'ant-design:menu-unfold-outlined' : 'ant-design:menu-fold-outlined'"
        />
      </button>
      <button type="button" class="layout-header__icon-btn" title="刷新" @click="refresh">
        <Icon icon="ant-design:reload-outlined" />
      </button>
      <LayoutBreadcrumb
        v-if="props.showBreadcrumb && appStore.layout.showBreadcrumb"
        class="layout-header__breadcrumb"
      />
    </div>

    <div class="layout-header__right">
      <button
        type="button"
        class="layout-header__search-btn"
        title="搜索"
        @click="searchVisible = true"
      >
        <Icon icon="ant-design:search-outlined" />
        <span class="layout-header__search-text">搜索</span>
        <kbd class="layout-header__search-kbd">Ctrl K</kbd>
      </button>

      <button
        type="button"
        class="layout-header__icon-btn"
        :title="themeTitle"
        @click="toggleTheme"
      >
        <Icon :icon="themeIcon" />
      </button>

      <button
        type="button"
        class="layout-header__icon-btn"
        :title="isFullscreen ? '退出全屏' : '全屏'"
        @click="toggleFullscreen"
      >
        <Icon
          :icon="
            isFullscreen ? 'ant-design:fullscreen-exit-outlined' : 'ant-design:fullscreen-outlined'
          "
        />
      </button>

      <LayoutHeaderNotify />

      <span class="layout-header__divider" />

      <LayoutHeaderUserPanel
        :avatar-src="avatarSrc"
        :nickname="nickname"
        :role="userRole"
        :account="userEmail || userStore.userInfo?.username || ''"
        @logout="handleLogout"
      />
    </div>

    <LayoutSearch v-model:visible="searchVisible" />
  </header>
</template>

<style scoped lang="scss">
  .layout-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    height: var(--axis-header-height, 56px);
    padding: 0 20px;
    background: var(--axis-header-bg);
    color: var(--axis-header-text);
    border-bottom: 1px solid var(--axis-header-border);
    flex-shrink: 0;

    &__left,
    &__right {
      display: flex;
      align-items: center;
      gap: 6px;
    }

    &__icon-btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 34px;
      height: 34px;
      border: none;
      border-radius: 8px;
      background: transparent;
      color: var(--axis-header-text-2);
      font-size: 18px;
      cursor: pointer;
      transition: all 0.15s ease;

      &:hover {
        background: var(--axis-header-action);
        color: var(--axis-primary-color);
      }
    }

    &__divider {
      width: 1px;
      height: 16px;
      background: var(--axis-border-color);
      flex-shrink: 0;
    }

    &__search-btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      height: 32px;
      padding: 0 10px;
      border: 1px solid var(--axis-border-color);
      border-radius: 8px;
      background: var(--axis-action-color);
      color: var(--axis-text-color-3);
      font-size: 14px;
      cursor: pointer;
      transition: all 0.15s ease;
      white-space: nowrap;

      &:hover {
        border-color: var(--axis-primary-color);
        color: var(--axis-primary-color);
      }
    }

    &__search-text {
      font-size: 13px;
    }

    &__search-kbd {
      font-size: 11px;
      font-family: inherit;
      padding: 1px 5px;
      border-radius: 4px;
      border: 1px solid var(--axis-border-color);
      background: var(--axis-card-color);
      color: var(--axis-text-color-3);
      line-height: 1.4;
    }

    &__breadcrumb {
      height: 36px;
      display: inline-flex;
      align-items: center;
      margin-left: 4px;
    }
  }
</style>
