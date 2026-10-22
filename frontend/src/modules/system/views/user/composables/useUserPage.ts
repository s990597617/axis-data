import { computed, h, onMounted, ref } from 'vue'
import { NAvatar, NTag, type TreeOption } from 'naive-ui'

import { getOrgTree, type OrgTreeNode } from '@/modules/system/api/org'
import {
  changeUserStatus,
  deleteUser,
  downloadUserImportTemplate,
  exportUsers,
  getUser,
  getUserPage,
  importUsers,
  resetPassword,
  type UserDto
} from '@/modules/system/api/user'
import type { AxisTableAction, AxisTableColumn } from '@/shared/components/axis-table'
import type { DetailField } from '@/shared/components/detail-drawer'
import { tryRequest } from '@/shared/http'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { formatLocalDate, formatLocalDateTime } from '@/shared/utils/date'
import { message } from '@/shared/utils/message'

import type UserFormDialog from '../components/UserFormDialog.vue'

// 头像底色池：按姓名首字符稳定取色，避免列表闪烁
const AVATAR_COLORS = [
  '#4f8ef7',
  '#52c41a',
  '#fa8c16',
  '#eb2f96',
  '#722ed1',
  '#13c2c2',
  '#f5222d',
  '#1890ff'
]

const sexLabels: Record<number, string> = { 0: '未知', 1: '男', 2: '女' }

const statusOptions = [
  { label: '启用', value: 1 },
  { label: '禁用', value: 0 }
]

