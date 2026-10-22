<script setup lang="ts">
  import { NButton, NInput, NSpin, NTree } from 'naive-ui'

  import { AxisTable } from '@/shared/components/axis-table'
  import { DetailDrawer } from '@/shared/components/detail-drawer'
  import { ExcelImportDialog } from '@/shared/components/excel-import-dialog'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { Icon } from '@/shared/icons'

  import UserFormDialog from './components/UserFormDialog.vue'
  import { useUserPage } from './composables/useUserPage'

  defineOptions({ name: 'SysUser' })

  /* ============ 状态 & 引用 ============ */

  const {
    systemPermissionCodes,
    orgLoading,
    orgTreeData,
    orgOptions,
    selectedOrgId,
    tableRef,
    formDialogRef,
    columns,
    getUserPage,
    handleOrgSelect,
    handleDelete,
    handleToggleStatus,
    openResetPassword,
    clearResetPassword,
    handleResetPassword,
    resetPasswordVisible,
    resetPasswordTarget,
    resetPasswordValue,
    resetPasswordSubmitting,
    showDetail,
    detailVisible,
    detailLoading,
    detailUser,
    detailFields,
    detailStatusText,
    detailStatusType,
    importVisible,
    openImport,
    downloadImportTemplate,
    importUserFile,
    handleImportSuccess,
    exporting,
    handleExport
  } = useUserPage()
</script>

<template>
  <div class="user-page">
    <div class="user-page__tree-panel">
      <div class="user-page__tree-header">
        <Icon icon="ant-design:apartment-outlined" class="user-page__tree-icon" />
        <span>机构列表</span>
      </div>
      <NSpin :show="orgLoading" class="user-page__tree-spin">
        <NTree
          :data="orgOptions"
          :selected-keys="selectedOrgId ? [selectedOrgId] : []"
          key-field="key"
          label-field="label"
          children-field="children"
          selectable
          expand-on-click
          default-expand-all
          block-line
          show-line
          @update:selected-keys="handleOrgSelect"
        />
      </NSpin>
    </div>

    <div class="user-page__content">
      <AxisTable
        ref="tableRef"
        :columns="columns"
        :request="params => getUserPage({ ...params, orgId: selectedOrgId } as any)"
        :query-params="{ orgId: selectedOrgId }"
        :search-config="{ minItemWidth: 220, maxColumns: 3 }"
        row-key="id"
        :pagination="{ pageSize: 20 }"
        :action-column="{
          width: 176,
          maxInline: 5,
          actions: [
            {
              key: 'detail',
              label: '详情',
              icon: 'ant-design:eye-outlined',
              type: 'primary',
              permissions: [systemPermissionCodes.user.detail],
              onClick: row => showDetail(row)
            },
            {
              key: 'edit',
              label: '编辑',
              icon: 'ant-design:edit-outlined',
              color: '#f0a020',
              permissions: [systemPermissionCodes.user.edit],
              onClick: row => formDialogRef?.open(row.id)
            },
            {
              key: 'disable',
              label: '禁用',
              icon: 'ant-design:stop-outlined',
              color: '#d03050',
              permissions: [systemPermissionCodes.user.edit],
              ifShow: row => row.status === 1,
              confirm: row => `确认禁用用户「${row.realName}」？`,
              onClick: row => handleToggleStatus(row)
            },
            {
              key: 'enable',
              label: '启用',
              icon: 'ant-design:check-circle-outlined',
              type: 'success',
              permissions: [systemPermissionCodes.user.edit],
              ifShow: row => row.status !== 1,
              confirm: row => `确认启用用户「${row.realName}」？`,
              onClick: row => handleToggleStatus(row)
            },
            {
              key: 'reset-pwd',
              label: '重置密码',
              icon: 'ant-design:key-outlined',
              color: '#2080f0',
              permissions: [systemPermissionCodes.user.resetPassword],
              onClick: row => openResetPassword(row)
            },
            {
              key: 'delete',
              label: '删除',
              icon: 'ant-design:delete-outlined',
              type: 'error',
              permissions: [systemPermissionCodes.user.delete],
              ifShow: row => row.account !== 'admin',
              confirm: row => `确认删除用户「${row.realName}」？`,
              onClick: row => handleDelete(row)
            }
          ]
        }"
      >
        <template #toolbar>
          <NButton
            v-permission="systemPermissionCodes.user.create"
            type="primary"
            size="small"
            @click="formDialogRef?.open()"
          >
            <template #icon>
              <Icon icon="ant-design:plus-outlined" />
            </template>
            新增用户
          </NButton>
          <NButton
            v-permission="systemPermissionCodes.user.import"
            type="primary"
            ghost
            size="small"
            @click="openImport"
          >
            <template #icon>
              <Icon icon="ant-design:cloud-upload-outlined" />
            </template>
            导入
          </NButton>
          <NButton
            v-permission="systemPermissionCodes.user.export"
            type="primary"
            ghost
            size="small"
            :loading="exporting"
            @click="handleExport"
          >
            <template #icon>
              <Icon icon="ant-design:cloud-download-outlined" />
            </template>
            导出
          </NButton>
        </template>
      </AxisTable>
    </div>

    <UserFormDialog ref="formDialogRef" :org-tree="orgTreeData" @saved="tableRef?.reload()" />

    <ExcelImportDialog
      v-model="importVisible"
      title="导入用户"
      tip="仅支持新增。账号已存在或机构/角色编码无效的行会进入失败列表。"
      :download-template="downloadImportTemplate"
      :import-file="importUserFile"
      @success="handleImportSuccess"
    />

    <FormDialog
      v-model="resetPasswordVisible"
      title="重置密码"
      icon="ant-design:key-outlined"
      icon-theme="warning"
      :width="460"
      :confirm-loading="resetPasswordSubmitting"
      confirm-text="确认重置"
      :mask-closable="!resetPasswordSubmitting"
      @confirm="handleResetPassword"
      @after-leave="clearResetPassword"
    >
      <p class="reset-password-tip">
        为用户「{{ resetPasswordTarget?.realName || resetPasswordTarget?.account }}」设置临时密码。
        请勿使用全局固定密码，并通过安全渠道单独告知用户。
      </p>
      <NInput
        v-model:value="resetPasswordValue"
        type="password"
        show-password-on="mousedown"
        placeholder="请输入至少 8 位临时密码"
        :maxlength="64"
        :disabled="resetPasswordSubmitting"
        @keyup.enter="handleResetPassword"
      />
    </FormDialog>

    <DetailDrawer
      v-model:show="detailVisible"
      title="用户详情"
      :subtitle="detailUser ? `@${detailUser.account}` : undefined"
      icon="ant-design:user-outlined"
      :status-text="detailStatusText"
      :status-type="detailStatusType"
      :width="760"
      :loading="detailLoading"
      :fields="detailFields"
      section-title="基本信息"
    />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
