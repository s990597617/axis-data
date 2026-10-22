<script setup lang="ts">
  import { computed, nextTick, reactive, ref } from 'vue'

  import type { DictTypeDto, SaveDictTypeInput } from '@/modules/system/api/dict'
  import { createDictType, updateDictType } from '@/modules/system/api/dict'
  import { isSuperAdmin } from '@/shared/access/access.service'
  import {
    AxisForm,
    type AxisFormExposed,
    type AxisFormSection
  } from '@/shared/components/axis-form'
  import { statusOptions } from '@/shared/components/axis-crud'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'
  import { useUserStore } from '@/store/modules/user.store'

  import { yesNoOptions } from '../utils/display'

  defineOptions({ name: 'DictTypeFormDialog' })

  const emit = defineEmits<{
    saved: [payload?: { id?: string; code?: string }]
  }>()

  /* ============ 状态 & 引用 ============ */

  const userStore = useUserStore()
  const formRef = ref<AxisFormExposed | null>(null)
  const visible = ref(false)
  const submitting = ref(false)
  const editing = ref(false)
  const editId = ref<string>()
  const isBuiltin = ref(false)
  const form = reactive<Record<string, unknown>>({
    name: '',
    code: '',
    isTenant: true,
    status: 1,
    orderNo: 100,
    remark: ''
  })

  /* ============ 视图模型 ============ */

  const canEditTenantFlag = computed(
    () =>
      !editing.value &&
      isSuperAdmin({
        roles: userStore.roles,
        permissions: userStore.permissions,
        accountType: userStore.userInfo?.accountType
      })
  )

  const formTitle = computed(() => (editing.value ? '编辑字典类型' : '新增字典类型'))

  const formSubtitle = computed(() =>
    editing.value
      ? '调整名称、状态与排序 · 内置字典编码与租户标记不可改'
      : '创建字典类型后，可在右侧维护数据项'
  )

  const formSections = computed<AxisFormSection[]>(() => [
    {
      key: 'basic',
      title: '基础信息',
      icon: 'ant-design:book-outlined',
      columns: 2,
      schemas: [
        {
          field: 'code',
          label: '字典编码',
          required: true,
          disabled: () => isBuiltin.value,
          componentProps: { maxlength: 64, placeholder: '请输入字典编码' }
        },
        {
          field: 'name',
          label: '字典名称',
          required: true,
          componentProps: { maxlength: 64, placeholder: '请输入字典名称' }
        }
      ]
    },
    {
      key: 'settings',
      title: '其他设置',
      icon: 'ant-design:setting-outlined',
      columns: 2,
      schemas: [
        {
          field: 'isTenant',
          label: '租户字典',
          component: 'radio',
          options: yesNoOptions,
          required: true,
          disabled: () => !canEditTenantFlag.value
        },
        {
          field: 'status',
          label: '状态',
          component: 'radio',
          options: statusOptions,
          required: true
        },
        {
          field: 'orderNo',
          label: '排序',
          component: 'inputNumber',
          required: true,
          componentProps: { min: 0, max: 9999, placeholder: '请输入排序' }
        },
        {
          field: 'remark',
          label: '备注',
          component: 'textarea',
          fullWidth: true,
          componentProps: {
            maxlength: 256,
            showCount: true,
            rows: 2,
            placeholder: '请输入备注'
          }
        }
      ]
    }
  ])

  /* ============ 交互操作 ============ */

  function resetForm() {
    Object.assign(form, {
      name: '',
      code: '',
      isTenant: true,
      status: 1,
      orderNo: 100,
      remark: ''
    })
  }

  async function openCreate() {
    editing.value = false
    editId.value = undefined
    isBuiltin.value = false
    resetForm()
    // 非超管只能建租户字典
    if (
      !isSuperAdmin({
        roles: userStore.roles,
        permissions: userStore.permissions,
        accountType: userStore.userInfo?.accountType
      })
    ) {
      form.isTenant = true
    }
    visible.value = true
    await nextTick()
    formRef.value?.restoreValidation()
  }

  async function openEdit(row: DictTypeDto) {
    editing.value = true
    editId.value = row.id
    isBuiltin.value = Boolean(row.isBuiltin)
    Object.assign(form, {
      name: row.name,
      code: row.code,
      isTenant: Boolean(row.isTenant),
      status: row.status,
      orderNo: row.orderNo,
      remark: row.remark ?? ''
    })
    visible.value = true
    await nextTick()
    formRef.value?.restoreValidation()
  }

  function buildSubmitData(): SaveDictTypeInput {
    const superAdmin = isSuperAdmin({
      roles: userStore.roles,
      permissions: userStore.permissions,
      accountType: userStore.userInfo?.accountType
    })
    return {
      name: String(form.name ?? '').trim(),
      code: String(form.code ?? '').trim(),
      isTenant: superAdmin ? Boolean(form.isTenant) : true,
      status: Number(form.status ?? 1),
      orderNo: Number(form.orderNo ?? 100),
      remark: String(form.remark ?? '').trim() || undefined
    }
  }

  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      return
    }

    submitting.value = true
    const data = buildSubmitData()
    const operation =
      editing.value && editId.value ? updateDictType(editId.value, data) : createDictType(data)
    const [, error] = await tryRequest(operation)
    submitting.value = false
    if (error) return

    message.success(editing.value ? '更新成功' : '新增成功')
    const preferredId = editId.value
    const preferredCode = data.code
    visible.value = false
    emit('saved', { id: preferredId, code: preferredCode })
  }

  defineExpose({ openCreate, openEdit })
</script>

<template>
  <FormDialog
    v-model="visible"
    layout="sectioned"
    :title="formTitle"
    :subtitle="formSubtitle"
    icon="ant-design:book-outlined"
    :width="640"
    :confirm-loading="submitting"
    confirm-text="保存"
    :mask-closable="!submitting"
    @confirm="handleSubmit"
  >
    <AxisForm
      ref="formRef"
      :model="form"
      :sections="formSections"
      :columns="2"
      :disabled="submitting"
    />
  </FormDialog>
</template>
