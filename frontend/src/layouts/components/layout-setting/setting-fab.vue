<script setup lang="ts">
import { Icon } from '@/shared/icons'

defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  open: []
}>()
</script>

<template>
  <Transition name="fab">
    <button v-if="!visible" type="button" class="setting-fab" title="偏好设置" @click="emit('open')">
      <Icon icon="ant-design:setting-outlined" class="setting-fab__icon" />
    </button>
  </Transition>
</template>
