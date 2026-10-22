<script setup lang="ts">
  import { ref } from 'vue'
  import {
    NButton,
    NDatePicker,
    NForm,
    NFormItem,
    NInput,
    NRadio,
    NRadioGroup,
    type FormInst,
    type FormRules
  } from 'naive-ui'

  defineOptions({ name: 'ProfileBasicForm' })

  defineProps<{
    basicForm: {
      realName: string
      nickName: string
      avatar: string
      sex: number
      birthday: number | null
      phone: string
      email: string
      remark: string
    }
    basicRules: FormRules
    permissions: { update: string }
    savingBasic: boolean
  }>()

  const emit = defineEmits<{
    save: []
  }>()

  const formRef = ref<FormInst | null>(null)

  async function handleSave() {
    await formRef.value?.validate()
    emit('save')
  }
</script>

<template>
  <section class="profile-section">
    <div class="profile-section__header">
      <h3 class="profile-section__title">基础信息</h3>
      <p class="profile-section__desc">完善个人联系方式与资料，保存后立即生效</p>
    </div>

    <n-form
      ref="formRef"
      class="profile-form"
      :model="basicForm"
      :rules="basicRules"
      label-placement="top"
      require-mark-placement="right-hanging"
    >
      <div class="profile-form__grid">
        <n-form-item label="真实姓名" path="realName" required>
          <n-input
            v-model:value="basicForm.realName"
            maxlength="32"
            placeholder="请输入真实姓名"
          />
        </n-form-item>
        <n-form-item label="昵称" path="nickName">
          <n-input
            v-model:value="basicForm.nickName"
            maxlength="32"
            placeholder="请输入昵称"
          />
        </n-form-item>
        <n-form-item label="手机号码" path="phone">
          <n-input
            v-model:value="basicForm.phone"
            maxlength="16"
            placeholder="请输入手机号码"
          />
        </n-form-item>
        <n-form-item label="邮箱" path="email">
          <n-input
            v-model:value="basicForm.email"
            maxlength="64"
            placeholder="请输入邮箱"
          />
        </n-form-item>
        <n-form-item label="出生日期" path="birthday">
          <n-date-picker
            v-model:value="basicForm.birthday"
            type="date"
            clearable
            style="width: 100%"
            placeholder="请选择出生日期"
          />
        </n-form-item>
        <n-form-item label="性别" path="sex">
          <n-radio-group v-model:value="basicForm.sex">
            <n-radio :value="1">男</n-radio>
            <n-radio :value="2">女</n-radio>
            <n-radio :value="0">未知</n-radio>
          </n-radio-group>
        </n-form-item>
      </div>

      <n-form-item label="备注" path="remark">
        <n-input
          v-model:value="basicForm.remark"
          type="textarea"
          maxlength="256"
          :rows="3"
          placeholder="可填写个人备注"
          show-count
        />
      </n-form-item>

      <div class="profile-form__actions">
        <n-button
          type="primary"
          :loading="savingBasic"
          v-permission="permissions.update"
          @click="handleSave"
        >
          保存基本信息
        </n-button>
      </div>
    </n-form>
  </section>
</template>

<style scoped lang="scss">
  @use '../styles/basic-form.scss';
</style>
