import { computed, onMounted, reactive, ref } from 'vue'
import type { FormRules } from 'naive-ui'

import { uploadFile } from '@/modules/system/api/file'
import { getProfile, updateProfile, type ProfileDto } from '@/modules/system/api/profile'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { tryRequest } from '@/shared/http'
import { formatLocalDate } from '@/shared/utils/date'
import { message } from '@/shared/utils/message'
import { useUserStore } from '@/store/modules/user.store'

// 账号类型枚举文案（AccountTypeEnum）
const accountTypeLabels: Record<number, string> = {
  777: '普通用户',
  888: '系统管理员',
  999: '超级管理员'
}

// StatusEnum.Enable
const STATUS_ENABLE = 1

function toDateValue(value?: string | null) {
  if (!value) return null
  const time = Date.parse(value)
  return Number.isNaN(time) ? null : time
}

function toDatePayload(value: number | null) {
  if (value == null) return null
  return formatLocalDate(value) || null
}

export function useProfilePage() {
  const userStore = useUserStore()

  /* ============ 状态 ============ */

  const loading = ref(false)
  const savingBasic = ref(false)
  const uploadingAvatar = ref(false)
  const profile = ref<ProfileDto | null>(null)

  const basicForm = reactive({
    realName: '',
    nickName: '',
    avatar: '',
    sex: 0,
    birthday: null as number | null,
    phone: '',
    email: '',
    remark: ''
  })

  const basicRules: FormRules = {
    realName: [{ required: true, message: '请输入真实姓名', trigger: ['input', 'blur'] }],
    phone: [
      {
        validator: (_rule, value: string) => {
          if (!value) return true
          return /^1\d{10}$/.test(value)
        },
        message: '手机号格式不正确',
        trigger: ['input', 'blur']
      }
    ],
    email: [
      {
        validator: (_rule, value: string) => {
          if (!value) return true
          return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
        },
        message: '邮箱格式不正确',
        trigger: ['input', 'blur']
      }
    ]
  }

  /* ============ 派生展示 ============ */

  const displayName = computed(
    () => profile.value?.realName || profile.value?.nickName || profile.value?.account || '未命名'
  )

  const accountTypeLabel = computed(
    () => accountTypeLabels[profile.value?.accountType as number] ?? '普通用户'
  )

  const isAccountEnabled = computed(() => (profile.value?.status ?? STATUS_ENABLE) === STATUS_ENABLE)

  const statusTagLabel = computed(() => (isAccountEnabled.value ? '在职' : '停用'))

  const statusValueLabel = computed(() => (isAccountEnabled.value ? '正常' : '停用'))

  const statusHint = computed(() => (isAccountEnabled.value ? '状态良好' : '账号已停用'))

  const avatarSrc = computed(() => basicForm.avatar || profile.value?.avatar || '')

  const createdTimeText = computed(() => formatLocalDate(profile.value?.createdTime) || '-')

  const primaryRoleName = computed(() => profile.value?.roleNames?.[0] || accountTypeLabel.value)

  /* ============ 数据加载 ============ */

  function fillBasicForm(data: ProfileDto) {
    basicForm.realName = data.realName || ''
    basicForm.nickName = data.nickName || ''
    basicForm.avatar = data.avatar || ''
    basicForm.sex = data.sex ?? 0
    basicForm.birthday = toDateValue(data.birthday)
    basicForm.phone = data.phone || ''
    basicForm.email = data.email || ''
    basicForm.remark = data.remark || ''
  }

  async function loadProfile() {
    loading.value = true
    const [data, error] = await tryRequest(getProfile())
    loading.value = false
    if (error || !data) return

    profile.value = data
    fillBasicForm(data)
  }

  /* ============ 交互 ============ */

  async function copyAccountId() {
    const text = profile.value?.account
    if (!text) return
    try {
      await navigator.clipboard.writeText(text)
      message.success('账号已复制')
    } catch {
      message.warning('复制失败，请手动选择')
    }
  }

  async function handleAvatarChange(event: Event) {
    const input = event.target as HTMLInputElement
    const file = input.files?.[0]
    input.value = ''
    if (!file) return

    if (!file.type.startsWith('image/')) {
      message.warning('请上传图片文件')
      return
    }
    if (file.size > 5 * 1024 * 1024) {
      message.warning('头像不能超过 5MB')
      return
    }

    uploadingAvatar.value = true
    const [uploaded, uploadError] = await tryRequest(uploadFile(file, 'avatar'))
    if (uploadError || !uploaded?.url) {
      uploadingAvatar.value = false
      return
    }

    const nextAvatar = uploaded.url
    const [, saveError] = await tryRequest(
      updateProfile({
        realName: (basicForm.realName || profile.value?.realName || '').trim(),
        nickName: basicForm.nickName.trim() || null,
        avatar: nextAvatar,
        sex: basicForm.sex,
        birthday: toDatePayload(basicForm.birthday),
        phone: basicForm.phone.trim() || null,
        email: basicForm.email.trim() || null,
        remark: basicForm.remark.trim() || null
      })
    )
    uploadingAvatar.value = false
    if (saveError) return

    basicForm.avatar = nextAvatar
    if (profile.value) profile.value.avatar = nextAvatar
    message.success('头像已更新')
    await userStore.fetchUserInfo()
  }

  async function handleSaveBasic() {
    savingBasic.value = true
    const [, error] = await tryRequest(
      updateProfile({
        realName: basicForm.realName.trim(),
        nickName: basicForm.nickName.trim() || null,
        avatar: basicForm.avatar || null,
        sex: basicForm.sex,
        birthday: toDatePayload(basicForm.birthday),
        phone: basicForm.phone.trim() || null,
        email: basicForm.email.trim() || null,
        remark: basicForm.remark.trim() || null
      })
    )
    savingBasic.value = false
    if (error) return

    message.success('基础信息已保存')
    await loadProfile()
    await userStore.fetchUserInfo()
  }

  onMounted(() => {
    void loadProfile()
  })

  return {
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
    permissions: systemPermissionCodes.profile,
    primaryRoleName,
    profile,
    savingBasic,
    statusHint,
    statusTagLabel,
    statusValueLabel,
    uploadingAvatar
  }
}
