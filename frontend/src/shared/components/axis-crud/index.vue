<script setup lang="ts">
  import { computed, nextTick, reactive, ref } from 'vue'
  import { NButton, NSpace } from 'naive-ui'

  import {
    AxisForm,
    type AxisFormExposed,
    type AxisFormSchema,
    type AxisFormSection
  } from '@/shared/components/axis-form'
  import {
    AxisTable,
    type AxisTableAction,
    type AxisTableRowAction
  } from '@/shared/components/axis-table'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { Icon } from '@/shared/icons'
  import { isHandledHttpError, tryRequest } from '@/shared/http'
  import { formatLocalDate } from '@/shared/utils/date'
  import { message } from '@/shared/utils/message'

  import type { AxisCrudField, AxisCrudResource, AxisCrudRow } from './types'

  defineOptions({ name: 'AxisCrudPage' })

  const props = defineProps<{
    resource: AxisCrudResource<any>
  }>()

  /* ============ 状态 & 引用 ============ */

  const tableRef = ref<AxisTableAction<AxisCrudRow> | null>(null)
  const formRef = ref<AxisFormExposed | null>(null)
  const formVisible = ref(false)
  const editing = ref(false)
  const editId = ref<string>()
  const submitting = ref(false)
  const flatRows = ref<AxisCrudRow[]>([])
  // 树表受控展开；异步加载后 defaultExpandAll 无效，需主动写入
  const expandedRowKeys = ref<Array<string | number>>([])
  const form = reactive<Record<string, unknown>>({})

  /* ============ 视图模型 ============ */

  const formTitle = computed(() =>
    editing.value ? `编辑${props.resource.entityName}` : `新增${props.resource.entityName}`
  )

  const formSubtitle = computed(() => {
    const subtitle = props.resource.formSubtitle
    if (typeof subtitle === 'function') {
      return subtitle({ editing: editing.value, model: form })
    }
    return subtitle
  })

  // 叶子不留空 children，避免 Naive 误显示展开箭头
  function pruneEmptyChildren(rows: AxisCrudRow[]): AxisCrudRow[] {
    return rows.map(row => {
      const children = row.children?.length ? pruneEmptyChildren(row.children) : undefined
      return { ...row, children }
    })
  }

  // 把后端平铺节点转换为表格树，并保留因筛选失去父节点的命中项
  function buildRowTree(rows: AxisCrudRow[]): AxisCrudRow[] {
    const nodeMap = new Map<string, AxisCrudRow>()
    rows.forEach(row => nodeMap.set(row.id, { ...row, children: [] }))

    const roots: AxisCrudRow[] = []
    nodeMap.forEach(node => {
      const parentId = String(node.parentId ?? '0')
      const parent = parentId !== '0' ? nodeMap.get(parentId) : undefined
      if (parent && parent.id !== node.id) parent.children!.push(node)
      else roots.push(node)
    })
    return pruneEmptyChildren(roots)
  }

  // 收集有子节点的 key，用于默认全展开
  function collectExpandableKeys(rows: AxisCrudRow[]): string[] {
    const keys: string[] = []
    rows.forEach(row => {
      if (!row.children?.length) return
      keys.push(row.id)
      keys.push(...collectExpandableKeys(row.children))
    })
    return keys
  }

  const treeTableProps = computed(() => {
    if (!props.resource.tree) return undefined
    return {
      expandedRowKeys: expandedRowKeys.value,
      onUpdateExpandedRowKeys: (keys: Array<string | number>) => {
        expandedRowKeys.value = keys
      }
    }
  })

  // 把当前树数据转换为树选择可识别的父级选项
  function buildTreeOptions(rows: AxisCrudRow[]): Array<Record<string, unknown>> {
    return rows.map(row => ({
      key: row.id,
      label: String(row.title ?? row.name ?? row.id),
      children: row.children?.length ? buildTreeOptions(row.children) : undefined
    }))
  }

  const parentOptions = computed(() => [
    { key: '0', label: '顶级节点' },
    ...buildTreeOptions(buildRowTree(flatRows.value))
  ])

  // 分区优先；无分区时回退扁平 fields
  const flatFields = computed<AxisCrudField[]>(() => {
    if (props.resource.formSections?.length) {
      return props.resource.formSections.flatMap(section => section.fields)
    }
    return props.resource.fields ?? []
  })

  function toFormSchema(field: AxisCrudField): AxisFormSchema {
    return {
      field: field.field,
      label: field.label,
      component: field.component,
      required: field.required,
      options: field.options === 'tree' ? parentOptions.value : field.options,
      fullWidth: field.fullWidth,
      span: field.span,
      help: field.help,
      componentProps: field.componentProps,
      ifShow: field.ifShow ? context => field.ifShow!(context.model) : undefined,
      disabled: field.disabled
        ? context => field.disabled!(context.model, editing.value)
        : undefined
    }
  }

  const formSchemas = computed<AxisFormSchema[]>(() =>
    props.resource.formSections?.length ? [] : flatFields.value.map(toFormSchema)
  )

  const formSections = computed<AxisFormSection[]>(() =>
    (props.resource.formSections ?? []).map(section => ({
      key: section.key,
      title: section.title,
      icon: section.icon,
      description: section.description,
      columns: section.columns,
      schemas: section.fields.map(toFormSchema)
    }))
  )

  const allowCreate = computed(() => {
    const rule = props.resource.canCreate
    if (typeof rule === 'function') return rule()
    return rule !== false
  })

  const rowActions = computed<AxisTableRowAction<AxisCrudRow>[]>(() => [
    ...(props.resource.leadingActions ?? []),
    {
      key: 'edit',
      label: '编辑',
      icon: 'ant-design:edit-outlined',
      color: '#f0a020',
      permissions: [props.resource.permissions.edit],
      ifShow: row => (props.resource.canEdit ? props.resource.canEdit(row) : true),
      onClick: row => openForm(row)
    },
    ...(props.resource.extraActions ?? []),
    {
      key: 'delete',
      label: '删除',
      icon: 'ant-design:delete-outlined',
      type: 'error',
      permissions: [props.resource.permissions.delete],
      ifShow: row => (props.resource.canDelete ? props.resource.canDelete(row) : true),
      confirm: row =>
        `确认删除${props.resource.entityName}「${String(row.name ?? row.title ?? row.label ?? row.code ?? '')}」？`,
      onClick: row => handleDelete(row)
    }
  ])

  /* ============ 交互操作 ============ */

  // 记录最新列表数据；树形资源在此完成平铺到树的转换并默认展开
  function afterRequest(rows: AxisCrudRow[]) {
    flatRows.value = rows
    if (!props.resource.tree) return rows
    const tree = buildRowTree(rows)
    expandedRowKeys.value = collectExpandableKeys(tree)
    return tree
  }

  // 重建表单默认值，避免上一次编辑数据残留
  function resetForm() {
    Object.keys(form).forEach(key => delete form[key])
    flatFields.value.forEach(field => {
      form[field.field] = field.defaultValue ?? null
    })
  }

  // 打开新增或编辑弹窗；日期字符串转为本地时间戳供日期组件回显
  async function openForm(row?: AxisCrudRow) {
    resetForm()
    editing.value = Boolean(row)
    editId.value = row?.id

    if (row) {
      flatFields.value.forEach(field => {
        const value = row[field.field]
        form[field.field] =
          field.component === 'date' && typeof value === 'string'
            ? new Date(value).getTime()
            : (value ?? field.defaultValue ?? null)
      })
      // 主键不在表单字段里，编辑回填详情等仍需要。
      form.id = row.id
      // 乐观锁令牌不进表单字段，但仍需随编辑提交回传。
      if (row.version !== undefined && row.version !== null) {
        form.version = row.version
      }
    }
    formVisible.value = true
    await nextTick()
    formRef.value?.restoreValidation()
    await props.resource.onFormOpen?.({
      editing: editing.value,
      model: form,
      id: editId.value
    })
  }

  // 只提取资源声明字段，日期以本地日历日期提交；编辑时附带 version
  async function buildSubmitData() {
    const data: Record<string, unknown> = {}
    flatFields.value.forEach(field => {
      const value = form[field.field]
      data[field.field] =
        field.component === 'date' && typeof value === 'number' ? formatLocalDate(value) : value
    })
    if (editing.value && form.version !== undefined && form.version !== null) {
      data.version = form.version
    }
    return props.resource.beforeSubmit
      ? await props.resource.beforeSubmit(data, editing.value)
      : data
  }

  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      // 字段下方已展示校验信息，勿再抛到全局错误提示
      return
    }

    submitting.value = true
    try {
      const data = await buildSubmitData()
      const operation =
        editing.value && editId.value
          ? props.resource.update(editId.value, data)
          : props.resource.create(data)
      const [, error] = await tryRequest(operation)
      if (error) return

      message.success(editing.value ? '更新成功' : '新增成功')
      formVisible.value = false
      await tableRef.value?.reload()
    } catch (error) {
      if (!isHandledHttpError(error)) {
        message.error(error instanceof Error && error.message ? error.message : '提交失败')
      }
    } finally {
      submitting.value = false
    }
  }

  async function handleDelete(row: AxisCrudRow) {
    const [, error] = await tryRequest(props.resource.remove(row.id))
    if (error) return
    message.success('删除成功')
    await tableRef.value?.reload()
  }

  /* ============ 对外暴露 ============ */

  defineExpose({
    reload: () => tableRef.value?.reload(),
    open: openForm
  })
