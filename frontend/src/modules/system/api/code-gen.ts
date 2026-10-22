import { request } from '@/shared/http'

/** 代码生成状态。 */
export interface CodeGenStatusDto {
  enabled: boolean
  allowInProduction: boolean
  isProduction: boolean
  canGenerate: boolean
  repoRoot: string
  message: string
}

/** 预览字段。 */
export interface CodeGenFieldDto {
  name: string
  csType: string
  tsType: string
  label: string
}

/** 文件动作（含完整预览内容）。 */
export interface CodeGenFileActionDto {
  path: string
  action: string
  detail?: string | null
  content?: string | null
  language?: string
}

/** 预览 / 生成结果。 */
export interface CodeGenResultDto {
  area: string
  module: string
  cnName: string
  routePrefix: string
  fieldSource: string
  dryRun: boolean
  fields: CodeGenFieldDto[]
  actions: CodeGenFileActionDto[]
  notes: string[]
}

/** 生成入参。 */
export interface GenerateModuleInput {
  module: string
  cnName: string
  area: string
  routeKey?: string
  entity?: string
  table?: string
  fromTable?: string
  fromEntity?: string
  force?: boolean
  backendOnly?: boolean
  frontendOnly?: boolean
  noMenu?: boolean
}

export function getCodeGenStatus() {
  return request.get<CodeGenStatusDto>({ url: '/sys/code-gen/status' })
}

export function listCodeGenTables() {
  return request.get<string[]>({ url: '/sys/code-gen/tables' })
}

export function listCodeGenEntities() {
  return request.get<string[]>({ url: '/sys/code-gen/entities' })
}

export function previewCodeGen(data: GenerateModuleInput) {
  return request.post<CodeGenResultDto>({ url: '/sys/code-gen/preview', data })
}

export function generateCodeGen(data: GenerateModuleInput) {
  return request.post<CodeGenResultDto>({ url: '/sys/code-gen/generate', data })
}
