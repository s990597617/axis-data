// 字节 → 可读容量
export function formatBytes(bytes?: number | null) {
  if (bytes == null || Number.isNaN(bytes) || bytes < 0) return '-'
  if (bytes === 0) return '0 B'
  const units = ['B', 'KB', 'MB', 'GB', 'TB', 'PB']
  const index = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1)
  const value = bytes / 1024 ** index
  return `${value.toFixed(value >= 100 || index === 0 ? 0 : 2)} ${units[index]}`
}

// 秒 → 可读运行时长
export function formatUptime(seconds?: number | null) {
  if (seconds == null || seconds < 0) return '-'
  const day = Math.floor(seconds / 86400)
  const hour = Math.floor((seconds % 86400) / 3600)
  const minute = Math.floor((seconds % 3600) / 60)
  const second = Math.floor(seconds % 60)
  const parts: string[] = []
  if (day) parts.push(`${day} 天`)
  if (hour || day) parts.push(`${hour} 小时`)
  if (minute || hour || day) parts.push(`${minute} 分`)
  parts.push(`${second} 秒`)
  return parts.join(' ')
}

// 使用率色阶（Naive Progress）
export function usageStatus(usage?: number | null): 'success' | 'warning' | 'error' | 'info' {
  if (usage == null) return 'info'
  if (usage >= 90) return 'error'
  if (usage >= 70) return 'warning'
  return 'success'
}

// 健康等级文案
export function healthLabel(usage?: number | null) {
  if (usage == null) return '未知'
  if (usage >= 90) return '告警'
  if (usage >= 70) return '偏高'
  return '正常'
}

// 健康等级 class 后缀
export function healthTone(usage?: number | null): 'ok' | 'warn' | 'danger' | 'muted' {
  if (usage == null) return 'muted'
  if (usage >= 90) return 'danger'
  if (usage >= 70) return 'warn'
  return 'ok'
}

// 进度条/圆环配色
export function usageColor(usage?: number | null) {
  if (usage == null) return 'var(--axis-text-color-3)'
  if (usage >= 90) return '#d03050'
  if (usage >= 70) return '#f0a020'
  return 'var(--axis-primary-color)'
}
