import { computed, onMounted, reactive, ref, watch } from 'vue'

import {
  generateCodeGen,
  getCodeGenStatus,
  listCodeGenEntities,
  listCodeGenTables,
  previewCodeGen,
  type CodeGenFileActionDto,
  type CodeGenResultDto,
  type CodeGenStatusDto,
  type GenerateModuleInput
} from '@/modules/system/api/code-gen'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { message } from '@/shared/utils/message'

export type FieldSourceMode = 'default' | 'table' | 'entity'

export function useCodeGenPage() {
  const permissions = systemPermissionCodes.codeGen

  const status = ref<CodeGenStatusDto | null>(null)
  const tables = ref<string[]>([])
  const entities = ref<string[]>([])
  const loadingMeta = ref(false)
  const submitting = ref(false)
  const result = ref<CodeGenResultDto | null>(null)
  const fieldSource = ref<FieldSourceMode>('default')
  const showAdvanced = ref(false)
  const selectedPath = ref<string | null>(null)

  const form = reactive({
    module: '',
    cnName: '',
    area: 'Demo',
    routeKey: '',
    entity: '',
    table: '',
    fromTable: null as string | null,
    fromEntity: null as string | null,
    force: false,
    backendOnly: false,
    frontendOnly: false,
    noMenu: false
  })

  const canRun = computed(() => !!status.value?.canGenerate)

  const statusTone = computed(() => {
    if (!status.value) return 'muted'
    return status.value.canGenerate ? 'ok' : 'warn'
  })

  const statusLabel = computed(() => {
    if (!status.value) return '检查中'
    return status.value.canGenerate ? '已启用' : '不可用'
  })

  const tableOptions = computed(() => tables.value.map((t) => ({ label: t, value: t })))
  const entityOptions = computed(() => entities.value.map((t) => ({ label: t, value: t })))

  const fieldSourceOptions = [
    { label: '默认五字段', value: 'default' as const },
    { label: '数据库表', value: 'table' as const },
    { label: 'Domain 实体', value: 'entity' as const }
  ]

  const previewableActions = computed(() =>
    (result.value?.actions ?? []).filter((a) => !!a.content && a.action !== 'SKIP')
  )

  const skipActions = computed(() =>
    (result.value?.actions ?? []).filter((a) => a.action === 'SKIP' || !a.content)
  )

  const fileList = computed(() => [...previewableActions.value, ...skipActions.value])

  const selectedFile = computed<CodeGenFileActionDto | null>(() => {
    if (!result.value) return null
    const list = fileList.value
    if (!list.length) return null
    return list.find((a) => a.path === selectedPath.value) ?? list[0] ?? null
  })

  const actionSummary = computed(() => {
    const actions = result.value?.actions ?? []
    const count = (name: string) => actions.filter((a) => a.action === name).length
    return {
      create: count('CREATE'),
      overwrite: count('OVERWRITE'),
      merge: count('MERGE'),
      skip: count('SKIP'),
      previewable: previewableActions.value.length,
      total: actions.length
    }
  })

  watch(
    () => result.value,
    (r) => {
      if (!r) {
        selectedPath.value = null
        return
      }
      const first = r.actions.find((a) => !!a.content && a.action !== 'SKIP')
      selectedPath.value = first?.path ?? r.actions[0]?.path ?? null
    }
  )

  async function loadMeta() {
    loadingMeta.value = true
    try {
      status.value = await getCodeGenStatus()
      if (!status.value.canGenerate) return

      const [tableResult, entityResult] = await Promise.allSettled([
        listCodeGenTables(),
        listCodeGenEntities()
      ])

      if (tableResult.status === 'fulfilled') {
        tables.value = tableResult.value
      } else {
        tables.value = []
        message.warning(
          `库表列表加载失败：${tableResult.reason instanceof Error ? tableResult.reason.message : '未知错误'}`
        )
      }

      if (entityResult.status === 'fulfilled') {
        entities.value = entityResult.value
        if (entityResult.value.length === 0) {
          message.warning('未扫到 Domain 实体，请先执行：dotnet build src/Axis.Domain')
        }
      } else {
        entities.value = []
        message.warning(
          `实体列表加载失败：${entityResult.reason instanceof Error ? entityResult.reason.message : '未知错误'}`
        )
      }
    } catch (e) {
      message.error(e instanceof Error ? e.message : '加载代码生成状态失败')
    } finally {
      loadingMeta.value = false
    }
  }

  function buildInput(): GenerateModuleInput {
    const input: GenerateModuleInput = {
      module: form.module.trim(),
      cnName: form.cnName.trim(),
      area: form.area.trim() || 'Demo',
      routeKey: form.routeKey.trim() || undefined,
      entity: form.entity.trim() || undefined,
      table: form.table.trim() || undefined,
      force: form.force,
      backendOnly: form.backendOnly,
      frontendOnly: form.frontendOnly,
      noMenu: form.noMenu
    }

    if (fieldSource.value === 'table' && form.fromTable) {
      input.fromTable = form.fromTable
      input.fromEntity = undefined
    } else if (fieldSource.value === 'entity' && form.fromEntity) {
      input.fromEntity = form.fromEntity
      input.fromTable = undefined
    } else {
      input.fromTable = undefined
      input.fromEntity = undefined
    }

    return input
  }

  async function run(mode: 'preview' | 'generate') {
    if (!form.module.trim() || !form.cnName.trim()) {
      message.warning('请填写模块名与中文名')
      return
    }
    if (fieldSource.value === 'table' && !form.fromTable) {
      message.warning('请选择数据库表')
      return
    }
    if (fieldSource.value === 'entity' && !form.fromEntity) {
      message.warning('请选择实体')
      return
    }

    submitting.value = true
    try {
      const input = buildInput()
      result.value =
        mode === 'preview' ? await previewCodeGen(input) : await generateCodeGen(input)
      message.success(
        mode === 'preview'
          ? `预览完成，共 ${previewableActions.value.length} 个可查看文件`
          : '生成完成，请编译验证'
      )
    } catch (e) {
      message.error(e instanceof Error ? e.message : '操作失败')
    } finally {
      submitting.value = false
    }
  }

  function selectFile(path: string) {
    selectedPath.value = path
  }

  async function copySelected() {
    const content = selectedFile.value?.content
    if (!content) {
      message.warning('当前文件无可复制内容')
      return
    }
    try {
      await navigator.clipboard.writeText(content)
      message.success('已复制到剪贴板')
    } catch {
      message.error('复制失败')
    }
  }

  onMounted(() => {
    void loadMeta()
  })

  return {
    actionSummary,
    canRun,
    copySelected,
    entityOptions,
    fieldSource,
    fieldSourceOptions,
    fileList,
    form,
    loadingMeta,
    permissions,
    previewableActions,
    result,
    run,
    selectFile,
    selectedFile,
    showAdvanced,
    status,
    statusLabel,
    statusTone,
    submitting,
    tableOptions,
    reload: loadMeta
  }
}
