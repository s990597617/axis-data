import { request } from '@/shared/http'

/** 字典类型列表数据。 */
export interface DictTypeDto extends Record<string, unknown> {
  id: string
  name: string
  code: string
  isBuiltin: boolean
  isTenant: boolean
  orderNo: number
  status: number
  remark?: string
  createdTime: string
}

/** 字典类型保存数据。 */
export interface SaveDictTypeInput {
  name: string
  code: string
  isTenant: boolean
  orderNo: number
  status: number
  remark?: string
}

/** 字典数据项。 */
export interface DictDataDto extends Record<string, unknown> {
  id: string
  dictTypeId: string
  label: string
  value: string
  tagStyle?: string
  listClass?: string
  isDefault: boolean
  orderNo: number
  status: number
  remark?: string
}

/** 字典数据项保存数据。 */
export type SaveDictDataInput = Omit<DictDataDto, 'id'>

/** 业务侧缓存字典项（按 code 读取）。 */
export interface DictCacheItemDto {
  label: string
  value: string
  tagStyle?: string
  listClass?: string
  isDefault: boolean
  orderNo: number
}

/** AxisTable / 下拉可用的字典选项。 */
export interface DictSelectOption {
  label: string
  value: string | number
  tagStyle?: string
  listClass?: string
  isDefault?: boolean
  orderNo?: number
  [key: string]: unknown
}

export function getDictTypePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/dict/type/page', params })
}

export function createDictType(data: SaveDictTypeInput) {
  return request.post<void>({ url: '/sys/dict/type', data })
}

export function updateDictType(id: string, data: SaveDictTypeInput) {
  return request.put<void>({ url: `/sys/dict/type/${id}`, data })
}

export function deleteDictType(id: string) {
  return request.delete<void>({ url: `/sys/dict/type/${id}` })
}

export function getDictDataList(dictTypeId: string, params: Record<string, unknown> = {}) {
  return request.get<DictDataDto[]>({
    url: '/sys/dict/data/list',
    params: { ...params, dictTypeId }
  })
}

export function createDictData(data: SaveDictDataInput) {
  return request.post<void>({ url: '/sys/dict/data', data })
}

export function updateDictData(id: string, data: SaveDictDataInput) {
  return request.put<void>({ url: `/sys/dict/data/${id}`, data })
}

export function deleteDictData(id: string) {
  return request.delete<void>({ url: `/sys/dict/data/${id}` })
}

/**
 * 按字典类型编码读取启用项（后端走缓存）。
 * 供业务下拉与列表 value→label 翻译。
 */
export function getDictItems(code: string) {
  return request.get<DictCacheItemDto[]>({
    url: '/sys/dict/items',
    params: { code }
  })
}
