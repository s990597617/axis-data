<script setup lang="ts">
  import { computed } from 'vue'
  import { NButton, NEmpty, NInput, NPagination, NSpin, NTag } from 'naive-ui'

  import type { DictTypeDto } from '@/modules/system/api/dict'
  import { isSuperAdmin } from '@/shared/access/access.service'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import { Icon } from '@/shared/icons'
  import { useUserStore } from '@/store/modules/user.store'

  defineOptions({ name: 'DictTypePanel' })

  defineProps<{
    loading: boolean
    keyword: string
    types: DictTypeDto[]
    selectedId?: string
    page: number
    pageSize: number
    total: number
  }>()

  const emit = defineEmits<{
    'update:keyword': [value: string]
    'update:page': [value: number]
    'update:pageSize': [value: number]
    select: [row: DictTypeDto]
    create: []
    edit: [row: DictTypeDto]
    delete: [row: DictTypeDto]
  }>()

  const userStore = useUserStore()
  const isSuper = computed(() =>
    isSuperAdmin({
      roles: userStore.roles,
      permissions: userStore.permissions,
      accountType: userStore.userInfo?.accountType
    })
  )

  function canEditType(row: DictTypeDto) {
    return !row.isBuiltin || isSuper.value
  }
</script>

<template>
  <aside class="dict-type-panel">
    <header class="dict-type-panel__header">
      <div class="dict-type-panel__heading">
        <span class="dict-type-panel__icon-wrap">
          <Icon icon="ant-design:book-outlined" />
        </span>
        <div class="dict-type-panel__title-block">
          <h2 class="dict-type-panel__title">字典类型</h2>
          <p class="dict-type-panel__desc">选择左侧类型，右侧维护数据项</p>
        </div>
        <span class="dict-type-panel__count">{{ total }}</span>
      </div>

      <div class="dict-type-panel__tools">
        <NInput
          :value="keyword"
          clearable
          size="small"
          placeholder="搜索名称或编码"
          @update:value="value => emit('update:keyword', value)"
        >
          <template #prefix>
            <Icon icon="ant-design:search-outlined" />
          </template>
        </NInput>
        <NButton
          v-permission="systemPermissionCodes.dict.create"
          type="primary"
          size="small"
          @click="emit('create')"
        >
          <template #icon>
            <Icon icon="ant-design:plus-outlined" />
          </template>
          新增
        </NButton>
      </div>
    </header>

    <NSpin :show="loading" class="dict-type-panel__spin">
      <div v-if="types.length" class="dict-type-panel__list">
        <button
          v-for="item in types"
          :key="item.id"
          type="button"
          class="dict-type-card"
          :class="{
            'is-active': item.id === selectedId,
            'is-disabled': item.status !== 1
          }"
          @click="emit('select', item)"
        >
          <span class="dict-type-card__accent" />
          <div class="dict-type-card__body">
            <div class="dict-type-card__row">
              <span class="dict-type-card__name" :title="item.name">{{ item.name }}</span>
              <div class="dict-type-card__badges">
                <NTag v-if="item.isBuiltin" size="tiny" :bordered="false" type="info" round>
                  内置
                </NTag>
                <NTag
                  v-if="!item.isTenant"
                  size="tiny"
                  :bordered="false"
                  type="warning"
                  round
                >
                  平台
                </NTag>
                <span
                  class="dict-type-card__status"
                  :class="item.status === 1 ? 'is-on' : 'is-off'"
                >
                  {{ item.status === 1 ? '启用' : '停用' }}
                </span>
              </div>
            </div>
            <code class="dict-type-card__code">{{ item.code }}</code>
          </div>

          <div class="dict-type-card__actions" @click.stop>
            <button
              v-if="canEditType(item)"
              v-permission="systemPermissionCodes.dict.edit"
              type="button"
              class="dict-type-card__action"
              title="编辑"
              @click="emit('edit', item)"
            >
              <Icon icon="ant-design:edit-outlined" />
            </button>
            <button
              v-if="!item.isBuiltin"
              v-permission="systemPermissionCodes.dict.delete"
              type="button"
              class="dict-type-card__action is-danger"
              title="删除"
              @click="emit('delete', item)"
            >
              <Icon icon="ant-design:delete-outlined" />
            </button>
          </div>
        </button>
      </div>

      <div v-else class="dict-type-panel__empty">
        <NEmpty description="暂无字典类型" size="small" />
      </div>
    </NSpin>

    <footer v-if="total > 0" class="dict-type-panel__pager">
      <NPagination
        :page="page"
        :page-size="pageSize"
        :item-count="total"
        :page-sizes="[12, 20, 50]"
        size="small"
        simple
        show-size-picker
        :prefix="info => `共 ${info.itemCount ?? 0} 条`"
        @update:page="value => emit('update:page', value)"
        @update:page-size="value => emit('update:pageSize', value)"
      />
    </footer>
  </aside>
</template>
