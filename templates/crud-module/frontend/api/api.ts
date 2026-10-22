import { request } from '@/shared/http'

/** __CnName__列表数据。 */
export interface __Module__Dto extends Record<string, unknown> {
__FIELD_FE_DTO__
}

/** __CnName__保存数据。 */
export interface Save__Module__Input {
__FIELD_FE_SAVE__
}

export function get__Module__Page(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '__RoutePrefix__/page', params })
}

export function create__Module__(data: Save__Module__Input) {
  return request.post<void>({ url: '__RoutePrefix__', data })
}

export function update__Module__(id: string, data: Save__Module__Input) {
  return request.put<void>({ url: `__RoutePrefix__/${id}`, data })
}

export function delete__Module__(id: string) {
  return request.delete<void>({ url: `__RoutePrefix__/${id}` })
}
