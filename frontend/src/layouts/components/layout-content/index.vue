<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref } from 'vue'

import { useLayoutReload } from '@/layouts/composables/use-layout-reload'
import { useLayoutTabs } from '@/layouts/composables/use-layout-tabs'
import { useAppStore } from '@/store/modules/app.store'

/**
 * LayoutContent
 *
 * @description
 * 主内容区容器，承载 `router-view` 与 `keep-alive` 缓存容器，并提供刷新当前页的能力。
 * 布局层只负责"承载"，是否缓存由路由 meta 与 `tabs.store` 决定。
 */
const appStore = useAppStore()
const { cachedNames } = useLayoutTabs()

/** 页面过渡名，由布局配置决定；禁用时置空以完全关闭过渡 */
const transitionName = computed(() => {
  if (!appStore.layout.pageAnimate) return ''
  const type = appStore.layout.pageAnimateType
  return type === 'none' ? '' : `page-${type}`
})

/** 通过切换该值强制重建 `<router-view>`，实现刷新 */
const reloadKey = ref(0)

/**
 * 刷新当前页（软刷新）
 *
 * @description
 * 触发 `<router-view>` 重建，配合 keep-alive 的 `exclude` 临时排除当前组件名，保证缓存能被重置。
 */
async function reload(): Promise<void> {
  reloadKey.value += 1
  await nextTick()
}

const { registerReload } = useLayoutReload()
registerReload(reload)
onBeforeUnmount(() => registerReload(null))
</script>

<template>
  <section class="layout-content">
    <!-- 非固定头部模式下外部会把 header / tabs 放到该 slot，与主内容一起滚动 -->
    <slot name="before" />
    <div class="layout-content__body">
      <router-view v-slot="{ Component, route }">
        <transition :name="transitionName" mode="out-in">
          <keep-alive :include="cachedNames">
            <component :is="Component" v-if="Component" :key="`${route.fullPath}-${reloadKey}`" />
          </keep-alive>
        </transition>
      </router-view>
    </div>
    <!-- footer 插槽 -->
    <slot name="after" />
  </section>
</template>

<style scoped lang="scss">
.layout-content {
  flex: 1;
  min-height: 0; /* 打破 flex 子项默认 min-height:auto，允许内容收缩并触发内部滚动 */
  overflow: hidden;
  background: var(--axis-bg-color);
  display: flex;
  flex-direction: column;

  &__body {
    box-sizing: border-box;
    display: flex;
    flex: 1 1 0;
    min-height: 0;
    flex-direction: column;
    overflow: auto;
    padding: 8px;

    :deep(> *) {
      min-height: 0;
    }
  }
}

/* 淡入滑动（默认） */
.page-fade-slide-enter-active,
.page-fade-slide-leave-active {
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}
.page-fade-slide-enter-from {
  opacity: 0;
  transform: translateY(6px);
}
.page-fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-6px);
}

/* 仅淡入 */
.page-fade-enter-active,
.page-fade-leave-active {
  transition: opacity 0.2s ease;
}
.page-fade-enter-from,
.page-fade-leave-to {
  opacity: 0;
}

/* 左滑 */
.page-slide-left-enter-active,
.page-slide-left-leave-active {
  transition:
    opacity 0.24s ease,
    transform 0.24s ease;
}
.page-slide-left-enter-from {
  opacity: 0;
  transform: translateX(16px);
}
.page-slide-left-leave-to {
  opacity: 0;
  transform: translateX(-16px);
}
</style>
