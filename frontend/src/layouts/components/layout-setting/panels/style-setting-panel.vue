<script setup lang="ts">
import { presetPrimaryColors } from '@/config/theme.config'
import { Icon } from '@/shared/icons'
import { useThemeStore } from '@/store/modules/theme.store'

const themeStore = useThemeStore()

function updatePrimaryColor(event: Event) {
  themeStore.setPrimaryColor((event.target as HTMLInputElement).value)
}
</script>

<template>
  <div>
    <div class="pref-section-title">主色</div>
    <div class="pref-colors">
      <button
        v-for="color in presetPrimaryColors"
        :key="color.value"
        type="button"
        class="pref-color"
        :class="{
          'is-active': themeStore.primaryColor.toLowerCase() === color.value.toLowerCase(),
        }"
        :title="color.label"
        :style="{ backgroundColor: color.value }"
        @click="themeStore.setPrimaryColor(color.value)"
      >
        <Icon
          v-if="themeStore.primaryColor.toLowerCase() === color.value.toLowerCase()"
          icon="ant-design:check-outlined"
        />
      </button>
    </div>

    <div class="pref-row pref-row--color pref-row--mt">
      <span>自定义主色</span>
      <div class="pref-color-control">
        <label class="pref-color-trigger" :style="{ backgroundColor: themeStore.primaryColor }">
          <input type="color" :value="themeStore.primaryColor" @input="updatePrimaryColor" />
        </label>
        <span class="pref-color-value">{{ themeStore.primaryColor.toUpperCase() }}</span>
      </div>
    </div>

    <div class="pref-section-title pref-section-title--mt">状态预览</div>
    <div class="pref-preview">
      <div
        v-for="stage in ['pressed', 'default', 'hover', 'suppl']"
        :key="stage"
        class="pref-preview__chip"
        :style="{
          background: `var(--axis-primary-color${stage === 'default' ? '' : `-${stage}`})`,
        }"
      >
        {{ stage }}
      </div>
    </div>
  </div>
</template>
