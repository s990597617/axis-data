<script setup lang="ts">
import { computed } from 'vue'
import type { SelectOption } from 'naive-ui'

import { pageAnimateOptions } from '@/config/layout.config'
import { Icon } from '@/shared/icons'
import type { LayoutMode, PageAnimateType } from '@/shared/types/app-layout'
import { useAppStore } from '@/store/modules/app.store'

const appStore = useAppStore()

const layoutModes: readonly { value: LayoutMode; label: string }[] = [
  { value: 'vertical', label: '垂直布局' },
  { value: 'horizontal', label: '水平布局' },
  { value: 'split', label: '分栏布局' },
  { value: 'mix', label: '混合布局' },
] as const

const animateSelectOptions = computed<SelectOption[]>(() =>
  pageAnimateOptions.map((option) => ({ label: option.label, value: option.value })),
)
</script>

<template>
  <div>
    <div class="pref-section-title">布局模式</div>
    <div class="pref-layout-grid">
      <div
        v-for="mode in layoutModes"
        :key="mode.value"
        class="pref-layout-thumb"
        :class="{ 'is-active': appStore.layout.mode === mode.value }"
        :title="mode.label"
        @click="appStore.setLayoutMode(mode.value)"
      >
        <div v-if="appStore.layout.mode === mode.value" class="pref-layout-thumb__badge">
          <Icon icon="ant-design:check-outlined" />
        </div>
        <div class="pref-layout-thumb__preview">
          <template v-if="mode.value === 'vertical'">
            <div class="thumb-aside" />
            <div class="thumb-main">
              <div class="thumb-header" />
              <div class="thumb-body" />
            </div>
          </template>
          <template v-else-if="mode.value === 'horizontal'">
            <div class="thumb-full">
              <div class="thumb-topbar" />
              <div class="thumb-body" />
            </div>
          </template>
          <template v-else-if="mode.value === 'split'">
            <div class="thumb-aside thumb-aside--narrow" />
            <div class="thumb-aside thumb-aside--second" />
            <div class="thumb-main">
              <div class="thumb-header" />
              <div class="thumb-body" />
            </div>
          </template>
          <template v-else>
            <div class="thumb-aside thumb-aside--narrow" />
            <div class="thumb-main">
              <div class="thumb-topbar" />
              <div class="thumb-body" />
            </div>
          </template>
        </div>
        <div class="pref-layout-thumb__label">{{ mode.label }}</div>
      </div>
    </div>

    <div class="pref-section-title pref-section-title--mt">侧边栏</div>
    <div class="pref-row">
      <span>显示侧边栏</span>
      <n-switch
        :value="appStore.layout.showSidebar"
        @update:value="(value: boolean) => appStore.updateLayout({ showSidebar: value })"
      />
    </div>
    <div class="pref-row">
      <span>显示 Logo</span>
      <n-switch
        :value="appStore.layout.showLogo"
        @update:value="(value: boolean) => appStore.updateLayout({ showLogo: value })"
      />
    </div>
    <div class="pref-row">
      <span>折叠侧边栏</span>
      <n-switch
        :value="appStore.layout.collapsed"
        @update:value="(value: boolean) => appStore.setCollapsed(value)"
      />
    </div>
    <div class="pref-row">
      <span>手风琴菜单</span>
      <n-switch
        :value="appStore.layout.accordionMenu"
        @update:value="(value: boolean) => appStore.updateLayout({ accordionMenu: value })"
      />
    </div>
    <div class="pref-row">
      <span>侧边栏宽度</span>
      <n-input-number
        :value="appStore.layout.siderWidth"
        :min="160"
        :max="320"
        :step="4"
        size="small"
        style="width: 100px"
        @update:value="(value: number | null) => value && appStore.updateLayout({ siderWidth: value })"
      />
    </div>
    <div class="pref-row">
      <span>折叠宽度</span>
      <n-input-number
        :value="appStore.layout.siderCollapsedWidth"
        :min="48"
        :max="120"
        :step="4"
        size="small"
        style="width: 100px"
        @update:value="
          (value: number | null) => value && appStore.updateLayout({ siderCollapsedWidth: value })
        "
      />
    </div>

    <div class="pref-section-title pref-section-title--mt">顶部栏</div>
    <div class="pref-row">
      <span>显示顶部栏</span>
      <n-switch
        :value="appStore.layout.showHeader"
        @update:value="(value: boolean) => appStore.updateLayout({ showHeader: value })"
      />
    </div>
    <div class="pref-row">
      <span>固定顶部栏</span>
      <n-switch
        :value="appStore.layout.fixedHeader"
        @update:value="(value: boolean) => appStore.updateLayout({ fixedHeader: value })"
      />
    </div>
    <div class="pref-row">
      <span>顶部栏高度</span>
      <n-input-number
        :value="appStore.layout.headerHeight"
        :min="48"
        :max="80"
        :step="4"
        size="small"
        style="width: 100px"
        @update:value="(value: number | null) => value && appStore.updateLayout({ headerHeight: value })"
      />
    </div>
    <div class="pref-row">
      <span>显示面包屑</span>
      <n-switch
        :value="appStore.layout.showBreadcrumb"
        @update:value="(value: boolean) => appStore.updateLayout({ showBreadcrumb: value })"
      />
    </div>

    <div class="pref-section-title pref-section-title--mt">标签栏</div>
    <div class="pref-row">
      <span>显示标签栏</span>
      <n-switch
        :value="appStore.layout.showTabs"
        @update:value="(value: boolean) => appStore.updateLayout({ showTabs: value })"
      />
    </div>
    <div class="pref-row">
      <span>标签栏高度</span>
      <n-input-number
        :value="appStore.layout.tabsHeight"
        :min="28"
        :max="48"
        :step="2"
        size="small"
        style="width: 100px"
        @update:value="(value: number | null) => value && appStore.updateLayout({ tabsHeight: value })"
      />
    </div>

    <div class="pref-section-title pref-section-title--mt">页面</div>
    <div class="pref-row">
      <span>启用动画</span>
      <n-switch
        :value="appStore.layout.pageAnimate"
        @update:value="(value: boolean) => appStore.updateLayout({ pageAnimate: value })"
      />
    </div>
    <div class="pref-row">
      <span>动画类型</span>
      <n-select
        :value="appStore.layout.pageAnimateType"
        :options="animateSelectOptions"
        :disabled="!appStore.layout.pageAnimate"
        size="small"
        style="width: 110px"
        @update:value="(value: PageAnimateType) => appStore.updateLayout({ pageAnimateType: value })"
      />
    </div>
    <div class="pref-row">
      <span>显示页脚</span>
      <n-switch
        :value="appStore.layout.showFooter"
        @update:value="(value: boolean) => appStore.updateLayout({ showFooter: value })"
      />
    </div>
    <div class="pref-row">
      <span>全局水印</span>
      <n-switch
        :value="appStore.layout.showWatermark"
        @update:value="(value: boolean) => appStore.updateLayout({ showWatermark: value })"
      />
    </div>
    <div v-if="appStore.layout.showWatermark" class="pref-row pref-row--block">
      <span>水印文案</span>
      <n-input
        :value="appStore.layout.watermarkContent"
        size="small"
        clearable
        placeholder="默认：姓名 · 机构"
        @update:value="(value: string) => appStore.updateLayout({ watermarkContent: value })"
      />
    </div>
  </div>
</template>
