import { request } from '@/shared/http'

/** 缓存分组。 */
export interface CachePrefixGroupDto {
  code: string
  name: string
  prefix: string
  description: string
  keyCount: number
  allowClear: boolean
}

/** 缓存运行概览。 */
export interface CacheOverviewDto {
  cacheType: string
  globalPrefix: string
  separator: string
  allowFlushAll: boolean
  maxKeysPerQuery: number
  groups: CachePrefixGroupDto[]
}

/** 缓存键列表项。 */
export interface CacheKeyItemDto {
  key: string
  expirationSeconds: number
}

/** 按前缀查询结果。 */
export interface CacheKeyListDto {
  prefix: string
  total: number
  truncated: boolean
  items: CacheKeyItemDto[]
}

/** 缓存条目详情。 */
export interface CacheEntryDto {
  key: string
  exists: boolean
  expirationSeconds: number
  valueType?: string | null
  valuePreview?: string | null
  previewTruncated: boolean
}

/** 清理结果。 */
export interface CacheClearResultDto {
  success: boolean
  matchedCount: number
  message: string
}

export function getCacheOverview() {
  return request.get<CacheOverviewDto>({ url: '/sys/cache/overview' })
}

export function listCacheKeys(params: { prefix: string; take?: number }) {
  return request.get<CacheKeyListDto>({ url: '/sys/cache/keys', params })
}

export function getCacheEntry(params: { key: string }) {
  return request.get<CacheEntryDto>({ url: '/sys/cache/entry', params })
}

export function removeCacheKey(params: { key: string }) {
  return request.delete<void>({ url: '/sys/cache/key', params })
}

export function removeCacheByPrefix(params: { prefix: string }) {
  return request.delete<CacheClearResultDto>({ url: '/sys/cache/prefix', params })
}

export function flushCache() {
  return request.delete<CacheClearResultDto>({ url: '/sys/cache/flush' })
}
