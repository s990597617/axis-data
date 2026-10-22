<script setup lang="ts">
import { computed, h, nextTick, ref, watch } from 'vue'
import type { DropdownOption } from 'naive-ui'
import { NIcon } from 'naive-ui'

import { useLayoutTabs } from '@/layouts/composables/use-layout-tabs'
import { Icon } from '@/shared/icons'
import type { TabItem } from '@/store/modules/tabs.store'

/**
 * LayoutTabs
 *
 * @description
 * 标签页条，承载已打开页面的切换、关闭、右键菜单与刷新操作。
 * 数据与副作用全部由 `use-layout-tabs` 完成，本组件只负责视觉与交互。
 */
const {
  tabs,
  activeFullPath,
  activateTab,
  closeTab,
  closeOthers,
  closeLeft,
  closeRight,
  closeAll,
  refresh,
} = useLayoutTabs()

/** 右键菜单状态 */
const contextMenu = ref<{ show: boolean; x: number; y: number; target: TabItem | null }>({
  show: false,
  x: 0,
  y: 0,
  target: null,
})

/** 滚动容器引用，用于激活项自动滚动到视区 */
const scrollRef = ref<HTMLElement | null>(null)

const contextOptions = computed<DropdownOption[]>(() => {
  const target = contextMenu.value.target
  if (!target) return []
  const index = tabs.value.findIndex((t) => t.fullPath === target.fullPath)
  const isActive = target.fullPath === activeFullPath.value
  return [
    {
      key: 'refresh',
      label: '刷新',
      disabled: !isActive,
      icon: () =>
        h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:reload-outlined' }) }),
    },
    {
      key: 'close',
      label: '关闭',
      disabled: !!target.affix,
      icon: () => h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:close-outlined' }) }),
    },
    { type: 'divider', key: 'd1' },
    {
      key: 'closeLeft',
      label: '关闭左侧',
      disabled: index <= 0,
      icon: () =>
        h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:vertical-right-outlined' }) }),
    },
    {
      key: 'closeRight',
      label: '关闭右侧',
      disabled: index >= tabs.value.length - 1,
      icon: () =>
        h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:vertical-left-outlined' }) }),
    },
    {
      key: 'closeOthers',
      label: '关闭其他',
      icon: () =>
        h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:column-width-outlined' }) }),
    },
    {
      key: 'closeAll',
      label: '全部关闭',
      icon: () => h(NIcon, null, { default: () => h(Icon, { icon: 'ant-design:minus-outlined' }) }),
    },
  ]
})

function handleContextMenu(event: MouseEvent, tab: TabItem) {
  event.preventDefault()
  contextMenu.value = { show: true, x: event.clientX, y: event.clientY, target: tab }
}

function handleContextSelect(key: string) {
  const target = contextMenu.value.target
  contextMenu.value.show = false
  if (!target) return

  switch (key) {
    case 'refresh':
      refresh()
      break
    case 'close':
      closeTab(target.fullPath)
      break
    case 'closeLeft':
      closeLeft(target.fullPath)
      break
    case 'closeRight':
      closeRight(target.fullPath)
      break
    case 'closeOthers':
      closeOthers(target.fullPath)
      break
    case 'closeAll':
      closeAll()
      break
  }
}

function handleClickOutside() {
  contextMenu.value.show = false
}

/**
 * 激活项变化时滚动到视区
 */
watch(
  activeFullPath,
  async () => {
    await nextTick()
    const container = scrollRef.value
    if (!container) return
    const active = container.querySelector<HTMLElement>('.layout-tabs__tab.is-active')
    if (active) active.scrollIntoView({ behavior: 'smooth', inline: 'nearest', block: 'nearest' })
  },
  { immediate: true },
)
</script>

<template>
  <div class="layout-tabs">
    <div ref="scrollRef" class="layout-tabs__scroll">
      <div
        v-for="tab in tabs"
        :key="tab.fullPath"
        class="layout-tabs__tab"
        :class="{ 'is-active': tab.fullPath === activeFullPath, 'is-affix': tab.affix }"
        @click="activateTab(tab)"
        @contextmenu="handleContextMenu($event, tab)"
      >
        <Icon v-if="tab.icon" :icon="tab.icon" class="layout-tabs__icon" />
        <span class="layout-tabs__label">{{ tab.title }}</span>
        <Icon
          v-if="!tab.affix"
          icon="ant-design:close-outlined"
          class="layout-tabs__close"
          @click.stop="closeTab(tab.fullPath)"
        />
      </div>
    </div>

    <n-dropdown
      placement="bottom-start"
      trigger="manual"
      :x="contextMenu.x"
      :y="contextMenu.y"
      :options="contextOptions"
      :show="contextMenu.show"
      :on-clickoutside="handleClickOutside"
      @select="handleContextSelect"
    />
  </div>
</template>

<style scoped lang="scss">
.layout-tabs {
  height: var(--axis-tabs-height, 40px);
  padding: 0 12px;
  background: var(--axis-card-color);
  border-bottom: 1px solid var(--axis-border-color);
  flex-shrink: 0;
  display: flex;
  align-items: center;

  &__scroll {
    flex: 1;
    display: flex;
    align-items: center;
    gap: 6px;
    overflow-x: auto;
    scrollbar-width: none;

    &::-webkit-scrollbar {
      display: none;
    }
  }

  &__tab {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    height: 28px;
    padding: 0 10px;
    border-radius: var(--axis-border-radius, 6px);
    font-size: 13px;
    color: var(--axis-text-color-2);
    background: var(--axis-action-color);
    cursor: pointer;
    white-space: nowrap;
    user-select: none;
    transition: all 0.15s ease;
    flex-shrink: 0;

    &:hover {
      color: var(--axis-primary-color);
      background: var(--axis-primary-color-soft);
    }

    &.is-active {
      color: #fff;
      background: var(--axis-primary-color);

      .layout-tabs__close {
        color: rgba(255, 255, 255, 0.85);

        &:hover {
          color: #fff;
          background: rgba(0, 0, 0, 0.15);
        }
      }
    }
  }

  &__icon {
    font-size: 14px;
  }

  &__close {
    font-size: 12px;
    padding: 2px;
    border-radius: 50%;
    color: var(--axis-text-color-3);
    transition: all 0.15s ease;

    &:hover {
      color: #fff;
      background: rgba(0, 0, 0, 0.15);
    }
  }
}
</style>
