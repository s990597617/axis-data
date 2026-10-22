import type { FileDto } from '@/modules/system/api/file'

export type FileKind = 'image' | 'video' | 'audio' | 'pdf' | 'excel' | 'word' | 'archive' | 'text' | 'other'

const IMAGE_EXT = new Set(['.png', '.jpg', '.jpeg', '.gif', '.webp', '.bmp', '.svg'])
const VIDEO_EXT = new Set(['.mp4', '.webm', '.ogg', '.mov', '.m4v'])
const AUDIO_EXT = new Set(['.mp3', '.wav', '.flac', '.aac', '.m4a'])
const EXCEL_EXT = new Set(['.xls', '.xlsx', '.csv'])
const WORD_EXT = new Set(['.doc', '.docx'])
const ARCHIVE_EXT = new Set(['.zip', '.rar', '.7z', '.tar', '.gz'])
const TEXT_EXT = new Set(['.txt', '.md', '.json', '.xml', '.log', '.yml', '.yaml'])

function extOf(file: FileDto) {
  const fromField = (file.extension || '').toLowerCase()
  if (fromField) return fromField.startsWith('.') ? fromField : `.${fromField}`
  const name = file.originalName || ''
  const index = name.lastIndexOf('.')
  return index >= 0 ? name.slice(index).toLowerCase() : ''
}

/** 根据 Content-Type / 后缀判断预览类型。 */
export function getFileKind(file: FileDto): FileKind {
  const type = (file.contentType || '').toLowerCase()
  const ext = extOf(file)

  if (type.startsWith('image/') || IMAGE_EXT.has(ext)) return 'image'
  if (type.startsWith('video/') || VIDEO_EXT.has(ext)) return 'video'
  if (type.startsWith('audio/') || AUDIO_EXT.has(ext)) return 'audio'
  if (type.includes('pdf') || ext === '.pdf') return 'pdf'
  if (type.includes('sheet') || type.includes('excel') || EXCEL_EXT.has(ext)) return 'excel'
  if (type.includes('word') || WORD_EXT.has(ext)) return 'word'
  if (type.includes('zip') || type.includes('compressed') || ARCHIVE_EXT.has(ext)) return 'archive'
  if (type.startsWith('text/') || TEXT_EXT.has(ext)) return 'text'
  return 'other'
}

/** 将存储 URL 规范为可通过 Vite 代理访问的相对路径。 */
export function resolveFileUrl(file: FileDto) {
  const relative = (file.relativePath || '').replace(/\\/g, '/').replace(/^\/+/, '')
  if (relative) return `/Uploads/${relative}`

  const raw = (file.url || '').trim()
  if (!raw) return ''

  try {
    const url = new URL(raw, window.location.origin)
    return url.pathname + url.search
  } catch {
    return raw
  }
}

export function formatFileSize(size: number) {
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  return `${(size / 1024 / 1024).toFixed(2)} MB`
}

export function kindIcon(kind: FileKind) {
  switch (kind) {
    case 'image':
      return 'ant-design:file-image-outlined'
    case 'video':
      return 'ant-design:video-camera-outlined'
    case 'audio':
      return 'ant-design:customer-service-outlined'
    case 'pdf':
      return 'ant-design:file-pdf-outlined'
    case 'excel':
      return 'ant-design:file-excel-outlined'
    case 'word':
      return 'ant-design:file-word-outlined'
    case 'archive':
      return 'ant-design:file-zip-outlined'
    case 'text':
      return 'ant-design:file-text-outlined'
    default:
      return 'ant-design:file-outlined'
  }
}

export function kindTone(kind: FileKind) {
  switch (kind) {
    case 'image':
      return 'image'
    case 'video':
      return 'video'
    case 'audio':
      return 'audio'
    case 'pdf':
      return 'pdf'
    case 'excel':
      return 'excel'
    case 'word':
      return 'word'
    case 'archive':
      return 'archive'
    case 'text':
      return 'text'
    default:
      return 'other'
  }
}
