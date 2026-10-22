import { request } from '@/shared/http'

export interface UploadFileResult {
  fileId: string
  originalName: string
  url: string
  relativePath: string
  size: number
  contentType: string
}

/** 文件管理列表项。 */
export interface FileDto extends Record<string, unknown> {
  id: string
  fileId: string
  originalName: string
  storedName: string
  relativePath: string
  url?: string
  size: number
  contentType: string
  extension?: string
  module?: string
  provider?: string
  createdTime: string
  creator?: string
}

/** 文件模块分类统计。 */
export interface FileModuleStatDto {
  module: string
  count: number
}

/** 上传文件；module 用于归档子目录（如 avatar）。 */
export function uploadFile(file: File, module = 'common') {
  const data = new FormData()
  data.append('file', file)
  data.append('module', module)
  // 不手动设 Content-Type，由浏览器带 multipart boundary
  return request.post<UploadFileResult>({
    url: '/sys/file/upload',
    data,
    headers: { 'Content-Type': false as unknown as string }
  })
}

/** 分页查询文件。 */
export function getFilePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/file/page', params })
}

/** 按业务模块统计文件数量。 */
export function getFileModules() {
  return request.get<FileModuleStatDto[]>({ url: '/sys/file/modules' })
}

/** 删除文件。 */
export function deleteFile(fileId: string) {
  return request.delete<void>({ url: `/sys/file/${fileId}` })
}

/** 下载文件流（返回 Blob，不走业务 JSON 拆包）。 */
export function downloadFile(fileId: string) {
  return request.get<Blob>(
    {
      url: `/sys/file/${fileId}/download`,
      responseType: 'blob'
    },
    {
      isTransformResponse: false
    }
  )
}
