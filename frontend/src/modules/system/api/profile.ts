import { request } from '@/shared/http'

export interface ProfileDto {
  id: number | string
  account: string
  realName: string
  nickName?: string | null
  avatar?: string | null
  sex: number
  birthday?: string | null
  phone?: string | null
  email?: string | null
  remark?: string | null
  jobNum?: string | null
  joinDate?: string | null
  accountType: number
  orgId: number | string
  orgName?: string | null
  posId: number | string
  posName?: string | null
  roleNames: string[]
  status: number
  lastLoginIp?: string | null
  lastLoginTime?: string | null
  createdTime: string
}

export interface UpdateProfileInput {
  realName: string
  nickName?: string | null
  avatar?: string | null
  sex: number
  birthday?: string | null
  phone?: string | null
  email?: string | null
  remark?: string | null
}

export interface ChangePasswordInput {
  /** 当前密码 SM2 密文 */
  oldPassword: string
  /** 新密码 SM2 密文 */
  newPassword: string
  /** 确认新密码 SM2 密文 */
  confirmPassword: string
}

/** 获取当前登录用户个人中心资料 */
export function getProfile() {
  return request.get<ProfileDto>({ url: '/sys/profile' })
}

/** 更新当前登录用户基础资料 */
export function updateProfile(data: UpdateProfileInput) {
  return request.put({ url: '/sys/profile', data })
}

/** 修改当前登录用户密码 */
export function changePassword(data: ChangePasswordInput) {
  return request.post({ url: '/sys/profile/change-password', data })
}
