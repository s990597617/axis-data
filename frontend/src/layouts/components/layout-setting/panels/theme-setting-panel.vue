<script setup lang="ts">
import { computed } from 'vue'

import { BORDER_RADIUS_OPTIONS } from '@/config/theme.config'
import { Icon } from '@/shared/icons'
import type { SurfaceTheme, ThemeMode } from '@/shared/types/app-theme'
import { useThemeStore } from '@/store/modules/theme.store'

const themeStore = useThemeStore()

const themeModes: readonly { value: ThemeMode; label: string; icon: string }[] = [
  { value: 'light', label: '浅色', icon: 'ant-design:sun-outlined' },
  { value: 'dark', label: '深色', icon: 'ant-design:moon-outlined' },
  { value: 'auto', label: '跟随系统', icon: 'ant-design:desktop-outlined' },
] as const

const activeThemeIndex = computed(() =>
  Math.max(
    themeModes.findIndex((mode) => mode.value === themeStore.themeMode),
    0,
  ),
)

const themeIndicatorStyle = computed(() => {
  const count = themeModes.length
  const gaps = (count - 1) * 4
  return {
    width: `calc((100% - ${8 + gaps}px) / ${count})`,
    transform: `translateX(calc(${activeThemeIndex.value} * (100% + 4px)))`,
  }
})

const surfaceOptions: readonly { label: string; value: SurfaceTheme }[] = [
  { label: '浅色', value: 'light' },
  { label: '深色', value: 'dark' },
] as const

const resolvedSidebarColor = computed(
  () =>
    themeStore.config.sidebarColor ||
    (themeStore.config.sidebarTheme === 'dark' ? '#1b2029' : '#ffffff'),
)

const resolvedTopbarColor = computed(
  () =>
    themeStore.config.topbarColor ||
    (themeStore.config.topbarTheme === 'dark' ? '#1b2029' : '#ffffff'),
)

function handleThemeModeChange(mode: ThemeMode) {
  themeStore.setThemeMode(mode)
  const resolved: SurfaceTheme = mode === 'auto' ? (themeStore.systemDark ? 'dark' : 'light') : mode
  themeStore.update({
    sidebarTheme: resolved,
    sidebarColor: '',
    topbarTheme: resolved,
    topbarColor: '',
  })
}

function updateSidebarColor(event: Event) {
  themeStore.update({ sidebarColor: (event.target as HTMLInputElement).value })
}

function updateTopbarColor(event: Event) {
  themeStore.update({ topbarColor: (event.target as HTMLInputElement).value })
}
</script>

<template>
  <div>
    <div class="pref-section-title">主题模式</div>
    <div class="pref-segmented">
      <div class="pref-segmented__indicator" :style="themeIndicatorStyle" />
      <div
        v-for="mode in themeModes"
        :key="mode.value"
        class="pref-segmented__item"
        :class="{ 'is-active': themeStore.themeMode === mode.value }"
        :title="mode.label"
        @click="handleThemeModeChange(mode.value)"
      >
        <Icon :icon="mode.icon" />
      </div>
    </div>

    <div class="pref-section-title pref-section-title--mt">布局主题</div>
    <div class="pref-row">
      <span>侧边栏主题</span>
      <n-select
        :value="themeStore.config.sidebarTheme"
        :options="[...surfaceOptions]"
        size="small"
        style="width: 100px"
        @update:value="(value: SurfaceTheme) => themeStore.update({ sidebarTheme: value })"
      />
    </div>
    <div class="pref-row pref-row--color">
      <span>侧边栏颜色</span>
      <div class="pref-color-control">
        <label class="pref-color-trigger" :style="{ backgroundColor: resolvedSidebarColor }">
          <input type="color" :value="resolvedSidebarColor" @input="updateSidebarColor" />
        </label>
        <button
          class="pref-color-reset"
          type="button"
          title="恢复默认"
          @click="themeStore.update({ sidebarColor: '' })"
        >
          <Icon icon="ant-design:reload-outlined" />
        </button>
      </div>
    </div>

    <div class="pref-row">
      <span>顶部栏主题</span>
      <n-select
        :value="themeStore.config.topbarTheme"
        :options="[...surfaceOptions]"
        size="small"
        style="width: 100px"
        @update:value="(value: SurfaceTheme) => themeStore.update({ topbarTheme: value })"
      />
    </div>
    <div class="pref-row pref-row--color">
      <span>顶部栏颜色</span>
      <div class="pref-color-control">
        <label class="pref-color-trigger" :style="{ backgroundColor: resolvedTopbarColor }">
          <input type="color" :value="resolvedTopbarColor" @input="updateTopbarColor" />
        </label>
        <button
          class="pref-color-reset"
          type="button"
          title="恢复默认"
          @click="themeStore.update({ topbarColor: '' })"
        >
          <Icon icon="ant-design:reload-outlined" />
        </button>
      </div>
    </div>

    <div class="pref-row">
      <span>色弱模式</span>
      <n-switch
        :value="themeStore.config.colorWeak"
        @update:value="(value: boolean) => themeStore.update({ colorWeak: value })"
      />
    </div>
    <div class="pref-row">
      <span>灰色模式</span>
      <n-switch
        :value="themeStore.config.grayMode"
        @update:value="(value: boolean) => themeStore.update({ grayMode: value })"
      />
    </div>

    <div class="pref-section-title pref-section-title--mt">圆角</div>
    <div class="pref-radius-group">
      <div
        v-for="radius in BORDER_RADIUS_OPTIONS"
        :key="radius"
        class="pref-radius-item"
        :class="{ 'is-active': themeStore.borderRadius === radius }"
        @click="themeStore.setBorderRadius(radius)"
      >
        {{ radius }}
      </div>
    </div>
  </div>
</template>
