<script setup lang="ts">
import { computed, ref } from 'vue'

import SettingActions from './setting-actions.vue'
import SettingFab from './setting-fab.vue'
import SettingTabs from './setting-tabs.vue'
import LayoutSettingPanel from './panels/layout-setting-panel.vue'
import StyleSettingPanel from './panels/style-setting-panel.vue'
import ThemeSettingPanel from './panels/theme-setting-panel.vue'
import { useLayoutSetting } from '@/layouts/composables/use-layout-setting'

type TabKey = 'theme' | 'layout' | 'style'

const { visible, open, close } = useLayoutSetting()
const activeTab = ref<TabKey>('layout')

const tabs: readonly { key: TabKey; label: string }[] = [
  { key: 'theme', label: '主题设置' },
  { key: 'layout', label: '框架布局' },
  { key: 'style', label: '主题风格' },
] as const

const activePanel = computed(() => {
  const panelMap = {
    theme: ThemeSettingPanel,
    layout: LayoutSettingPanel,
    style: StyleSettingPanel,
  }
  return panelMap[activeTab.value]
})
</script>

<template>
  <SettingFab :visible="visible" @open="open" />

  <n-drawer :show="visible" :width="340" placement="right" :on-update:show="close">
    <n-drawer-content closable :native-scrollbar="false" :header-style="{ padding: '16px 20px' }">
      <template #header>
        <span class="pref-title">偏好配置</span>
      </template>

      <div class="pref-body">
        <SettingTabs v-model:active-key="activeTab" :tabs="tabs" />

        <div class="pref-panel-wrap">
          <transition name="pref-panel" mode="out-in">
            <component :is="activePanel" :key="activeTab" class="pref-panel" />
          </transition>
        </div>
      </div>

      <template #footer>
        <SettingActions />
      </template>
    </n-drawer-content>
  </n-drawer>
</template>

<style lang="scss" src="./styles.scss"></style>
