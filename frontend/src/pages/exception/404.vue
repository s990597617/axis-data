<script setup lang="ts">
defineOptions({ name: 'Exception404' })
</script>

<template>
  <div class="flex h-full flex-col items-center mt-20">
    <n-result status="404" title="404 资源不存在" size="huge">
      <template #footer>
        <n-button @click="$router.push('/')">返回首页</n-button>
      </template>
    </n-result>
  </div>
</template>
