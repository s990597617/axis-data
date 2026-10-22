<script setup lang="ts">
  import { computed, nextTick, reactive, ref } from 'vue'
  import { NButton, NSpace, NSteps, NStep } from 'naive-ui'

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
  const wizardStepIndex = ref(0)

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

  const formSteps = computed(() => props.resource.formSteps ?? [])
  const hasWizard = computed(() => formSteps.value.length > 0)
  const isDrawerWizard = computed(
    () => hasWizard.value && (props.resource.formPresentation ?? 'modal') === 'drawer'
  )
  const isLastWizardStep = computed(
    () => !hasWizard.value || wizardStepIndex.value >= formSteps.value.length - 1
  )
  const wizardCurrentStep = computed(() => formSteps.value[wizardStepIndex.value])

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

  // 分区优先；无分区时回退扁平 fields（始终包含全部字段，供默认值/提交）
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

  const allFormSections = computed<AxisFormSection[]>(() =>
    (props.resource.formSections ?? []).map(section => ({
      key: section.key,
      title: section.title,
      icon: section.icon,
      description: section.description,
      columns: section.columns,
      schemas: section.fields.map(toFormSchema)
    }))
  )

  // 向导模式下只渲染当前步骤对应分区
  const formSections = computed<AxisFormSection[]>(() => {
    if (!hasWizard.value) return allFormSections.value
    const stepKey = wizardCurrentStep.value?.key
    if (!stepKey) return allFormSections.value
    return allFormSections.value.filter(section => section.key === stepKey)
  })

  const currentStepFieldKeys = computed(() => {
    const section = formSections.value[0]
    if (!section) return [] as string[]
    return section.schemas.map(schema => schema.field)
  })

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
    wizardStepIndex.value = 0

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

  async function validateCurrentStep() {
    const keys = new Set(currentStepFieldKeys.value)
    if (!keys.size) return true
    try {
      await formRef.value?.validate(
        undefined,
        rule => {
          const key = String((rule as { key?: string }).key ?? '')
          return !key || keys.has(key)
        }
      )
      return true
    } catch {
      return false
    }
  }

  async function goWizardNext() {
    if (!hasWizard.value || isLastWizardStep.value) return
    const ok = await validateCurrentStep()
    if (!ok) return
    wizardStepIndex.value += 1
    await nextTick()
    formRef.value?.restoreValidation()
  }

  function goWizardPrev() {
    if (!hasWizard.value || wizardStepIndex.value <= 0) return
    wizardStepIndex.value -= 1
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
      :presentation="resource.formPresentation ?? 'modal'"
      :title="formTitle"
      :subtitle="formSubtitle"
      icon="ant-design:form-outlined"
      :width="resource.formWidth ?? 760"
      :extra-class="resource.formExtraClass ?? ''"
      :confirm-loading="submitting"
      confirm-text="保存"
      :show-confirm="!hasWizard"
      :mask-closable="!submitting"
      @confirm="handleSubmit"
    >
      <div v-if="hasWizard" class="axis-crud-wizard" :class="{ 'axis-crud-wizard--split': isDrawerWizard }">
        <aside v-if="isDrawerWizard" class="axis-crud-wizard__rail">
          <div class="axis-crud-wizard__rail-title">配置步骤</div>
          <n-steps class="axis-crud-wizard__steps" vertical size="small" :current="wizardStepIndex + 1">
            <n-step
              v-for="(step, index) in formSteps"
              :key="step.key"
              :title="step.title"
              :status="index < wizardStepIndex ? 'finish' : index === wizardStepIndex ? 'process' : 'wait'"
            />
          </n-steps>
        </aside>
        <div v-else class="axis-crud-wizard__top">
          <n-steps class="axis-crud-wizard__steps" size="small" :current="wizardStepIndex + 1">
            <n-step
              v-for="(step, index) in formSteps"
              :key="step.key"
              :title="step.title"
              :status="index < wizardStepIndex ? 'finish' : index === wizardStepIndex ? 'process' : 'wait'"
            />
          </n-steps>
        </div>
        <div class="axis-crud-wizard__panel">
          <div class="axis-crud-wizard__panel-head">
            <div class="axis-crud-wizard__panel-kicker">
              步骤 {{ wizardStepIndex + 1 }} / {{ formSteps.length }}
            </div>
            <div class="axis-crud-wizard__panel-title">{{ wizardCurrentStep?.title }}</div>
          </div>
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
        </div>
      </div>

      <AxisForm
        v-else
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

      <template v-if="hasWizard" #footer="{ cancel }">
        <n-button :disabled="submitting" @click="cancel">取消</n-button>
        <n-button
          v-if="wizardStepIndex > 0"
          :disabled="submitting"
          @click="goWizardPrev"
        >
          上一步
        </n-button>
        <n-button
          v-if="!isLastWizardStep"
          type="primary"
          :disabled="submitting"
          @click="goWizardNext"
        >
          下一步
        </n-button>
        <n-button
          v-else
          type="primary"
          :loading="submitting"
          @click="handleSubmit"
        >
          保存
        </n-button>
      </template>
    </FormDialog>
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';

  .axis-crud-wizard {
    &--split {
      display: flex;
      gap: 20px;
      align-items: stretch;
      min-height: min(70vh, 640px);
    }

    &__rail {
      flex: 0 0 152px;
      padding: 16px 14px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 75%, transparent);
      border-radius: 10px;
      background: var(--axis-card-color);
    }

    &__rail-title {
      margin-bottom: 14px;
      color: var(--axis-text-color-3);
      font-size: 12px;
      font-weight: 600;
      letter-spacing: 0.04em;
    }

    &__top {
      margin-bottom: 16px;
      padding: 14px 16px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 80%, transparent);
      border-radius: 10px;
      background: var(--axis-card-color);
    }

    &__panel {
      flex: 1;
      min-width: 0;
      padding: 18px 20px 20px;
      border: 1px solid color-mix(in srgb, var(--axis-border-color) 75%, transparent);
      border-radius: 10px;
      background: var(--axis-card-color);
    }

    &__panel-head {
      margin-bottom: 18px;
      padding-bottom: 14px;
      border-bottom: 1px dashed color-mix(in srgb, var(--axis-border-color) 85%, transparent);
    }

    &__panel-kicker {
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 18px;
    }

    &__panel-title {
      margin-top: 4px;
      color: var(--axis-text-color-1);
      font-size: 16px;
      font-weight: 600;
      letter-spacing: 0.01em;
    }

    &__steps {
      :deep(.n-step-content) {
        min-height: 44px;
      }
    }
  }
</style>