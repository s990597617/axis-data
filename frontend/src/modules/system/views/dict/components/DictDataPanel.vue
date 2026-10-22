<script setup lang="ts">
  import { computed, h, ref, watch } from 'vue'
  import { NButton, NEmpty, NTag } from 'naive-ui'

  import type { DictDataDto, DictTypeDto, SaveDictDataInput } from '@/modules/system/api/dict'
  import {
    createDictData,
    deleteDictData,
    getDictDataList,
    updateDictData
  } from '@/modules/system/api/dict'
  import { isSuperAdmin } from '@/shared/access/access.service'
  import { systemPermissionCodes } from '@/modules/system/permissions/codes'
  import {
    AxisCrudPage,
    renderStatus,
    statusOptions,
    type AxisCrudResource
  } from '@/shared/components/axis-crud'
  import type { AxisTableColumn } from '@/shared/components/axis-table'
  import { clearDictCache } from '@/shared/composables/use-dict'
  import { Icon } from '@/shared/icons'
  import { useUserStore } from '@/store/modules/user.store'

  import { tagStyleOptions, resolveTagStyleOption } from '../utils/display'

  defineOptions({ name: 'DictDataPanel' })

  const props = defineProps<{
    selectedType: DictTypeDto | null
  }>()

  const emit = defineEmits<{
    'edit-type': []
  }>()

  const userStore = useUserStore()
  const crudRef = ref<{ reload: () => void; open: (row?: DictDataDto) => void } | null>(null)

  /** 内置字典仅超管可改类型与数据项 */
  const canMutateSelected = computed(() => {
    if (!props.selectedType?.isBuiltin) return true
    return isSuperAdmin({
      roles: userStore.roles,
      permissions: userStore.permissions,
      accountType: userStore.userInfo?.accountType
    })
  })

  const columns: AxisTableColumn<DictDataDto>[] = [
    {
      key: 'label',
      title: '显示标签',
      search: { field: 'keyword', placeholder: '标签或字典值' }
    },
    { key: 'value', title: '字典值' },
    {
      key: 'tagStyle',
      title: '标签样式',
      width: 120,
      render: row => {
        const option = resolveTagStyleOption(row.tagStyle)
        return h(
          NTag,
          {
            size: 'small',
            bordered: false,
            type: option.value
          },
          { default: () => option.label }
        )
      }
    },
    {
      key: 'isDefault',
      title: '默认',
      width: 72,
      render: row =>
        h(
          NTag,
          { size: 'small', bordered: false, type: row.isDefault ? 'success' : 'default' },
          { default: () => (row.isDefault ? '是' : '否') }
        )
    },
    {
      key: 'status',
      title: '状态',
      width: 88,
      dict: statusOptions,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 72 },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } }
  ]

  const dataResource = computed<AxisCrudResource<DictDataDto>>(() => ({
    title: '',
    entityName: '数据项',
    permissions: {
      query: systemPermissionCodes.dict.dataQuery,
      create: systemPermissionCodes.dict.dataCreate,
      edit: systemPermissionCodes.dict.dataEdit,
      delete: systemPermissionCodes.dict.dataDelete
    },
    columns,
    formSubtitle: ({ editing, model }) =>
      editing
        ? `编辑「${String(model.label || '-')}」· ${props.selectedType?.name ?? ''}`
        : `为「${props.selectedType?.name ?? '当前字典'}」新增数据项`,
    actionColumn: {
      width: 120,
      maxInline: 2
    },
    canCreate: () => canMutateSelected.value,
    canEdit: () => canMutateSelected.value,
    canDelete: () => canMutateSelected.value,
    formSections: [
      {
        key: 'basic',
        title: '基础信息',
        icon: 'ant-design:tags-outlined',
        columns: 2,
        fields: [
          {
            field: 'dictTypeId',
            label: '字典类型',
            defaultValue: props.selectedType?.id,
            ifShow: () => false
          },
          {
            field: 'label',
            label: '显示标签',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入显示标签' }
          },
          {
            field: 'value',
            label: '字典值',
            required: true,
            defaultValue: '',
            componentProps: { maxlength: 64, placeholder: '请输入字典值' }
          },
          {
            field: 'tagStyle',
            label: '标签样式',
            component: 'custom',
            fullWidth: true,
            defaultValue: 'default'
          },
          {
            field: 'listClass',
            label: '列表样式',
            defaultValue: '',
            componentProps: { maxlength: 32, placeholder: '可选 CSS class' }
          },
          {
            field: 'isDefault',
            label: '默认项',
            component: 'switch',
            defaultValue: false
          }
        ]
      },
      {
        key: 'settings',
        title: '其他设置',
        icon: 'ant-design:setting-outlined',
        columns: 2,
        fields: [
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 1
          },
          {
            field: 'orderNo',
            label: '排序',
            component: 'inputNumber',
            required: true,
            defaultValue: 100,
            componentProps: { min: 0, max: 9999, placeholder: '请输入排序' }
          },
          {
            field: 'remark',
            label: '备注',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: 256,
              showCount: true,
              rows: 2,
              placeholder: '请输入备注'
            }
          }
        ]
      }
    ],
    request: params =>
      props.selectedType
        ? getDictDataList(props.selectedType.id, params)
        : Promise.resolve([]),
    create: async data => {
      await createDictData({
        ...(data as unknown as SaveDictDataInput),
        dictTypeId: props.selectedType!.id
      })
      clearDictCache(props.selectedType!.code)
    },
    update: async (id, data) => {
      await updateDictData(id, {
        ...(data as unknown as SaveDictDataInput),
        dictTypeId: props.selectedType!.id
      })
      clearDictCache(props.selectedType!.code)
    },
    remove: async id => {
      await deleteDictData(id)
      if (props.selectedType?.code) clearDictCache(props.selectedType.code)
    },
    pagination: false
  }))

  watch(
    () => props.selectedType?.id,
    () => {
      crudRef.value?.reload()
    }
  )

  function reload() {
    crudRef.value?.reload()
  }

  defineExpose({ reload })
