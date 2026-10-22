<script setup lang="ts">
  import { computed, nextTick, onMounted, reactive, ref } from 'vue'
  import type { FormRules } from 'naive-ui'

  import type { OrgTreeNode } from '@/modules/system/api/org'
  import { getRolePage, type RoleDto } from '@/modules/system/api/role'
  import {
    createUser,
    getUser,
    updateUser,
    type CreateUserInput,
    type UpdateUserInput
  } from '@/modules/system/api/user'
  import {
    AxisForm,
    type AxisFormExposed,
    type AxisFormOption,
    type AxisFormSection
  } from '@/shared/components/axis-form'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { formatLocalDate } from '@/shared/utils/date'
  import { message } from '@/shared/utils/message'

  import UserAvatarField from './UserAvatarField.vue'

  defineOptions({ name: 'UserFormDialog' })

  const props = defineProps<{
    orgTree: OrgTreeNode[]
  }>()

  const emit = defineEmits<{
    saved: []
  }>()

  /* ============ 状态 & 引用 ============ */

  const visible = ref(false)
  const isEdit = ref(false)
  const editId = ref<string>()
  const loading = ref(false)
  const submitting = ref(false)
  const formRef = ref<AxisFormExposed | null>(null)
  const roleOptions = ref<AxisFormOption[]>([])

  const statusOptions = [
    { label: '启用', value: 1 },
    { label: '禁用', value: 0 }
  ]

  const accountTypeOptions = [
    { label: '普通用户', value: 777 },
    { label: '系统管理员', value: 888 },
    { label: '超级管理员', value: 999 }
  ]

  const defaultForm = () => ({
    orgId: undefined as string | undefined,
    posId: undefined as string | undefined,
    account: '',
    realName: '',
    nickName: '',
    avatar: '',
    sex: 0,
    phone: '',
    email: '',
    jobNum: '',
    joinDate: null as number | null,
    accountType: 777,
    orderNo: 100,
    status: 1,
    remark: '',
    roleIds: [] as string[],
    version: '0'
  })

  const form = reactive(defaultForm())

  /* ============ 视图模型 ============ */

  const drawerTitle = computed(() => (isEdit.value ? '编辑用户' : '新增用户'))

  const drawerSubtitle = computed(() =>
    isEdit.value
      ? `编辑账号 @${form.account || '-'} · 调整资料与角色`
      : '创建账号并分配机构与角色 · 初始密码由系统生成'
  )

  const rules: FormRules = {
    orgId: [{ required: true, message: '请选择所属机构', trigger: 'change' }],
    account: [
      { required: true, message: '请输入账号', trigger: 'blur' },
      { min: 3, max: 32, message: '账号长度 3-32 位', trigger: 'blur' }
    ],
    realName: [{ required: true, message: '请输入真实姓名', trigger: 'blur' }],
    nickName: [{ required: true, message: '请输入昵称', trigger: 'blur' }],
    accountType: [{ required: true, type: 'number', message: '请选择账号类型', trigger: 'change' }],
    status: [{ required: true, type: 'number', message: '请选择状态', trigger: 'change' }],
    orderNo: [{ required: true, type: 'number', message: '请输入排序', trigger: ['blur', 'change'] }],
    roleIds: [
      {
        type: 'array',
        required: true,
        min: 1,
        message: '请至少选择一个角色',
        trigger: 'change'
      }
    ]
  }

  function orgTreeToSelectOptions(nodes: OrgTreeNode[]): AxisFormOption[] {
    return nodes.map(n => ({
      key: n.id,
      label: n.name,
      children: n.children?.length ? orgTreeToSelectOptions(n.children) : undefined
    }))
  }

  const orgSelectOptions = computed(() => orgTreeToSelectOptions(props.orgTree))

  // 左侧头像跨 3 行，右侧两列：机构/账号 → 姓名/昵称 → 角色
  const formSections = computed<AxisFormSection[]>(() => [
    {
      key: 'basic',
      title: '基础信息',
      icon: 'ant-design:idcard-outlined',
      columns: '1 720:3',
      schemas: [
        {
          field: 'avatar',
          label: '头像',
          component: 'custom',
          span: 1,
          gridItemProps: {
            class: 'user-drawer-avatar-cell',
            style: { gridRow: 'span 3', alignSelf: 'stretch' }
          },
          formItemProps: {
            showLabel: false,
            class: 'user-drawer-avatar-item'
          }
        },
        {
          field: 'orgId',
          label: '所属机构',
          component: 'treeSelect',
          required: true,
          options: orgSelectOptions.value,
          componentProps: {
            keyField: 'key',
            labelField: 'label',
            childrenField: 'children',
            filterable: true,
            placeholder: '请选择所属机构'
          }
        },
        {
          field: 'account',
          label: '账号',
          required: true,
          disabled: isEdit.value,
          componentProps: { maxlength: 32, placeholder: '请输入登录账号' }
        },
        {
          field: 'realName',
          label: '真实姓名',
          required: true,
          componentProps: { maxlength: 32, placeholder: '请输入真实姓名' }
        },
        {
          field: 'nickName',
          label: '昵称',
          required: true,
          componentProps: { maxlength: 32, placeholder: '请输入昵称' }
        },
        {
          field: 'roleIds',
          label: '角色',
          component: 'select',
          required: true,
          span: 2,
          options: roleOptions.value,
          componentProps: {
            multiple: true,
            filterable: true,
            maxTagCount: 3,
            placeholder: '请选择角色'
          }
        }
      ]
    },
    {
      key: 'extra',
      title: '补充信息',
      icon: 'ant-design:contacts-outlined',
      columns: '1 560:2 860:3',
      schemas: [
        {
          field: 'jobNum',
          label: '工号',
          componentProps: { maxlength: 32, placeholder: '请输入工号' }
        },
        {
          field: 'joinDate',
          label: '入职日期',
          component: 'date',
          componentProps: { placeholder: '请选择入职日期' }
        },
        {
          field: 'accountType',
          label: '账号类型',
          component: 'select',
          required: true,
          options: accountTypeOptions,
          componentProps: { placeholder: '请选择账号类型' }
        },
        {
          field: 'sex',
          label: '性别',
          component: 'radio',
          options: [
            { label: '未知', value: 0 },
            { label: '男', value: 1 },
            { label: '女', value: 2 }
          ]
        },
        {
          field: 'phone',
          label: '手机号',
          componentProps: { maxlength: 16, placeholder: '请输入手机号' }
        },
        {
          field: 'email',
          label: '邮箱',
          componentProps: { maxlength: 64, placeholder: '请输入邮箱' }
        }
      ]
    },
    {
      key: 'settings',
      title: '账号设置',
      icon: 'ant-design:setting-outlined',
      columns: '1 560:2',
      schemas: [
        {
          field: 'status',
          label: '状态',
          component: 'radio',
          required: true,
          options: statusOptions
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

  async function loadRoleOptions() {
    if (roleOptions.value.length) return
    const [data] = await tryRequest(getRolePage({ page: 1, pageSize: 200, status: 1 }))
    const page = data as { items?: RoleDto[]; Items?: RoleDto[] } | null
    const rows = page?.items ?? page?.Items ?? []
    roleOptions.value = rows.map(role => ({
      label: role.name,
      value: String(role.id)
    }))
  }

  async function open(id?: string) {
    Object.assign(form, defaultForm())
    isEdit.value = !!id
    editId.value = id
    visible.value = true
    await loadRoleOptions()

    if (id) {
      loading.value = true
      const [data] = await tryRequest(getUser(id))
      loading.value = false
      if (data) {
        form.orgId = data.orgId
        form.posId = data.posId || undefined
        form.account = data.account
        form.realName = data.realName
        form.nickName = data.nickName || ''
        form.avatar = data.avatar || ''
        form.sex = data.sex
        form.phone = data.phone || ''
        form.email = data.email || ''
        form.jobNum = data.jobNum || ''
        form.joinDate = data.joinDate ? new Date(data.joinDate).getTime() : null
        form.accountType = data.accountType
        form.orderNo = data.orderNo
        form.status = data.status
        form.remark = data.remark || ''
        form.roleIds = (data.roleIds ?? []).map(String)
        form.version = data.version ?? '0'
      }
    }

    await nextTick()
    formRef.value?.restoreValidation()
  }

  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      // 字段下方已展示校验信息，勿再抛到全局错误提示
      return
    }

    submitting.value = true

    if (isEdit.value && editId.value) {
      const input: UpdateUserInput = {
        id: editId.value,
        version: form.version,
        orgId: form.orgId!,
        posId: form.posId,
        realName: form.realName,
        nickName: form.nickName,
        avatar: form.avatar ?? '',
        sex: form.sex,
        phone: form.phone || undefined,
        email: form.email || undefined,
        jobNum: form.jobNum || undefined,
        joinDate: form.joinDate ? formatLocalDate(form.joinDate) : undefined,
        accountType: form.accountType,
        orderNo: form.orderNo,
        status: form.status,
        remark: form.remark || undefined,
        roleIds: form.roleIds
      }
      const [, err] = await tryRequest(updateUser(editId.value, input))
      if (!err) {
        message.success('更新成功')
        visible.value = false
        emit('saved')
      }
    } else {
      const input: CreateUserInput = {
        orgId: form.orgId!,
        posId: form.posId,
        account: form.account,
        realName: form.realName,
        nickName: form.nickName,
        avatar: form.avatar || undefined,
        sex: form.sex,
        phone: form.phone || undefined,
        email: form.email || undefined,
        jobNum: form.jobNum || undefined,
        joinDate: form.joinDate ? formatLocalDate(form.joinDate) : undefined,
        accountType: form.accountType,
        orderNo: form.orderNo,
        status: form.status,
        remark: form.remark || undefined,
        roleIds: form.roleIds
      }
      const [, err] = await tryRequest(createUser(input))
      if (!err) {
        message.success('创建成功，初始密码为系统默认值，可通过重置密码修改')
        visible.value = false
        emit('saved')
      }
    }

    submitting.value = false
  }

  onMounted(() => {
    void loadRoleOptions()
  })

  /* ============ 对外暴露 ============ */

  defineExpose({ open })
</script>

<template>
  <FormDialog
    v-model="visible"
    layout="sectioned"
    :title="drawerTitle"
    :subtitle="drawerSubtitle"
    icon="ant-design:user-outlined"
    :width="980"
    :loading="loading"
    :confirm-loading="submitting"
    :confirm-text="isEdit ? '保存修改' : '立即创建'"
    :mask-closable="!submitting"
    @confirm="handleSubmit"
  >
    <AxisForm
      ref="formRef"
      class="user-drawer-form"
      :model="form"
      :sections="formSections"
      :rules="rules"
      :loading="loading"
      :disabled="submitting"
    >
      <template #field-avatar="{ value, setValue, disabled }">
        <UserAvatarField
          :value="typeof value === 'string' ? value : ''"
          :disabled="disabled"
          :fallback-text="form.realName || form.nickName || form.account"
          @update:value="setValue"
        />
      </template>
    </AxisForm>
  </FormDialog>
</template>

<style scoped lang="scss">
  .user-drawer-form {
    /* 基础信息：左列固定窄宽，右侧两列吃满剩余宽度，避免等分三列留白 */
    :deep(.axis-form__section.has-header:first-of-type .n-grid) {
      grid-template-columns: 188px minmax(0, 1fr) minmax(0, 1fr) !important;
      column-gap: 12px !important;
    }

    :deep(.user-drawer-avatar-cell) {
      width: 188px;
      max-width: 188px;
    }

    :deep(.user-drawer-avatar-item) {
      height: 100%;
      margin-bottom: 0;

      .n-form-item-blank {
        display: flex;
        height: 100%;
        align-items: flex-start;
      }
    }
  }

  @media (max-width: 720px) {
    .user-drawer-form {
      :deep(.axis-form__section.has-header:first-of-type .n-grid) {
        grid-template-columns: minmax(0, 1fr) !important;
      }

      :deep(.user-drawer-avatar-cell) {
        width: auto;
        max-width: none;
        grid-row: auto !important;
      }
    }
  }
</style>
