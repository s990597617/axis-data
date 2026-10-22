// 格式化 TTL 秒数
export function formatTtl(seconds: number | null | undefined) {
  if (seconds == null || seconds < 0) return '永久 / 未知'
  if (seconds < 60) return `${seconds.toFixed(0)} 秒`
  if (seconds < 3600) return `${(seconds / 60).toFixed(1)} 分钟`
  if (seconds < 86400) return `${(seconds / 3600).toFixed(1)} 小时`
  return `${(seconds / 86400).toFixed(1)} 天`
}

// 尝试美化 JSON / 普通文本预览
export function formatValuePreview(raw: string | null | undefined) {
  const text = (raw ?? '').trim()
  if (!text) return ''
  try {
    return JSON.stringify(JSON.parse(text), null, 2)
  } catch {
    return text
  }
}
