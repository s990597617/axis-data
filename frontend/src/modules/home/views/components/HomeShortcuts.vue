<script setup lang="ts">
  import { useRouter } from 'vue-router'

  import { Icon } from '@/shared/icons'

  import type { HomeShortcut } from '../composables/useHomePage'

  defineOptions({ name: 'HomeShortcuts' })

  defineProps<{
    shortcuts: HomeShortcut[]
  }>()

  const router = useRouter()

  function goTo(path: string) {
    router.push(path)
  }
</script>

<template>
  <section class="home-panel home-panel--shortcuts">
    <header class="home-panel__header">
      <div class="home-panel__header-main">
        <span class="home-panel__icon">
          <Icon icon="ant-design:appstore-outlined" />
        </span>
        <div class="home-panel__heading">
          <span class="home-panel__eyebrow">QUICK ACCESS</span>
          <div class="home-panel__title">常用功能</div>
          <span class="home-panel__subtitle">你的高频操作，触手可及</span>
        </div>
      </div>
      <span v-if="shortcuts.length" class="home-panel__badge">{{ shortcuts.length }} 项</span>
    </header>

    <div v-if="shortcuts.length" class="home-shortcuts">
      <button
        v-for="(item, index) in shortcuts"
        :key="item.path"
        type="button"
        class="home-shortcut"
        :style="{ '--item-delay': `${0.04 + index * 0.03}s` }"
        @click="goTo(item.path)"
      >
        <span class="home-shortcut__icon">
          <Icon :icon="item.icon" />
        </span>
        <span class="home-shortcut__body">
          <strong class="home-shortcut__label">{{ item.label }}</strong>
          <span class="home-shortcut__desc">{{ item.desc }}</span>
        </span>
        <span class="home-shortcut__arrow">
          <Icon icon="ant-design:arrow-up-outlined" />
        </span>
      </button>
    </div>

    <div v-else class="home-panel__empty">暂无可访问的快捷入口</div>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/shortcuts.scss';
</style>
