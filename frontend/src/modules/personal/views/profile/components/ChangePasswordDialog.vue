<script setup lang="ts">
  import { reactive, ref, watch } from 'vue'
  import { NForm, NFormItem, NInput, type FormInst, type FormRules } from 'naive-ui'

  import { changePassword } from '@/modules/system/api/profile'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'
  import { sm2Encrypt } from '@/shared/utils/sm-crypto'

  defineOptions({ name: 'ChangePasswordDialog' })

  const visible = ref(false)
  const submitting = ref(false)
  const formRef = ref<FormInst | null>(null)

  const form = reactive({
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  })

  const rules: FormRules = {
    oldPassword: [{ required: true, message: '请输入当前密码', trigger: ['input', 'blur'] }],
    newPassword: [
      { required: true, message: '请输入新密码', trigger: ['input', 'blur'] },
      {
        validator: (_rule, value: string) => !value || value.length >= 6,
        message: '新密码不能少于 6 个字符',
        trigger: ['input', 'blur']
      },
      {
        validator: (_rule, value: string) => !value || value !== form.oldPassword,
        message: '新密码不能与当前密码相同',
        trigger: ['input', 'blur']
      }
    ],
    confirmPassword: [
      { required: true, message: '请再次输入新密码', trigger: ['input', 'blur'] },
      {
        validator: (_rule, value: string) => !value || value === form.newPassword,
        message: '两次输入的新密码不一致',
        trigger: ['input', 'blur']
      }
    ]
  }

  function resetForm() {
    form.oldPassword = ''
    form.newPassword = ''
    form.confirmPassword = ''
    formRef.value?.restoreValidation()
  }

  function open() {
    resetForm()
    visible.value = true
  }

  function close() {
    visible.value = false
  }

  async function handleConfirm() {
    await formRef.value?.validate()
    submitting.value = true
    const [, error] = await tryRequest(
      changePassword({
        oldPassword: sm2Encrypt(form.oldPassword),
        newPassword: sm2Encrypt(form.newPassword),
        confirmPassword: sm2Encrypt(form.confirmPassword)
      })
    )
    submitting.value = false
    if (error) return

    message.success('密码修改成功')
    close()
  }

  watch(visible, value => {
    if (!value) resetForm()
  })

  defineExpose({ open, close })
</script>

<template>
  <FormDialog
    v-model="visible"
    title="修改密码"
    icon="ant-design:lock-outlined"
    :width="460"
    :confirm-loading="submitting"
    confirm-text="确认修改"
    :mask-closable="!submitting"
    @confirm="handleConfirm"
  >
    <n-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-placement="top"
      require-mark-placement="right-hanging"
    >
      <n-form-item label="当前密码" path="oldPassword" required>
        <n-input
          v-model:value="form.oldPassword"
          type="password"
          show-password-on="click"
          maxlength="64"
          placeholder="请输入当前密码"
          :disabled="submitting"
        />
      </n-form-item>
      <n-form-item label="新密码" path="newPassword" required>
        <n-input
          v-model:value="form.newPassword"
          type="password"
          show-password-on="click"
          maxlength="64"
          placeholder="请输入新密码（至少 6 位）"
          :disabled="submitting"
        />
      </n-form-item>
      <n-form-item label="确认新密码" path="confirmPassword" required>
        <n-input
          v-model:value="form.confirmPassword"
          type="password"
          show-password-on="click"
          maxlength="64"
          placeholder="请再次输入新密码"
          :disabled="submitting"
          @keyup.enter="handleConfirm"
        />
      </n-form-item>
    </n-form>
  </FormDialog>
</template>
