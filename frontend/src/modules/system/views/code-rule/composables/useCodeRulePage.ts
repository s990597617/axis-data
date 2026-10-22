import { computed } from 'vue'

import type { CodeRuleDto, SaveCodeRuleInput } from '@/modules/system/api/code-rule'
import {
  createCodeRule,
  deleteCodeRule,
  getCodeRulePage,
  updateCodeRule
} from '@/modules/system/api/code-rule'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { loadDictOptions, useDict } from '@/shared/composables/use-dict'

// 与 SysDictTypeSeed 双轨对齐
export const codeRuleDictCodes = {
  resetCycle: 'code_reset_cycle',
  checkAlgo: 'code_check_algo',
  sourceType: 'code_source_type',
  status: 'common_status'
} as const

const dateFormatOptions: AxisFormOption[] = [
  { label: 'yyyyMMdd', value: 'yyyyMMdd' },
  { label: 'yyyyMM', value: 'yyyyMM' },
  { label: 'yyyy', value: 'yyyy' },
  { label: 'yyMMdd', value: 'yyMMdd' },
  { label: 'yyyy-MM-dd', value: 'yyyy-MM-dd' }
]

/** 编码规则维护页：列表 + AxisCrud 通用分区弹窗。 */
export function useCodeRulePage() {
  const { options: resetCycleOptions } = useDict(codeRuleDictCodes.resetCycle)
  const { options: checkAlgoOptions } = useDict(codeRuleDictCodes.checkAlgo)
  const { options: sourceTypeOptions } = useDict(codeRuleDictCodes.sourceType)
  const { options: statusDictOptions } = useDict(codeRuleDictCodes.status)

  const columns: AxisTableColumn<CodeRuleDto>[] = [
    { key: 'code', title: '规则编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '规则名称' },
    { key: 'prefix', title: '前缀', width: 90 },
    { key: 'template', title: '拼接模板', ellipsis: { tooltip: true } },
    {
      key: 'resetCycle',
      title: '重置周期',
      width: 110,
      dict: () => loadDictOptions(codeRuleDictCodes.resetCycle),
      dictRender: true,
      search: true
    },
    {
      key: 'sourceType',
      title: '来源',
      width: 110,
      dict: () => loadDictOptions(codeRuleDictCodes.sourceType),
      dictRender: true,
      search: true
    },
    {
      key: 'status',
      title: '状态',
      width: 90,
      dict: () => loadDictOptions(codeRuleDictCodes.status),
      search: true,
      render: row => renderStatus(row.status)
    },
    { key: 'priority', title: '优先级', width: 80 },
    { key: 'createdTime', title: '创建时间', width: 170, render: row => formatDateTime(row.createdTime) }
  ]

  const resource = computed(() => {
    const next: AxisCrudResource<CodeRuleDto> = {
      title: '',
      entityName: '编码规则',
      permissions: systemPermissionCodes.codeRule,
      columns,
      formWidth: 1160,
      formBeforePlacement: 'aside',
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑规则「${String(model.name || '-')}」· 调整模板与流水`
          : '创建编码规则并设置前缀、日期、流水与拼接模板',
      actionColumn: {
        width: 120,
        maxInline: 2
      },
      formSections: [
        {
          key: 'basic',
          title: '基础信息',
          icon: 'ant-design:barcode-outlined',
          columns: 2,
          fields: [
            {
              field: 'code',
              label: '规则编码',
              required: true,
              defaultValue: '',
              disabled: model => Boolean(model.isBuiltin),
              componentProps: { maxlength: 64, placeholder: '如 WO、SN-PCBA' }
            },
            {
              field: 'name',
              label: '规则名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '请输入规则名称' }
            },
            {
              field: 'prefix',
              label: '条码前缀',
              defaultValue: '',
              componentProps: { maxlength: 32, placeholder: '对应 {Prefix}' }
            },
            {
              field: 'dateFormat',
              label: '日期格式',
              component: 'select',
              options: dateFormatOptions,
              defaultValue: 'yyyyMMdd',
              componentProps: { filterable: true, tag: true, placeholder: '如 yyyyMMdd' }
            },
            {
              field: 'serialLength',
              label: '流水长度',
              component: 'inputNumber',
              required: true,
              defaultValue: 5,
              componentProps: { min: 1, max: 16, placeholder: '请输入流水长度' }
            },
            {
              field: 'resetCycle',
              label: '重置周期',
              component: 'select',
              options: resetCycleOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 2,
              componentProps: { placeholder: '请选择重置周期' }
            },
            {
              field: 'template',
              label: '拼接模板',
              fullWidth: true,
              required: true,
              defaultValue: '{Prefix}-{Date}-{Serial}',
              componentProps: { maxlength: 128, placeholder: '{Prefix}-{Date}-{Serial}' }
            },
            {
              field: 'isBuiltin',
              label: '内置',
              ifShow: () => false,
              defaultValue: false
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
              field: 'checkAlgo',
              label: '校验位',
              component: 'select',
              options: checkAlgoOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 1,
              componentProps: { placeholder: '请选择校验位' }
            },
            {
              field: 'sourceType',
              label: '来源类型',
              component: 'select',
              options: sourceTypeOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 1,
              componentProps: { placeholder: '请选择来源类型' }
            },
            {
              field: 'priority',
              label: '匹配优先级',
              component: 'inputNumber',
              required: true,
              defaultValue: 100,
              componentProps: { min: 1, max: 9999, placeholder: '请输入优先级' }
            },
            {
              field: 'status',
              label: '状态',
              component: 'radio',
              options: statusDictOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 1
            },
            {
              field: 'reverseRegex',
              label: '反向正则',
              component: 'textarea',
              fullWidth: true,
              defaultValue: '',
              componentProps: {
                maxlength: 512,
                placeholder: '用于扫码解析 / 外来码校验'
              }
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
      request: getCodeRulePage,
      create: data => createCodeRule(toSaveInput(data)),
      update: (id, data) => updateCodeRule(id, toSaveInput(data)),
      remove: deleteCodeRule,
      canDelete: row => !row.isBuiltin
    }
    return next
  })

  return { resource }
}

export function toSaveInput(data: Record<string, unknown>): SaveCodeRuleInput {
  return {
    code: String(data.code ?? ''),
    name: String(data.name ?? ''),
    prefix: (data.prefix as string | undefined) || undefined,
    dateFormat: (data.dateFormat as string | undefined) || undefined,
    serialLength: Number(data.serialLength ?? 5),
    resetCycle: Number(data.resetCycle ?? 2),
    template: String(data.template ?? ''),
    checkAlgo: Number(data.checkAlgo ?? 1),
    sourceType: Number(data.sourceType ?? 1),
    reverseRegex: (data.reverseRegex as string | undefined) || undefined,
    priority: Number(data.priority ?? 100),
    status: Number(data.status ?? 1),
    remark: (data.remark as string | undefined) || undefined
  }
}

// 预览 / 生成正则走同一套入参校验，未填编码名称时用占位值，避免编辑过程中接口 400
export function toPreviewInput(data: Record<string, unknown>): SaveCodeRuleInput {
  const input = toSaveInput(data)
  if (!input.code) input.code = 'PREVIEW'
  if (!input.name) input.name = '预览'
  return input
}
