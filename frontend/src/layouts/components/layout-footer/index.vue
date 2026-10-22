<script setup lang="ts">
import { appConfig } from '@/config/app.config'

const year = new Date().getFullYear()
</script>

<template>
  <footer class="layout-footer">
    <span>© {{ year }} {{ appConfig.appName }}</span>
  </footer>
</template>

<style scoped lang="scss">
.layout-footer {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 32px;
  padding: 0 20px;
  font-size: 12px;
  color: var(--axis-text-color-3);
  border-top: 1px solid var(--axis-border-color);
  background: var(--axis-card-color);
  flex-shrink: 0;
}
</style>
