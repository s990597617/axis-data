import { computed, onMounted, ref } from 'vue'

import type { MaterialDto, SaveMaterialInput } from '@/modules/mdm/api/material'
import {
  createMaterial,
  deleteMaterial,
  getMaterialPage,
  previewMaterialCode,
  updateMaterial
} from '@/modules/mdm/api/material'
import type { MaterialCategoryTreeNode } from '@/modules/mdm/api/material-category'
import { getMaterialCategoryTree } from '@/modules/mdm/api/material-category'
import { mdmPermissionCodes } from '@/modules/mdm/permissions/codes'
import {
  formatDateTime,
  renderStatus,
  statusOptions,
  type AxisCrudResource
} from '@/shared/components/axis-crud'
import type { AxisFormOption } from '@/shared/components/axis-form'
import type { AxisTableColumn } from '@/shared/components/axis-table'
import { loadDictOptions, useDict } from '@/shared/composables/use-dict'
import { tryRequest } from '@/shared/http'

// 与 SysDictTypeSeed / MdmDictCodes 对齐
export const mdmDictCodes = {
  materialUnit: 'mdm_material_unit',
  materialSource: 'mdm_material_source'
} as const

const ALL_CATEGORY_KEY = '0'

/** 物料维护页：左树筛选 + AxisCrud 分区弹窗。 */
export function useMaterialPage() {
  const { options: unitOptions } = useDict(mdmDictCodes.materialUnit)
  const { options: sourceOptions } = useDict(mdmDictCodes.materialSource)
  const codeFromRule = ref(true)
  const categoryLoading = ref(false)
  const categoryTreeData = ref<MaterialCategoryTreeNode[]>([])
  const selectedCategoryId = ref(ALL_CATEGORY_KEY)
  const crudRef = ref<{ reload: () => void } | null>(null)

  function mapCategoryOptions(nodes: MaterialCategoryTreeNode[]): AxisFormOption[] {
    return nodes.map(node => ({
      key: node.id,
      label: node.name,
      children: node.children?.length ? mapCategoryOptions(node.children) : undefined
    }))
  }

  const categoryFormOptions = computed(() => mapCategoryOptions(categoryTreeData.value))

  const categoryTreeOptions = computed(() => [
    {
      key: ALL_CATEGORY_KEY,
      label: '全部分类',
      children: categoryFormOptions.value.length ? categoryFormOptions.value : undefined
    }
  ])

  async function loadCategoryTree() {
    categoryLoading.value = true
    const [data] = await tryRequest(getMaterialCategoryTree())
    categoryLoading.value = false
    if (data) categoryTreeData.value = data
  }

  function handleCategorySelect(keys: Array<string | number>) {
    if (!keys.length) return
    selectedCategoryId.value = String(keys[0])
    crudRef.value?.reload()
  }

  onMounted(() => {
    void loadCategoryTree()
  })

  const columns: AxisTableColumn<MaterialDto>[] = [
    { key: 'code', title: '物料编码', search: { field: 'keyword', placeholder: '编码或名称' } },
    { key: 'name', title: '物料名称' },
    { key: 'spec', title: '规格型号', ellipsis: { tooltip: true } },
    { key: 'categoryName', title: '分类', width: 120, ellipsis: { tooltip: true } },
    {
      key: 'unit',
      title: '计量单位',
      width: 110,
      dict: () => loadDictOptions(mdmDictCodes.materialUnit),
      dictRender: true,
      search: true
    },
    {
      key: 'sourceType',
      title: '来源',
      width: 100,
      dict: () => loadDictOptions(mdmDictCodes.materialSource),
      dictRender: true,
      search: true
    },
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
    const [data, err] = await tryRequest(previewMaterialCode(), {
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
    const next: AxisCrudResource<MaterialDto> = {
      title: '',
      entityName: '物料档案',
      permissions: mdmPermissionCodes.material,
      columns,
      formSubtitle: ({ editing, model }) =>
        editing
          ? `编辑物料档案「${String(model.name || '-')}」· 调整规格与来源`
          : '创建物料档案；编码可刷新预览，保存时占用流水',
      actionColumn: {
        width: 120,
        maxInline: 2
      },
      formSections: [
        {
          key: 'basic',
          title: '基础信息',
          icon: 'ant-design:idcard-outlined',
          columns: 2,
          fields: [
            {
              field: 'code',
              label: '物料编码',
              component: 'custom',
              required: true,
              defaultValue: '',
              requiredMessage: '请输入物料编码',
              help: '新增时自动预览下一号，点刷新可重新取预览；手改后按手填编码保存'
            },
            {
              field: 'name',
              label: '物料名称',
              required: true,
              defaultValue: '',
              componentProps: { maxlength: 128, placeholder: '请输入物料名称' }
            },
            {
              field: 'categoryId',
              label: '物料分类',
              component: 'treeSelect',
              options: categoryFormOptions.value,
              defaultValue: null,
              componentProps: {
                filterable: true,
                placeholder: '请选择分类',
                keyField: 'key',
                labelField: 'label',
                childrenField: 'children'
              }
            },
            {
              field: 'spec',
              label: '规格型号',
              defaultValue: '',
              componentProps: { maxlength: 512, placeholder: '请输入规格型号' }
            },
            {
              field: 'unit',
              label: '计量单位',
              component: 'select',
              options: unitOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 'PCS',
              componentProps: { placeholder: '请选择计量单位' }
            },
            {
              field: 'sourceType',
              label: '来源类型',
              component: 'select',
              options: sourceOptions.value as AxisFormOption[],
              required: true,
              defaultValue: 'Buy',
              componentProps: { placeholder: '请选择来源类型' }
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
      request: params =>
        getMaterialPage({
          ...params,
          categoryId:
            selectedCategoryId.value && selectedCategoryId.value !== ALL_CATEGORY_KEY
              ? selectedCategoryId.value
              : undefined
        }),
      create: data => createMaterial(data as unknown as SaveMaterialInput),
      update: (id, data) => updateMaterial(id, data as unknown as SaveMaterialInput),
      remove: deleteMaterial,
      onFormOpen: async ({ editing, model }) => {
        if (editing) {
          codeFromRule.value = false
          return
        }
        const code = await previewNextCode()
        if (code) model.code = code
      },
      beforeSubmit: (data, editing) => {
        const next: Record<string, unknown> = { ...data }
        if (!editing && codeFromRule.value) next.code = ''
        if (!next.categoryId || next.categoryId === ALL_CATEGORY_KEY) next.categoryId = null
        return next
      }
    }
    return next
  })

  return {
    resource,
    previewNextCode,
    markCodeManual,
    categoryLoading,
    categoryTreeOptions,
    selectedCategoryId,
    handleCategorySelect,
    crudRef
  }
}
