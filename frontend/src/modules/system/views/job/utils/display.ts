export const yesNoOptions = [
  { label: '是', value: true },
  { label: '否', value: false }
]

export const jobExecStatusOptions = [
  { label: '失败', value: 0 },
  { label: '成功', value: 1 }
]

export const jobTriggerTypeOptions = [
  { label: '自动', value: 1 },
  { label: '手动', value: 2 }
]

const WEEKDAY_LABELS: Record<string, string> = {
  SUN: '周日',
  MON: '周一',
  TUE: '周二',
  WED: '周三',
  THU: '周四',
  FRI: '周五',
  SAT: '周六',
  '1': '周日',
  '2': '周一',
  '3': '周二',
  '4': '周三',
  '5': '周四',
  '6': '周五',
  '7': '周六'
}

/** 将 Quartz Cron 转为友好中文；无法识别时回退原表达式。 */
export function describeCron(cron?: string | null): string {
  const raw = cron?.trim()
  if (!raw) return '-'

  const parts = raw.split(/\s+/)
  if (parts.length < 6 || parts.length > 7) return raw

  const sec = parts[0]!
  const min = parts[1]!
  const hour = parts[2]!
  const dayOfMonth = parts[3]!
  const month = parts[4]!
  const dayOfWeek = parts[5]!

  const everySec = readStep(sec)
  if (
    everySec &&
    isAny(min) &&
    isAny(hour) &&
    isAnyDom(dayOfMonth) &&
    isAny(month) &&
    isAnyDow(dayOfWeek)
  ) {
    return everySec === 1 ? '每秒执行一次' : `每隔 ${everySec} 秒执行一次`
  }

  if (!isZero(sec)) return raw

  const everyMin = readStep(min)
  if (
    everyMin &&
    isAny(hour) &&
    isAnyDom(dayOfMonth) &&
    isAny(month) &&
    isAnyDow(dayOfWeek)
  ) {
    return everyMin === 1 ? '每分钟执行一次' : `每隔 ${everyMin} 分钟执行一次`
  }

  if (isZero(min)) {
    const everyHour = readStep(hour)
    if (everyHour && isAnyDom(dayOfMonth) && isAny(month) && isAnyDow(dayOfWeek)) {
      return everyHour === 1 ? '每小时执行一次' : `每隔 ${everyHour} 小时执行一次`
    }
  }

  const fixedMin = readNumber(min)
  const fixedHour = readNumber(hour)
  if (fixedMin == null || fixedHour == null || !isAny(month)) return raw

  const time = `${pad(fixedHour)}:${pad(fixedMin)}`
  if (isAnyDom(dayOfMonth) && isAnyDow(dayOfWeek)) {
    return `每天 ${time} 执行一次`
  }
  if (dayOfMonth === '?' && !dayOfWeek.includes('-') && !dayOfWeek.includes(',')) {
    const weekday = describeWeekday(dayOfWeek)
    if (weekday) return `每${weekday} ${time} 执行一次`
  }
  if (dayOfMonth === '?' && (dayOfWeek.includes('-') || dayOfWeek.includes(','))) {
    const weekdays = describeWeekdayRange(dayOfWeek)
    if (weekdays) return `${weekdays} ${time} 执行`
  }
  if (dayOfWeek === '?' && readNumber(dayOfMonth) != null) {
    return `每月 ${readNumber(dayOfMonth)} 日 ${time} 执行一次`
  }

  return raw
}

/** 友好文案 + 原始表达式（用于 tooltip / 副文案）。 */
export function formatCronDisplay(cron?: string | null): { label: string; raw: string } {
  const raw = cron?.trim() || '-'
  const label = describeCron(raw)
  return { label, raw }
}

function isAny(part: string) {
  return part === '*'
}

function isAnyDom(part: string) {
  return part === '*' || part === '?'
}

function isAnyDow(part: string) {
  return part === '*' || part === '?'
}

function isZero(part: string) {
  return part === '0'
}

function readNumber(part: string): number | null {
  if (!/^\d+$/.test(part)) return null
  return Number(part)
}

/** 解析星号/0 加斜杠步进，例如星号斜杠5、0斜杠10。 */
function readStep(part: string): number | null {
  const match = part.match(/^(?:\*|0)\/(\d+)$/)
  if (!match) return null
  const step = Number(match[1])
  return step > 0 ? step : null
}

function pad(value: number) {
  return String(value).padStart(2, '0')
}

function describeWeekday(part: string): string | null {
  const key = part.toUpperCase()
  return WEEKDAY_LABELS[key] ?? null
}

function describeWeekdayRange(part: string): string | null {
  const upper = part.toUpperCase()
  if (upper === 'MON-FRI') return '工作日'
  if (upper.includes(',')) {
    const labels = upper
      .split(',')
      .map(item => describeWeekday(item.trim()))
      .filter((item): item is string => Boolean(item))
    return labels.length ? labels.join('、') : null
  }
  if (upper.includes('-')) {
    const [start, end] = upper.split('-')
    const startLabel = describeWeekday(start ?? '')
    const endLabel = describeWeekday(end ?? '')
    if (startLabel && endLabel) return `${startLabel}至${endLabel}`
  }
  return describeWeekday(upper)
}
