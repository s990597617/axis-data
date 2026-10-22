<script setup lang="ts">
defineOptions({ name: 'Exception403' })
</script>

<template>
  <div class="flex h-full flex-col items-center mt-20">
    <n-result status="403" title="403 无权限" description="当前账号没有访问该页面的权限。">
      <template #footer>
        <n-button @click="$router.push('/')">返回首页</n-button>
      </template>
    </n-result>
  </div>
</template>