</script>

<template>
  <div class="axis-crud-page">
    <AxisTable
      ref="tableRef"
      :title="resource.title"
      :columns="resource.columns"
      :request="resource.request"
      :after-request="afterRequest"
      :pagination="resource.pagination ?? { pageSize: 20 }"
      :action-column="{
        width: resource.actionColumn?.width ?? 130,
        maxInline: resource.actionColumn?.maxInline ?? 1,
        actions: rowActions
      }"
      :table-props="treeTableProps"
      row-key="id"
    >
      <template #toolbar>
        <NSpace :size="8" align="center">
          <NButton
            v-if="allowCreate"
            v-permission="resource.permissions.create"
            type="primary"
            size="small"
            @click="openForm()"
          >
            <template #icon>
              <Icon icon="ant-design:plus-outlined" />
            </template>
            新增{{ resource.entityName }}
          </NButton>
          <slot name="toolbar-extra" />
        </NSpace>
      </template>
    </AxisTable>

    <FormDialog
      v-model="formVisible"
      layout="sectioned"
      :title="formTitle"
      :subtitle="formSubtitle"
      icon="ant-design:form-outlined"
      :width="resource.formWidth ?? 760"
      :confirm-loading="submitting"
      confirm-text="保存"
      :mask-closable="!submitting"
      @confirm="handleSubmit"
    >
      <AxisForm
        ref="formRef"
        :model="form"
        :schemas="formSchemas"
        :sections="formSections"
        :columns="2"
        :disabled="submitting"
        :before-placement="resource.formBeforePlacement ?? 'block'"
      >
        <template v-for="(_, name) in $slots" :key="name" #[name]="slotData">
          <slot :name="name" v-bind="{ ...(slotData || {}), editing }" />
        </template>
      </AxisForm>
    </FormDialog>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
