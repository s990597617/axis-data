<script setup lang="ts">
  import { ref } from 'vue'
  import { NSpin } from 'naive-ui'

  import ChangePasswordDialog from './components/ChangePasswordDialog.vue'
  import ProfileBasicForm from './components/ProfileBasicForm.vue'
  import ProfileHero from './components/ProfileHero.vue'
  import ProfileSecurityPanel from './components/ProfileSecurityPanel.vue'
  import { useProfilePage } from './composables/useProfilePage'

  defineOptions({ name: 'SysProfile' })

  /* ============ 状态 & 引用 ============ */

  const {
    accountTypeLabel,
    avatarSrc,
    basicForm,
    basicRules,
    copyAccountId,
    createdTimeText,
    displayName,
    handleAvatarChange,
    handleSaveBasic,
    isAccountEnabled,
    loading,
    permissions,
    primaryRoleName,
    profile,
    savingBasic,
    statusHint,
    statusTagLabel,
    statusValueLabel,
    uploadingAvatar
  } = useProfilePage()

  const changePasswordDialogRef = ref<InstanceType<typeof ChangePasswordDialog> | null>(null)

  function openChangePassword() {
    changePasswordDialogRef.value?.open()
  }
</script>

<template>
  <div class="profile-page">
    <n-spin :show="loading">
      <ProfileHero
        :account-type-label="accountTypeLabel"
        :avatar-src="avatarSrc"
        :created-time-text="createdTimeText"
        :display-name="displayName"
        :is-account-enabled="isAccountEnabled"
        :primary-role-name="primaryRoleName"
        :profile="profile"
        :status-hint="statusHint"
        :status-tag-label="statusTagLabel"
        :status-value-label="statusValueLabel"
        :uploading-avatar="uploadingAvatar"
        @copy-account="copyAccountId"
        @avatar-change="handleAvatarChange"
      />

      <div class="profile-panel">
        <ProfileBasicForm
          :basic-form="basicForm"
          :basic-rules="basicRules"
          :permissions="permissions"
          :saving-basic="savingBasic"
          @save="handleSaveBasic"
        />

        <ProfileSecurityPanel
          :email="profile?.email"
          :phone="profile?.phone"
          :permissions="permissions"
          @change-password="openChangePassword"
        />
      </div>
    </n-spin>

    <ChangePasswordDialog ref="changePasswordDialogRef" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