</script>

<template>
  <section class="dict-data-panel">
    <template v-if="selectedType">
      <header class="dict-data-panel__hero">
        <div class="dict-data-panel__hero-main">
          <div class="dict-data-panel__eyebrow">
            <Icon icon="ant-design:unordered-list-outlined" />
            <span>字典数据项</span>
          </div>
          <h2 class="dict-data-panel__name">{{ selectedType.name }}</h2>
          <div class="dict-data-panel__meta">
            <code class="dict-data-panel__code">{{ selectedType.code }}</code>
            <NTag
              size="small"
              round
              :bordered="false"
              :type="selectedType.status === 1 ? 'success' : 'default'"
            >
              {{ selectedType.status === 1 ? '启用' : '停用' }}
            </NTag>
            <NTag
              v-if="selectedType.isBuiltin"
              size="small"
              round
              :bordered="false"
              type="info"
            >
              内置
            </NTag>
            <span v-if="selectedType.remark" class="dict-data-panel__remark">
              {{ selectedType.remark }}
            </span>
          </div>
        </div>

        <NButton
          v-if="canMutateSelected"
          v-permission="systemPermissionCodes.dict.edit"
          type="primary"
          secondary
          size="small"
          @click="emit('edit-type')"
        >
          <template #icon>
            <Icon icon="ant-design:edit-outlined" />
          </template>
          编辑类型
        </NButton>
      </header>

      <div class="dict-data-panel__table">
        <AxisCrudPage :key="selectedType.id" ref="crudRef" :resource="dataResource">
          <template #field-tagStyle="{ value, setValue, disabled }">
            <div class="dict-tag-style-picker" :class="{ 'is-disabled': disabled }">
              <button
                v-for="option in tagStyleOptions"
                :key="option.value"
                type="button"
                class="dict-tag-style-picker__item"
                :class="{ 'is-active': value === option.value }"
                :disabled="disabled"
                :title="option.label"
                @click="setValue(option.value)"
              >
                <span
                  class="dict-tag-style-picker__swatch"
                  :style="{ background: option.color }"
                />
                <span class="dict-tag-style-picker__label">{{ option.label }}</span>
              </button>
            </div>
          </template>
        </AxisCrudPage>
      </div>
    </template>

    <div v-else class="dict-data-panel__empty">
      <div class="dict-data-panel__empty-card">
        <span class="dict-data-panel__empty-icon">
          <Icon icon="ant-design:inbox-outlined" />
        </span>
        <NEmpty description="请选择左侧字典类型" />
        <p class="dict-data-panel__empty-hint">选中后可在此维护标签、字典值与默认项</p>
      </div>
    </div>
  </section>
</template>
