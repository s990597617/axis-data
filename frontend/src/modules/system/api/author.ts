import { request } from '@/shared/http'

/** 登录请求参数（Web 端，需要图形验证码） */
export interface LoginPayload {
  /** 账号 */
  account: string
  /** 密码（若后端配置了 SM2 私钥则需为 SM2 加密 HEX，否则为明文） */
  password: string
  /** 验证码 Id（来自 {@link getCaptcha}） */
  codeId: string
  /** 验证码字符 */
  code: string
  /** 是否在关闭浏览器后保持登录 */
  rememberMe: boolean
}

/** 登录 / 刷新 Token 响应 */
export interface LoginResult {
  /** 访问 Token */
  accessToken: string
  /** Access Token 过期时间（毫秒时间戳） */
  accessTokenExpires: number
}

/** 图形验证码响应 */
export interface CaptchaResult {
  /** 验证码 Id（登录时原样回传） */
  id: string
  /** 图片 base64（已含 data: 前缀，前端可直接赋值给 img.src） */
  img: string
}

/** 菜单 Meta（与 vue-router 路由 meta 对齐） */
export interface MenuMeta {
  title?: string
  icon?: string
  isIframe?: boolean
  isLink?: string
  isHide?: boolean
  isKeepAlive?: boolean
  isAffix?: boolean
}

/** 菜单树节点 */
export interface MenuTreeNode {
  id: number
  pid: number
  /** 0 目录 / 1 菜单 / 2 按钮 */
  type: number
  name?: string
  path?: string
  component?: string
  permission?: string
  redirect?: string
  orderNo?: number
  status?: number
  remark?: string
  meta: MenuMeta
  children?: MenuTreeNode[]
}

/** 当前登录用户完整信息 */
export interface LoginUserInfoResult {
  id: number
  account: string
  realName?: string
  phone?: string
  /** 账号类型：999 = 超级管理员、888 = 系统管理员、777 = 普通用户 */
  accountType: number
  /** 所属租户 Id；超级管理员可为空 */
  tenantId?: number | string | null
  avatar?: string
  introduction?: string
  address?: string
  signature?: string
  orgId: number
  orgName?: string
  orgType?: string
  posName?: string
  /** 后端返回的真实角色编码 */
  roles: string[]
  /** 按钮级权限标识，超级管理员为 `["*"]` */
  buttons: string[]
  /** 菜单路由树（已剔除按钮节点） */
  menuTrees: MenuTreeNode[]
}

/** 获取图形验证码（登录页使用） */
export async function getCaptcha(): Promise<CaptchaResult> {
  const res = await request.get<CaptchaResult>(
    { url: '/sys/author/captcha' },
    { withToken: false, skipAuthRefresh: true }
  )
  // 后端 Lazy.Captcha 返回裸 base64，统一在此处补 data URI 前缀，避免每个调用方各自处理
  return {
    ...res,
    img: res.img && !res.img.startsWith('data:') ? `data:image/png;base64,${res.img}` : res.img
  }
}

/** Web 端登录：账号 + 密码 + 图形验证码 */
export function login(payload: LoginPayload) {
  return request.post<LoginResult>(
    { url: '/sys/author/login', data: payload },
    { withToken: false, skipAuthRefresh: true }
  )
}

/** 使用 HttpOnly Cookie 无参换取新 AccessToken（一次性轮换） */
export function refreshToken() {
  return request.post<LoginResult>(
    { url: '/sys/author/refresh-token' },
    { withToken: false, skipAuthRefresh: true }
  )
}

/** 撤销 Refresh Token 并删除 HttpOnly Cookie */
export function logout() {
  return request.post<void>(
    { url: '/sys/author/logout' },
    { withToken: false, skipAuthRefresh: true }
  )
}

/** 获取当前登录用户完整信息（含组织、按钮权限、菜单树） */
export function getCurrentUserInfo() {
  return request.get<LoginUserInfoResult>({ url: '/sys/author/current-user-info' })
}
