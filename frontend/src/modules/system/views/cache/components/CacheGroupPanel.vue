<script setup lang="ts">
  import { computed, ref } from 'vue'
  import { NEmpty, NInput, NTooltip } from 'naive-ui'

  import type { CachePrefixGroupDto } from '@/modules/system/api/cache'
  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'CacheGroupPanel' })

  const props = defineProps<{
    groups: CachePrefixGroupDto[]
    selectedCode: string
  }>()

  const emit = defineEmits<{
    select: [group: CachePrefixGroupDto]
  }>()

  /* ============ 状态 & 引用 ============ */

  const keyword = ref('')

  /* ============ 视图模型 ============ */

  const filteredGroups = computed(() => {
    const text = keyword.value.trim().toLowerCase()
    if (!text) return props.groups
    return props.groups.filter(item => {
      const haystack = [item.name, item.code, item.prefix, item.description]
        .join(' ')
        .toLowerCase()
      return haystack.includes(text)
    })
  })

  const selectedGroup = computed(
    () =>
      props.groups.find(item => item.code === props.selectedCode) ??
      props.groups[0] ??
      null
  )

  const showSearch = computed(() => props.groups.length > 6)

  function tooltipText(group: CachePrefixGroupDto) {
    const lines = [group.description, group.prefix].filter(Boolean)
    return lines.join('\n')
  }
</script>

<template>
  <section class="cache-group-panel">
    <header class="cache-group-panel__header">
      <div class="cache-group-panel__heading">
        <span class="cache-group-panel__title">缓存分组</span>
        <span class="cache-group-panel__count">{{ groups.length }}</span>
      </div>

      <NInput
        v-if="showSearch"
        v-model:value="keyword"
        clearable
        size="small"
        placeholder="搜索分组 / 前缀"
        class="cache-group-panel__search"
      >
        <template #prefix>
          <Icon icon="ant-design:search-outlined" />
        </template>
      </NInput>
    </header>

    <div v-if="filteredGroups.length" class="cache-group-panel__chips">
      <NTooltip
        v-for="group in filteredGroups"
        :key="group.code"
        placement="top"
        :disabled="!tooltipText(group)"
      >
        <template #trigger>
          <button
            type="button"
            class="cache-group-chip"
            :class="{ 'is-active': selectedCode === group.code }"
            @click="emit('select', group)"
          >
            <span class="cache-group-chip__name">{{ group.name }}</span>
            <span class="cache-group-chip__badge">{{ group.keyCount }}</span>
          </button>
        </template>
        <div class="cache-group-panel__tooltip">{{ tooltipText(group) }}</div>
      </NTooltip>
    </div>

    <NEmpty
      v-else
      class="cache-group-panel__empty"
      size="small"
      description="无匹配分组"
    />

    <p v-if="selectedGroup" class="cache-group-panel__meta">
      <span class="cache-group-panel__meta-label">当前</span>
      <span class="cache-group-panel__meta-name">{{ selectedGroup.name }}</span>
      <span class="cache-group-panel__meta-sep">·</span>
      <code class="cache-group-panel__meta-prefix">{{ selectedGroup.prefix }}</code>
      <span v-if="selectedGroup.description" class="cache-group-panel__meta-desc">
        {{ selectedGroup.description }}
      </span>
    </p>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/group-panel.scss';
</style>

<style lang="scss">
  @use '../styles/group-panel.global.scss';
</style>
