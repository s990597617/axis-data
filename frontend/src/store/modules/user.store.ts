import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { Router } from 'vue-router'

import {
  getCurrentUserInfo,
  login as loginApi,
  logout as logoutApi,
  refreshToken as refreshTokenApi,
  type LoginResult,
  type LoginUserInfoResult
} from '@/modules/system/api/author'
import { STORAGE_KEYS } from '@/config/storage-keys'
import { sm2Encrypt } from '@/shared/utils/sm-crypto'
import { usePermissionStore } from '@/store/modules/permission.store'
import { useTabsStore } from '@/store/modules/tabs.store'
import { useNotifyStore } from '@/store/modules/notify.store'

export interface UserInfo {
  id?: string | number
  username?: string
  nickname?: string
  avatar?: string
  phone?: string
  email?: string
  orgId?: number
  orgName?: string
  posName?: string
  accountType?: number
  tenantId?: number | string | null
  roles?: string[]
  permissions?: string[]
  menuTrees?: LoginUserInfoResult['menuTrees']
  [key: string]: unknown
}

export interface LoginPayload {
  username: string
  password: string
  codeId?: string
  code?: string
}

const LEGACY_AUTH_STORAGE_KEYS = [
  // 旧 Pure Admin Store：内部包含 token、refreshToken、userInfo、dictList 等整包状态。
  'pure-user',
  'axis-admin:token',
  'axis-admin:refresh-token',
  'axis-admin:token-expires',
  'axis-admin:user-id',
  'axis-admin:user-info'
] as const

function clearLegacyAuthStorage() {
  try {
    LEGACY_AUTH_STORAGE_KEYS.forEach(key => {
      localStorage.removeItem(key)
      sessionStorage.removeItem(key)
    })
  } catch {
    // Storage 不可用时不影响内存登录态。
  }
}

function readRememberMe(): boolean {
  try {
    return localStorage.getItem(STORAGE_KEYS.REMEMBER_ME) === 'true'
  } catch {
    return false
  }
}

clearLegacyAuthStorage()

function mapUserInfo(remote: LoginUserInfoResult): UserInfo {
  return {
    id: remote.id,
    username: remote.account,
    nickname: remote.realName,
    avatar: remote.avatar,
    phone: remote.phone,
    orgId: remote.orgId,
    orgName: remote.orgName,
    posName: remote.posName,
    accountType: remote.accountType,
    tenantId: remote.tenantId ?? null,
    roles: remote.roles ?? [],
    permissions: remote.buttons ?? [],
    menuTrees: remote.menuTrees ?? []
  }
}

export const useUserStore = defineStore('user', () => {
  const rememberMe = ref(readRememberMe())
  const token = ref('')
  const tokenExpires = ref(0)
  const userInfo = ref<UserInfo | null>(null)
  const sessionInitialized = ref(false)
  const authorizationVersion = ref(0)
  let refreshPromise: Promise<boolean> | null = null
  let sessionPromise: Promise<boolean> | null = null

  const isTokenExpired = computed(
    () => !tokenExpires.value || tokenExpires.value <= Date.now() + 30_000
  )
  const isLogin = computed(() => Boolean(token.value) && !isTokenExpired.value)
  const roles = computed(() => userInfo.value?.roles ?? [])
  const permissions = computed(() => userInfo.value?.permissions ?? [])

  function setRememberMe(value: boolean) {
    rememberMe.value = value
    try {
      if (value) localStorage.setItem(STORAGE_KEYS.REMEMBER_ME, 'true')
      else localStorage.removeItem(STORAGE_KEYS.REMEMBER_ME)
    } catch {
      // 记住我只是 UI 偏好，写入失败不阻塞登录。
    }
  }

  function setToken(value: string) {
    token.value = value
  }

  function setUserInfo(info: UserInfo | null) {
    userInfo.value = info
  }

  function applyLoginResult(result: LoginResult) {
    token.value = result.accessToken
    tokenExpires.value = result.accessTokenExpires
  }

  async function login(payload: LoginPayload): Promise<UserInfo> {
    if (!payload.codeId || !payload.code) throw new Error('请先获取验证码')

    const result = await loginApi({
      account: payload.username,
      password: sm2Encrypt(payload.password),
      codeId: payload.codeId,
      code: payload.code,
      rememberMe: rememberMe.value
    })

    applyLoginResult(result)
    return await fetchUserInfo()
  }

  async function fetchUserInfo(): Promise<UserInfo> {
    const remote = await getCurrentUserInfo()
    const info = mapUserInfo(remote)
    setUserInfo(info)
    sessionInitialized.value = true
    authorizationVersion.value += 1
    return info
  }

  async function refresh(): Promise<boolean> {
    if (refreshPromise) return refreshPromise

    refreshPromise = (async () => {
      try {
        const result = await refreshTokenApi()
        applyLoginResult(result)
        return true
      } catch {
        return false
      } finally {
        refreshPromise = null
      }
    })()

    return refreshPromise
  }

  function clearAuthentication() {
    token.value = ''
    tokenExpires.value = 0
    userInfo.value = null
    sessionInitialized.value = false
  }

  function clearSession(router?: Router) {
    usePermissionStore().reset(router)
    useTabsStore().reset()
    void useNotifyStore().disconnect()
    clearAuthentication()
    clearLegacyAuthStorage()
  }

  async function ensureSession(): Promise<boolean> {
    if (sessionInitialized.value && isLogin.value && userInfo.value) return true
    if (sessionPromise) return sessionPromise

    sessionPromise = (async () => {
      if (!token.value || isTokenExpired.value) {
        const refreshed = await refresh()
        if (!refreshed) {
          clearAuthentication()
          return false
        }
      }

      try {
        await fetchUserInfo()
        return true
      } catch {
        clearAuthentication()
        return false
      }
    })().finally(() => {
      sessionPromise = null
    })

    return sessionPromise
  }

  async function logout(router?: Router) {
    try {
      await logoutApi()
    } finally {
      clearSession(router)
    }
  }

  return {
    token,
    tokenExpires,
    userInfo,
    rememberMe,
    sessionInitialized,
    authorizationVersion,
    isTokenExpired,
    isLogin,
    roles,
    permissions,
    setToken,
    setUserInfo,
    setRememberMe,
    login,
    fetchUserInfo,
    refresh,
    ensureSession,
    clearSession,
    logout
  }
})
