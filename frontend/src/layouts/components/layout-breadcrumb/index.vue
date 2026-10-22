<script setup lang="ts">
import { useRouter } from 'vue-router'

import { useLayoutBreadcrumb } from '@/layouts/composables/use-layout-breadcrumb'
import { Icon } from '@/shared/icons'

/**
 * LayoutBreadcrumb
 *
 * @description
 * 基于当前路由链生成面包屑。
 * 仅消费 `use-layout-breadcrumb` 输出，点击跳转由本组件封装。
 */
const router = useRouter()
const { items } = useLayoutBreadcrumb()

function handleClick(path: string | undefined) {
  if (path) router.push(path)
}
</script>

<template>
  <nav v-if="items.length" class="layout-breadcrumb">
    <n-breadcrumb separator="/">
      <n-breadcrumb-item
        v-for="item in items"
        :key="item.name"
        :clickable="!item.active && !!item.path"
        @click="handleClick(item.path)"
      >
        <span class="layout-breadcrumb__item" :class="{ 'is-active': item.active }">
          <Icon v-if="item.icon" :icon="item.icon" class="layout-breadcrumb__icon" />
          {{ item.title }}
        </span>
      </n-breadcrumb-item>
    </n-breadcrumb>
  </nav>
</template>

<style scoped lang="scss">
.layout-breadcrumb {
  display: inline-flex;
  align-items: center;
  min-width: 0;
  line-height: 1;

  :deep(.n-breadcrumb) {
    display: inline-flex;
    align-items: center;
    line-height: 1;
  }

  :deep(.n-breadcrumb > ul) {
    display: inline-flex;
    align-items: center;
    margin: 0;
    padding: 0;
    list-style: none;
  }

  :deep(.n-breadcrumb-item) {
    display: inline-flex;
    align-items: center;
  }

  :deep(.n-breadcrumb-item__link),
  :deep(.n-breadcrumb-item__separator) {
    display: inline-flex;
    align-items: center;
    line-height: 1;
  }

  &__item {
    display: inline-flex;
    align-items: center;
    gap: 4px;
    font-size: 13px;

    &.is-active {
      color: var(--axis-text-color-1);
      font-weight: 500;
    }
  }

  &__icon {
    font-size: 14px;
  }
}
</style>
