<script setup lang="ts">
import { useRouter } from 'vue-router'

import logoUrl from '@/assets/images/logo.png'
import { appConfig } from '@/config/app.config'
import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'

/**
 * LayoutLogo
 *
 * @description
 * 应用 Logo 展示组件，负责折叠态下自动隐藏应用名。
 * 点击后回到应用首页，不承载任何业务逻辑。
 */
const router = useRouter()
const { collapsed } = useLayoutCollapse()

function handleClick() {
  router.push('/')
}
</script>

<template>
  <div class="layout-logo" :class="{ 'layout-logo--collapsed': collapsed }" @click="handleClick">
    <img :src="logoUrl" alt="logo" class="layout-logo__img" />
    <span v-if="!collapsed" class="layout-logo__title">{{ appConfig.appName }}</span>
  </div>
</template>

<style scoped lang="scss">
.layout-logo {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  height: 56px;
  padding: 0 16px;
  border-bottom: 1px solid var(--axis-border-color);
  cursor: pointer;
  user-select: none;
  overflow: hidden;

  &--collapsed {
    padding: 0;
  }

  &__img {
    width: 28px;
    height: 28px;
    object-fit: contain;
    flex-shrink: 0;
  }

  &__title {
    font-size: 15px;
    font-weight: 600;
    color: var(--axis-text-color-1);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }
}
</style>
