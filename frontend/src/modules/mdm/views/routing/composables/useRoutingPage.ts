import { computed, ref } from 'vue'

import type { ProcessOptionDto } from '@/modules/mdm/api/process'
import { listEnabledProcesses } from '@/modules/mdm/api/process'
import type { RoutingDto, RoutingStepDto, SaveRoutingInput, SaveRoutingStepInput } from '@/modules/mdm/api/routing'
import {
  createRouting,
  deleteRouting,
  getRouting,
  getRoutingPage,
  previewRoutingCode,
  updateRouting
} from '@/modules/mdm/api/routing'
import { mdmPermissionCodes } from '@/modules/mdm/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'

function emptyStep(seq = 10): SaveRoutingStepInput {
  return {
    seq,
    processId: '',
    standardMinutes: 0,
    isReportPoint: true,
    remark: ''
  }
}

function toSaveStep(step: RoutingStepDto): SaveRoutingStepInput {
  return {
    seq: step.seq,
    processId: step.processId ? String(step.processId) : '',
    standardMinutes: step.standardMinutes ?? 0,
    isReportPoint: step.isReportPoint !== false,
    remark: step.remark ?? ''
  }
}

function mergeProcessOptions(options: ProcessOptionDto[], steps: RoutingStepDto[]) {
  const map = new Map(options.map(item => [String(item.id), item]))
  for (const step of steps) {
    const id = String(step.processId ?? '')
    if (!id || map.has(id)) continue
    map.set(id, {
      id,
      code: step.processCode ?? '',
      name: step.processName ?? ''
    })
  }
  return [...map.values()]
}

/** 工艺路线维护页：列表 + AxisCrud 分区弹窗。 */
export function useRoutingPage() {
  const codeFromRule = ref(true)
  const processOptions = ref<ProcessOptionDto[]>([])

  const columns: AxisTableColumn<RoutingDto>[] = [
    { key: 'code', title: '路线编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '路线名称' },
    { key: 'stepCount', title: '工序数', width: 90 },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'orderNo', title: '排序', width: 80 },
    { key: 'remark', title: '备注', ellipsis: { tooltip: true } },
    { key: 'createdTime', title: '创建时间', width: 170, render: row => formatDateTime(row.createdTime) }
  ]

  async function previewNextCode() {
    const [data, err] = await tryRequest(previewRoutingCode(), {
      fallbackMessage: '预览编码失败'
    })
    if (err || !data?.code) return undefined
    codeFromRule.value = true
    return data.code
  }

  function markCodeManual() {
    codeFromRule.value = false
  }

  async function loadProcessOptions(extraSteps: RoutingStepDto[] = []) {
    const [data, err] = await tryRequest(listEnabledProcesses(), {
      fallbackMessage: '加载工序失败'
    })
    processOptions.value = mergeProcessOptions(err ? [] : (data ?? []), extraSteps)
  }

  const resource = computed(() => {
    const next: AxisCrudResource<RoutingDto> = {
      title: '',
      entityName: '工艺路线',
      permissions: mdmPermissionCodes.routing,
      columns,
      formWidth: 960,
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑工艺路线「${String(model.name || '-')}」`
          : '创建工艺路线；编码可刷新预览，保存时占用流水',
      actionColumn: {
        width: 120,
        maxInline: 2
      },
      formSections: [
        {
          key: 'basic',
          title: '基础信息',
          icon: 'ant-design:node-index-outlined',
          columns: 2,
          fields: [
            {
              field: 'code',
              label: '路线编码',
              component: 'custom',
              required: true,
              defaultValue: '',
              requiredMessage: '请输入路线编码',
              help: '新增时自动预览下一号，点刷新可重新取预览；手改后按手填编码保存'
            },
            {
              field: 'name',
              label: '路线名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '如机加路线、组装路线' }
            }
          ]
        },
        {
          key: 'steps',
          title: '工序行',
          icon: 'ant-design:cluster-outlined',
          columns: 1,
          fields: [
            {
              field: 'steps',
              label: '工序',
              component: 'custom',
              fullWidth: true,
              defaultValue: [emptyStep()],
              help: '同一工序可出现多次；删除路线会一并清除工序行'
            }
          ]
        },
        {
          key: 'settings',
          title: '其他设置',
          icon: 'ant-design:setting-outlined',
          columns: 2,
          fields: [
            {
              field: 'status',
              label: '状态',
              component: 'radio',
              options: statusOptions,
              required: true,
              defaultValue: 1
            },
            {
              field: 'orderNo',
              label: '排序',
              component: 'inputNumber',
              required: true,
              defaultValue: 100,
              componentProps: { min: 0, max: 9999, placeholder: '请输入排序' }
            },
            {
              field: 'remark',
              label: '备注',
              component: 'textarea',
              fullWidth: true,
              defaultValue: '',
              componentProps: {
                maxlength: 256,
                showCount: true,
                rows: 2,
                placeholder: '请输入备注'
              }
            }
          ]
        }
      ],
      request: getRoutingPage,
      create: data => createRouting(data as unknown as SaveRoutingInput),
      update: (id, data) => updateRouting(id, data as unknown as SaveRoutingInput),
      remove: deleteRouting,
      onFormOpen: async ({ editing, model, id }) => {
        if (editing) {
          codeFromRule.value = false
          const routingId = id || String(model.id ?? '')
          if (!routingId) return
          const [detail, err] = await tryRequest(getRouting(routingId), {
            fallbackMessage: '加载路线详情失败'
          })
          const steps = err ? [] : (detail?.steps ?? [])
          await loadProcessOptions(steps)
          model.steps = steps.length ? steps.map(toSaveStep) : [emptyStep()]
          return
        }
        await loadProcessOptions()
        model.steps = [emptyStep()]
        const code = await previewNextCode()
        if (code) model.code = code
      },
      beforeSubmit: (data, editing) => {
        const steps = Array.isArray(data.steps) ? (data.steps as SaveRoutingStepInput[]) : []
        return {
          ...data,
          code: !editing && codeFromRule.value ? '' : data.code,
          steps: steps.map(item => ({
            seq: Number(item.seq) || 0,
            processId: item.processId ? String(item.processId) : '0',
            standardMinutes: Number(item.standardMinutes) || 0,
            isReportPoint: item.isReportPoint !== false,
            remark: item.remark
          }))
        }
      }
    }
    return next
  })

  return { resource, previewNextCode, markCodeManual, processOptions }
}
