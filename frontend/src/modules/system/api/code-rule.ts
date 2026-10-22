import { request } from '@/shared/http'

/** 编码规则列表与编辑回显。 */
export interface CodeRuleDto extends Record<string, unknown> {
  id: string
  code: string
  name: string
  prefix?: string
  dateFormat?: string
  serialLength: number
  resetCycle: number
  template: string
  checkAlgo: number
  sourceType: number
  reverseRegex?: string
  priority: number
  isBuiltin: boolean
  status: number
  remark?: string
  createdTime: string
}

/** 编码规则保存入参。 */
export interface SaveCodeRuleInput {
  code: string
  name: string
  prefix?: string
  dateFormat?: string
  serialLength: number
  resetCycle: number
  template: string
  checkAlgo: number
  sourceType: number
  reverseRegex?: string
  priority: number
  status: number
  remark?: string
}

/** 模板解析段。 */
export interface CodeSegmentDto {
  name: string
  value: string
}

/** 实时预览结果。 */
export interface CodePreviewDto {
  sample: string
  segments: CodeSegmentDto[]
  length: number
  resetCycle: number
  checkAlgo: number
  sourceType: number
  generatedRegex: string
}

export function getCodeRulePage(params: Record<string, unknown>) {
  return request.get<unknown>({ url: '/sys/code-rule/page', params })
}

export function createCodeRule(data: SaveCodeRuleInput) {
  return request.post<void>({ url: '/sys/code-rule', data })
}

export function updateCodeRule(id: string, data: SaveCodeRuleInput) {
  return request.put<void>({ url: '/sys/code-rule/' + id, data })
}

export function deleteCodeRule(id: string) {
  return request.delete<void>({ url: '/sys/code-rule/' + id })
}

export function previewCodeRule(data: SaveCodeRuleInput) {
  return request.post<CodePreviewDto>({ url: '/sys/code-rule/preview', data })
}

export function generateCodeRuleRegex(data: SaveCodeRuleInput) {
  return request.post<{ pattern: string }>({ url: '/sys/code-rule/regex', data })
}
