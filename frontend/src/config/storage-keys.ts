/**
 * 全局存储 key 注册表
 *
 * @description
 * 所有 localStorage / sessionStorage 的 key 集中在此声明，
 * 统一 `axis-admin:` 前缀，便于：
 * - 变更命名空间时只改一处
 * - 登出 / 重置时批量清理：`Object.values(STORAGE_KEYS).forEach(k => localStorage.removeItem(k))`
 * - 审计所有持久化数据入口
 *
 * @remarks
 * - 新增持久化项请统一在此声明，**不要**在业务代码里散落硬编码。
 * - `sessionStorage` 与 `localStorage` 都走同一命名空间，但分别持久化。
 */

/** 命名空间前缀 */
const NS = 'axis-admin'

/** 拼接命名空间 */
const k = (name: string) => `${NS}:${name}`

export const STORAGE_KEYS = {
  /** 布局配置（`app.store`） */
  LAYOUT_CONFIG: k('layout-config'),
  /** 主题配置（`theme.store`） */
  THEME_CONFIG: k('theme-config'),
  /** 标签页列表（`tabs.store`，sessionStorage） */
  TABS: k('tabs'),
  /** 记住我选项；认证 Token 本身只保存在内存或 HttpOnly Cookie。 */
  REMEMBER_ME: k('remember-me'),
  /** 最近一次 chunk 失败重载时间戳（sessionStorage） */
  CHUNK_RELOAD_AT: k('chunk-reload-at')
} as const

export type StorageKey = (typeof STORAGE_KEYS)[keyof typeof STORAGE_KEYS]
