<script setup lang="ts">
  import { NEmpty, NScrollbar, NSpin } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import type { FileModuleOption } from '../composables/useFilePage'

  defineOptions({ name: 'FileModulePanel' })

  defineProps<{
    loading: boolean
    options: FileModuleOption[]
    activeKey: string
  }>()

  const emit = defineEmits<{
    select: [key: string]
  }>()
</script>

<template>
  <aside class="file-module-panel">
    <header class="file-module-panel__header">
      <div class="file-module-panel__heading">
        <span class="file-module-panel__icon-wrap">
          <Icon icon="ant-design:cloud-server-outlined" />
        </span>
        <div class="file-module-panel__title-block">
          <h2 class="file-module-panel__title">文件分类</h2>
          <p class="file-module-panel__desc">按业务模块浏览与归档</p>
        </div>
      </div>
    </header>

    <div class="file-module-panel__body">
      <NSpin :show="loading" class="file-module-panel__spin">
        <NScrollbar class="file-module-panel__scroll">
          <div v-if="options.length" class="file-module-panel__list">
            <button
              v-for="item in options"
              :key="item.key"
              type="button"
              class="file-module-item"
              :class="{ 'is-active': item.key === activeKey }"
              @click="emit('select', item.key)"
            >
              <span class="file-module-item__icon">
                <Icon :icon="item.icon" />
              </span>
              <span class="file-module-item__label" :title="item.label">{{ item.label }}</span>
              <span class="file-module-item__count">{{ item.count }}</span>
            </button>
          </div>
          <NEmpty v-else description="暂无分类" size="small" />
        </NScrollbar>
      </NSpin>
    </div>
  </aside>
</template>
