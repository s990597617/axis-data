/**
 * 错误上报器
 *
 * @description
 * 预留一个可插拔的上报通道，后续接入 Sentry / 自研监控时只需替换实现。
 * 默认实现只在控制台输出，避免侵入业务。
 */
export interface ErrorReport {
  /** 错误来源：vue 组件 / 原生 window / promise / 路由 chunk */
  source: 'vue' | 'window' | 'promise' | 'chunk'
  /** 原始错误对象 */
  error: unknown
  /** 额外上下文（组件名、路由、用户等） */
  context?: Record<string, unknown>
}

export type ErrorReporter = (report: ErrorReport) => void

let reporter: ErrorReporter = (report) => {
  // 默认实现：开发环境打印完整信息，生产环境只打 error 关键信息
  if (import.meta.env.DEV) {
    // eslint-disable-next-line no-console
    console.error(`[${report.source}]`, report.error, report.context)
  } else {
    // eslint-disable-next-line no-console
    console.error(`[${report.source}]`, report.error)
  }
}

/** 注册自定义上报器（如 Sentry.captureException） */
export function setErrorReporter(fn: ErrorReporter) {
  reporter = fn
}

/** 内部使用：触发上报 */
export function reportError(report: ErrorReport) {
  try {
    reporter(report)
  } catch {
    // 上报器本身不能抛出
  }
}
