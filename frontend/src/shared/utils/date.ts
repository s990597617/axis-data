import dayjs from 'dayjs'
import customParseFormat from 'dayjs/plugin/customParseFormat'
import relativeTime from 'dayjs/plugin/relativeTime'
import 'dayjs/locale/zh-cn'

dayjs.extend(customParseFormat)
dayjs.extend(relativeTime)
dayjs.locale('zh-cn')

export { dayjs }

/**
 * 将日期值格式化为本地日历日期（YYYY-MM-DD）。
 *
 * 使用 dayjs 按本地时区取值，避免 `toISOString()` 在 UTC+ 时区把本地零点算成前一天。
 */
export function formatLocalDate(value: number | string | Date | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.format('YYYY-MM-DD') : ''
}

/**
 * 格式化为本地日期时间（YYYY-MM-DD HH:mm:ss）。
 */
export function formatLocalDateTime(value: number | string | Date | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.format('YYYY-MM-DD HH:mm:ss') : ''
}

/**
 * 相对当前时间的友好文案（如「3 小时前」）；无效日期返回空串。
 */
export function formatRelativeTime(value: number | string | Date | null | undefined): string {
  if (value === null || value === undefined || value === '') return ''
  const parsed = dayjs(value)
  return parsed.isValid() ? parsed.fromNow() : ''
}