export function useUserPage() {
  const orgLoading = ref(false)
  const orgTreeData = ref<OrgTreeNode[]>([])
  const selectedOrgId = ref<string | undefined>()
  const tableRef = ref<AxisTableAction<UserDto> | null>(null)
  const formDialogRef = ref<InstanceType<typeof UserFormDialog> | null>(null)

  const resetPasswordVisible = ref(false)
  const resetPasswordTarget = ref<UserDto | null>(null)
  const resetPasswordValue = ref('')
  const resetPasswordSubmitting = ref(false)

  const detailVisible = ref(false)
  const detailLoading = ref(false)
  const detailUser = ref<UserDto | null>(null)

  function getAvatarColor(name: string) {
    if (!name) return AVATAR_COLORS[0]
    const code = name.charCodeAt(0) || 0
    return AVATAR_COLORS[code % AVATAR_COLORS.length]
  }

  function buildTreeOptions(nodes: OrgTreeNode[]): TreeOption[] {
    return nodes.map(n => ({
      key: n.id,
      label: n.name,
      children: n.children?.length ? buildTreeOptions(n.children) : undefined
    }))
  }

  const orgOptions = computed(() => buildTreeOptions(orgTreeData.value))

  async function loadOrgTree() {
    orgLoading.value = true
    const [data] = await tryRequest(getOrgTree())
    orgLoading.value = false
    if (data) orgTreeData.value = data
  }

  function handleOrgSelect(keys: Array<string | number>) {
    // 防止点击同一节点取消选中导致筛选丢失
    if (!keys.length) return
    selectedOrgId.value = keys[0] as string | undefined
    tableRef.value?.search({ orgId: selectedOrgId.value })
  }

  function renderAvatar(row: UserDto) {
    const avatar = row.avatar
      ? h(NAvatar, { round: true, size: 25, src: row.avatar })
      : h(
          NAvatar,
          {
            round: true,
            size: 25,
            style: {
              backgroundColor: getAvatarColor(row.realName || row.account),
              color: '#fff',
              fontSize: '12px'
            }
          },
          { default: () => (row.realName || row.account || '?').charAt(0).toUpperCase() }
        )
    return h(
      'div',
      { style: { display: 'flex', alignItems: 'center', justifyContent: 'center' } },
      [avatar]
    )
  }

  function renderStatus(row: UserDto) {
    return h(
      NTag,
      { type: row.status === 1 ? 'success' : 'error', size: 'small', round: true },
      { default: () => (row.status === 1 ? '启用' : '禁用') }
    )
  }

  const columns: AxisTableColumn<UserDto>[] = [
    {
      key: 'avatar',
      title: '头像',
      align: 'center',
      configurable: false,
      render: row => renderAvatar(row)
    },
    {
      key: 'account',
      title: '账号',
      search: {
        field: 'keyword',
        placeholder: '输入账号/姓名'
      }
    },
    { key: 'realName', title: '姓名' },
    { key: 'orgName', title: '所属机构', ellipsis: { tooltip: true } },
    { key: 'phone', title: '手机号' },
    {
      key: 'sex',
      title: '性别',
      render: row => h('span', sexLabels[row.sex as number] ?? '未知')
    },
    { key: 'jobNum', title: '工号' },
    {
      key: 'status',
      title: '状态',
      align: 'center',
      search: true,
      dict: statusOptions,
      render: row => renderStatus(row)
    }
  ]

  async function handleDelete(row: UserDto) {
    const [, err] = await tryRequest(deleteUser(row.id))
    if (!err) {
      message.success('删除成功')
      tableRef.value?.reload()
    }
  }

  async function handleToggleStatus(row: UserDto) {
    const newStatus = row.status === 1 ? 0 : 1
    const label = newStatus === 1 ? '启用' : '禁用'
    const [, err] = await tryRequest(changeUserStatus(row.id, newStatus))
    if (!err) {
      message.success(`已${label}`)
      tableRef.value?.reload()
    }
  }

  function openResetPassword(row: UserDto) {
    resetPasswordTarget.value = row
    resetPasswordValue.value = ''
    resetPasswordVisible.value = true
  }

  function clearResetPassword() {
    resetPasswordValue.value = ''
    resetPasswordTarget.value = null
  }

  async function handleResetPassword() {
    const target = resetPasswordTarget.value
    if (!target) return
    if (resetPasswordValue.value.length < 8) {
      message.warning('临时密码至少需要 8 位')
      return
    }

    resetPasswordSubmitting.value = true
    const [, err] = await tryRequest(resetPassword(target.id, resetPasswordValue.value))
    resetPasswordSubmitting.value = false
    if (err) return

    message.success('密码重置成功，请通过安全渠道告知用户')
    resetPasswordVisible.value = false
  }

  async function showDetail(row: UserDto) {
    detailVisible.value = true
    detailLoading.value = true
    detailUser.value = row

    const [data, error] = await tryRequest(getUser(row.id))
    detailLoading.value = false
    if (error || !data) return
    detailUser.value = data
  }

  const detailFields = computed<DetailField[]>(() => {
    const user = detailUser.value
    if (!user) return []
    const roleNames = user.roleNames ?? []
    return [
      {
        key: 'account',
        label: '账号',
        value: user.account,
        type: 'code',
        icon: 'ant-design:user-outlined'
      },
      {
        key: 'realName',
        label: '姓名',
        value: user.realName,
        icon: 'ant-design:idcard-outlined'
      },
      {
        key: 'status',
        label: '状态',
        value: user.status === 1 ? '启用' : '禁用',
        type: 'tag',
        tagType: user.status === 1 ? 'success' : 'error',
        icon: 'ant-design:check-circle-outlined'
      },
      {
        key: 'orgName',
        label: '所属机构',
        value: user.orgName || '-',
        icon: 'ant-design:apartment-outlined'
      },
      {
        key: 'roles',
        label: '角色',
        value: roleNames.length ? roleNames : '-',
        fullRow: true,
        icon: 'ant-design:team-outlined',
        formatter: value => (Array.isArray(value) ? value.join('、') : String(value ?? '-'))
      },
      {
        key: 'phone',
        label: '手机号',
        value: user.phone || '-',
        icon: 'ant-design:phone-outlined'
      },
      {
        key: 'email',
        label: '邮箱',
        value: user.email || '-',
        icon: 'ant-design:mail-outlined'
      },
      {
        key: 'sex',
        label: '性别',
        value: sexLabels[user.sex as number] ?? '未知',
        icon: 'ant-design:man-outlined'
      },
      {
        key: 'jobNum',
        label: '工号',
        value: user.jobNum || '-',
        type: 'code',
        icon: 'ant-design:number-outlined'
      },
      {
        key: 'joinDate',
        label: '入职日期',
        value: user.joinDate ? formatLocalDate(user.joinDate) : '-',
        icon: 'ant-design:calendar-outlined'
      },
      {
        key: 'remark',
        label: '备注',
        value: user.remark || '-',
        fullRow: true,
        icon: 'ant-design:file-text-outlined'
      },
      {
        key: 'createdTime',
        label: '创建时间',
        value: user.createdTime ? formatLocalDateTime(user.createdTime) : '-',
        fullRow: true,
        icon: 'ant-design:clock-circle-outlined'
      }
    ]
  })

  const detailStatusText = computed(() =>
    detailUser.value ? (detailUser.value.status === 1 ? '启用' : '禁用') : undefined
  )

  const detailStatusType = computed(() => {
    if (!detailUser.value) return undefined
    return detailUser.value.status === 1 ? ('success' as const) : ('error' as const)
  })

  onMounted(loadOrgTree)

  function triggerBlobDownload(blob: Blob, fileName: string) {
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = fileName
    anchor.click()
    URL.revokeObjectURL(url)
  }

  const importVisible = ref(false)
  const exporting = ref(false)

  function openImport() {
    importVisible.value = true
  }

  async function downloadImportTemplate() {
    const [blob, error] = await tryRequest(downloadUserImportTemplate())
    if (error || !blob) throw error || new Error('下载模板失败')
    triggerBlobDownload(blob, '用户导入模板.xlsx')
  }

  async function importUserFile(file: File) {
    const [data, error] = await tryRequest(importUsers(file))
    if (error || !data) throw error || new Error('导入失败')
    return data
  }

  function handleImportSuccess() {
    tableRef.value?.reload()
  }

  async function handleExport() {
    exporting.value = true
    const search = tableRef.value?.getSearchParams() ?? {}
    const [blob, error] = await tryRequest(
      exportUsers({
        ...search,
        orgId: selectedOrgId.value
      })
    )
    exporting.value = false
    if (error || !blob) return
    triggerBlobDownload(blob, `用户导出_${Date.now()}.xlsx`)
    message.success('导出已开始下载')
  }

  return {
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
  }
}
