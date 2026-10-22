import { computed, ref } from 'vue'

import type { ProcessDto, SaveProcessInput } from '@/modules/mdm/api/process'
import {
  createProcess,
  deleteProcess,
  getProcessPage,
  previewProcessCode,
  updateProcess
} from '@/modules/mdm/api/process'
import { mdmPermissionCodes } from '@/modules/mdm/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { tryRequest } from '@/shared/http'

/** 工序维护页：列表 + AxisCrud 分区弹窗。 */
export function useProcessPage() {
  const codeFromRule = ref(true)

  const columns: AxisTableColumn<ProcessDto>[] = [
    { key: 'code', title: '工序编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '工序名称' },
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
    const [data, err] = await tryRequest(previewProcessCode(), {
      fallbackMessage: '预览编码失败'
    })
    if (err || !data?.code) return undefined
    codeFromRule.value = true
    return data.code
  }

  function markCodeManual() {
    codeFromRule.value = false
  }

  const resource = computed(() => {
    const next: AxisCrudResource<ProcessDto> = {
      title: '',
      entityName: '工序',
      permissions: mdmPermissionCodes.process,
      columns,
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑工序「${String(model.name || '-')}」`
          : '创建工序；编码可刷新预览，保存时占用流水',
      actionColumn: {
        width: 120,
        maxInline: 2
      },
      formSections: [
        {
          key: 'basic',
          title: '基础信息',
          icon: 'ant-design:cluster-outlined',
          columns: 2,
          fields: [
            {
              field: 'code',
              label: '工序编码',
              component: 'custom',
              required: true,
              defaultValue: '',
              requiredMessage: '请输入工序编码',
              help: '新增时自动预览下一号，点刷新可重新取预览；手改后按手填编码保存'
            },
            {
              field: 'name',
              label: '工序名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 64, placeholder: '如车削、铣削、组装' }
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
      request: getProcessPage,
      create: data => createProcess(data as unknown as SaveProcessInput),
      update: (id, data) => updateProcess(id, data as unknown as SaveProcessInput),
      remove: deleteProcess,
      onFormOpen: async ({ editing, model }) => {
        if (editing) {
          codeFromRule.value = false
          return
        }
        const code = await previewNextCode()
        if (code) model.code = code
      },
      beforeSubmit: (data, editing) => {
        if (!editing && codeFromRule.value) return { ...data, code: '' }
        return data
      }
    }
    return next
  })

  return { resource, previewNextCode, markCodeManual }
}
